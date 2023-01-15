using Navitski.Crystalized.Model.Engine.ChangesTracking;
using Navitski.Crystalized.Model.Engine.Persistence;
using Navitski.Crystalized.Model.Engine.Scheduling;
using Navitski.Crystalized.Model.Engine.Subscription;

namespace Navitski.Crystalized.Model.Engine;

/// <summary>
///     A domain model which can track all changes happened and provides undo/redo support.
/// </summary>
public class UndoRedoDomainModel : DomainModel
{
    private readonly IStorage _storage;
    private readonly Stack<IWritableModelChanges> _undoStack;
    private readonly Stack<IWritableModelChanges> _redoStack;

    /// <summary>
    ///     Ctor
    /// </summary>
    public UndoRedoDomainModel(
        IStorage storage,
        IEnumerable<IModelShard> modelShards,
        IScheduler scheduler)
        : base(modelShards, scheduler)
    {
        _storage = storage;
        _undoStack = new Stack<IWritableModelChanges>();
        _redoStack = new Stack<IWritableModelChanges>();
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
    /// <param name="path">A path to file</param>
    public async Task Save(string path)
    {
        var changes = _undoStack.Reverse().ToArray();

        if (changes.Any())
        {
            await Save(_storage, path, changes);

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
    ///     Saves model as a whole (if the data should be stored from the scratch)
    /// </summary>
    /// <param name="storage">A storage where a model will be saved</param>
    /// <param name="path">A path to a file</param>
    public async Task SaveAs(string path, IStorage? storage = null)
    {
        await Save(storage ?? _storage, path);

        // TODO(#8): saving operation executes in thread pool
        // and launched by 'async void' methods. If two
        // sequential savings happened, clearing of the stacks
        // can cause data race (just after first save we will clear stack
        // with new changes, made after first save started).
        _undoStack.Clear();
        _redoStack.Clear();

        Changed?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    ///     Loads the model
    /// </summary>
    /// <param name="storage">A storage from which a model will be loaded</param>
    /// <param name="path">A path to a file</param>
    public async Task Load(string path, IStorage? storage = null)
    {
        await Load(storage ?? _storage, path);
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
        _undoStack.Push((IWritableModelChanges)change.Hunk);
        _redoStack.Clear();

        Changed?.Invoke(this, EventArgs.Empty);
    }
}
