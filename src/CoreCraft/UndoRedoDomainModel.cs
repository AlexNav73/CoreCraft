using CoreCraft.ChangesTracking;
using CoreCraft.Exceptions;
using CoreCraft.Persistence;
using CoreCraft.Persistence.History;
using CoreCraft.Scheduling;
using CoreCraft.Subscription;

namespace CoreCraft;

/// <summary>
///     A domain model which can track all changes happened and provides undo/redo support.
/// </summary>
public class UndoRedoDomainModel : DomainModel
{
    private readonly Stack<IModelChanges> _undoStack;
    private readonly Stack<IModelChanges> _redoStack;

    /// <summary>
    ///     Ctor
    /// </summary>
    public UndoRedoDomainModel(
        IEnumerable<IModelShard> modelShards)
        : this(modelShards, new AsyncScheduler())
    {
    }

    /// <summary>
    ///     Ctor
    /// </summary>
    public UndoRedoDomainModel(
        IEnumerable<IModelShard> modelShards,
        IScheduler scheduler)
        : base(modelShards, scheduler)
    {
        _undoStack = new Stack<IModelChanges>();
        _redoStack = new Stack<IModelChanges>();
    }

    /// <summary>
    ///     Raised when model has changed
    /// </summary>
    public event EventHandler? Changed;

    /// <summary>
    ///     Undo stack of model changes
    /// </summary>
    public IReadOnlyCollection<IModelChanges> UndoStack => _undoStack;

    /// <summary>
    ///     Redo stack of model changes
    /// </summary>
    public IReadOnlyCollection<IModelChanges> RedoStack => _redoStack;

    /// <summary>
    ///     Saves changes happened since the last save operation
    /// </summary>
    /// <param name="storage">A storage where a model will be saved</param>
    public async Task Update(IStorage storage, CancellationToken token = default)
    {
        var changes = _undoStack.Reverse().ToArray();

        if (changes.Length != 0)
        {
            await Update(storage, changes, token);
        }
    }

    /// <summary>
    ///     Saves the model's undo/redo history to the provided storage.
    /// </summary>
    /// <param name="storage">A storage to write</param>
    /// <param name="token">Cancellation token</param>
    /// <exception cref="ModelSaveException">Throws when an error occurred while saving model changes history</exception>
    public async Task SaveHistory(IHistoryStorage storage, CancellationToken token = default)
    {
        try
        {
            var changes = _undoStack.Reverse().ToList();
            if (changes.Count > 0)
            {
                await Scheduler.RunParallel(() => storage.Save(changes), token);

                ClearHistory();
            }
        }
        catch (Exception ex)
        {
            throw new ModelSaveException("Model changes saving has failed", ex);
        }
    }

    /// <summary>
    ///     Restores the model's undo/redo history from the provided storage.
    /// </summary>
    /// <param name="storage">A storage to write</param>
    /// <param name="token">Cancellation token</param>
    public async Task RestoreHistory(IHistoryStorage storage, CancellationToken token = default)
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
    public void ClearHistory()
    {
        if (_undoStack.Count > 0 || _redoStack.Count > 0)
        {
            // TODO(#8): saving operation executes in thread pool
            // and launched by 'async void' methods. If two
            // sequential savings happened, clearing of the stacks
            // can cause data race (just after first save we will clear stack
            // with new changes, made after first save started).
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
            await Apply(changes.Invert());

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
            await Apply(changes);

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

    /// <inheritdoc/>
    protected override void OnModelChanged(Change<IModelChanges> change)
    {
        _undoStack.Push(change.Hunk);
        _redoStack.Clear();

        Changed?.Invoke(this, EventArgs.Empty);
    }

#if NET5_0_OR_GREATER
    private async ValueTask<IEnumerable<IModelChanges>> LoadHistory(
#else
    private async Task<IEnumerable<IModelChanges>> LoadHistory(
#endif
        IHistoryStorage storage,
        CancellationToken token = default)
    {
        // TODO: Write explanation why we can use UnsafeModelShards here
        var model = UnsafeModelShards;

        return await Scheduler.Enqueue(() => storage.Load(model), token);
    }
}
