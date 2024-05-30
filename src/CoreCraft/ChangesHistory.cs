using CoreCraft.ChangesTracking;
using CoreCraft.Exceptions;
using CoreCraft.Persistence;
using CoreCraft.Persistence.History;

namespace CoreCraft;

/// <summary>
///     Tracks changes history for a model and provides undo/redo functionality.
/// </summary>
public sealed class ChangesHistory
{
    private readonly DomainModel _model;

    private readonly Stack<IModelChanges> _undoStack;
    private readonly Stack<IModelChanges> _redoStack;

    /// <summary>
    ///     Ctor
    /// </summary>
    public ChangesHistory(DomainModel model)
    {
        _model = model;

        _undoStack = new Stack<IModelChanges>();
        _redoStack = new Stack<IModelChanges>();
    }

    /// <summary>
    ///     Undo stack of model changes
    /// </summary>
    public IReadOnlyCollection<IModelChanges> UndoStack => _undoStack;

    /// <summary>
    ///     Redo stack of model changes
    /// </summary>
    public IReadOnlyCollection<IModelChanges> RedoStack => _redoStack;

    /// <summary>
    ///     Raised when model has changed
    /// </summary>
    public event EventHandler? Changed;

    /// <summary>
    ///     Saves changes happened since the last save operation
    /// </summary>
    /// <param name="token">Cancellation token</param>
    /// <param name="storage">A storage where a model will be saved</param>
    /// <param name="historyStorage">A storage where a model changes history will be saved</param>
    public async Task Update(IStorage storage, IHistoryStorage? historyStorage = null, CancellationToken token = default)
    {
        try
        {
            var changes = _undoStack.Reverse().ToList();
            if (changes.Count > 0)
            {
                var merged = MergeChanges(changes);
                // merge operation can combine actions line Add and Remove, which will cause
                // resulting IModelChanges object contain no changes
                if (merged.HasChanges())
                {
                    await _model.Scheduler.RunParallel(() =>
                    {
                        storage.Update(merged);
                        historyStorage?.Save(changes);
                    }, token);

                    // TODO(#8): saving operation executes in thread pool
                    // and launched by 'async void' methods. If two
                    // sequential savings happened, clearing of the stacks
                    // can cause data race (just after first save we will clear stack
                    // with new changes, made after first save started).
                    Clear();
                }
            }
        }
        catch (Exception ex)
        {
            throw new ModelSaveException("Model update has failed", ex);
        }
    }

    /// <summary>
    ///     Saves the model's undo/redo history to the provided storage.
    /// </summary>
    /// <param name="storage">A storage to write</param>
    /// <param name="token">Cancellation token</param>
    /// <exception cref="ModelSaveException">Throws when an error occurred while saving model changes history</exception>
    public async Task Save(IHistoryStorage storage, CancellationToken token = default)
    {
        try
        {
            var changes = _undoStack.Reverse().ToList();
            if (changes.Count > 0)
            {
                await _model.Scheduler.RunParallel(() => storage.Save(changes), token);

                // TODO(#8): saving operation executes in thread pool
                // and launched by 'async void' methods. If two
                // sequential savings happened, clearing of the stacks
                // can cause data race (just after first save we will clear stack
                // with new changes, made after first save started).
                Clear();
            }
        }
        catch (Exception ex)
        {
            throw new ModelSaveException("Model changes saving has failed", ex);
        }
    }

    /// <summary>
    ///     Loads the model's undo/redo history from the provided storage.
    /// </summary>
    /// <param name="storage">A storage to write</param>
    /// <param name="token">Cancellation token</param>
    public async Task Load(IHistoryStorage storage, CancellationToken token = default)
    {
        if (_undoStack.Count > 0 || _redoStack.Count > 0)
        {
            return;
        }

        var changes = await LoadHistory(storage, token);

        foreach (var change in changes)
        {
            _undoStack.Push(change);
        }
    }

    /// <summary>
    ///     Clears the model's undo/redo history
    /// </summary>
    public void Clear()
    {
        if (_undoStack.Count > 0 || _redoStack.Count > 0)
        {
            _undoStack.Clear();
            _redoStack.Clear();

            Changed?.Invoke(this, EventArgs.Empty);
        }
    }

    /// <summary>
    ///     Undo latest changes
    /// </summary>
    public async Task Undo()
    {
        if (_undoStack.Count > 0)
        {
            var changes = _undoStack.Pop();
            _redoStack.Push(changes);
            await _model.Apply(changes.Invert());

            Changed?.Invoke(this, EventArgs.Empty);
        }
    }

    /// <summary>
    ///     Redo changes which were undone previously
    /// </summary>
    public async Task Redo()
    {
        if (_redoStack.Count > 0)
        {
            var changes = _redoStack.Pop();
            _undoStack.Push(changes);
            await _model.Apply(changes);

            Changed?.Invoke(this, EventArgs.Empty);
        }
    }

    /// <summary>
    ///     History has changes
    /// </summary>
    /// <returns>True - if history has some changes in it</returns>
    public bool HasChanges()
    {
        return _undoStack.Count > 0;
    }

    /// <summary>
    ///     Pushes the provided model changes to the undo stack
    /// </summary>
    /// <param name="change"></param>
    public void Push(IModelChanges change)
    {
        _undoStack.Push(change);
        _redoStack.Clear();

        Changed?.Invoke(this, EventArgs.Empty);
    }

    private Task<IEnumerable<IModelChanges>> LoadHistory(
        IHistoryStorage storage,
        CancellationToken token = default)
    {
        // It is safe there to call UnsafeGetModelShards method, because we will
        // use model shards only to create empty changes frames. We don't care about data
        // stored in the model shard and it's consistency.
        var shards = _model.UnsafeGetModelShards();

        return _model.Scheduler.Enqueue(() => storage.Load(shards), token);
    }

    private static IModelChanges MergeChanges(IReadOnlyList<IModelChanges> changes)
    {
        var merged = changes[0];

        for (var i = 1; i < changes.Count; i++)
        {
            merged = merged.Merge(changes[i]);
        }

        return merged;
    }
}
