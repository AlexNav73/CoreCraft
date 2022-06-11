using Navitski.Crystalized.Model.Engine.ChangesTracking;
using Navitski.Crystalized.Model.Engine.Exceptions;
using Navitski.Crystalized.Model.Engine.Persistence;

namespace Navitski.Crystalized.Model.Engine.History;

/// <summary>
///     A history which can track all changes happened with the model.
/// </summary>
/// <remarks>
///     Each time model changes, a new change is written to the history.
///     Current implementation of a history supports undo/redo stacks and
///     save/load of the model. History is automatically subscribes to the model
///     changes and receives them without any additional work to do. Each time
///     history receive new changes, it writes them to the undo stack. This
///     changes can be popped from the undo stack, inverted and applied to the model
///     reverting the model to the previous version. Next, this changes are placed
///     to the redo stack and can be redone.
/// </remarks>
public sealed class ManualSaveHistory : DisposableBase
{
    private readonly DomainModel _model;
    private readonly IStorage _storage;
    private readonly Stack<IWritableModelChanges> _undoStack;
    private readonly Stack<IWritableModelChanges> _redoStack;

    public ManualSaveHistory(DomainModel model, IStorage storage)
    {
        _model = model;
        _storage = storage;

        _undoStack = new Stack<IWritableModelChanges>();
        _redoStack = new Stack<IWritableModelChanges>();

        _model.ModelChanged += OnModelChanged;
    }

    public event EventHandler? Changed;

    /// <summary>
    ///     Saves changes happened since the last save operation
    /// </summary>
    /// <param name="path">A path to file</param>
    public async Task Save(string path)
    {
        var changes = _undoStack.Reverse().ToArray();

        if (changes.Any())
        {
            await _model.Save(_storage, path, changes);

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
    /// <param name="path">A path to a file</param>
    public async Task SaveAs(string path)
    {
        await _model.Save(_storage, path);

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
    /// <param name="path">A path to a file</param>
    public async Task Load(string path)
    {
        await _model.Load(_storage, path);
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

    private void OnModelChanged(object? sender, ModelChangedEventArgs args)
    {
        _undoStack.Push((IWritableModelChanges)args.Changes);
        Changed?.Invoke(this, EventArgs.Empty);
    }

    protected override void DisposeManagedObjects()
    {
        _model.ModelChanged -= OnModelChanged;
    }
}
