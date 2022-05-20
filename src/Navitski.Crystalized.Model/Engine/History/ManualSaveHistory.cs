using Navitski.Crystalized.Model.Engine.ChangesTracking;
using Navitski.Crystalized.Model.Engine.Exceptions;
using Navitski.Crystalized.Model.Engine.Persistence;

namespace Navitski.Crystalized.Model.Engine.History;

public sealed class ManualSaveHistory : DisposableBase
{
    private readonly BaseModel _model;
    private readonly IStorage _storage;
    private readonly Stack<IWritableModelChanges> _undoStack;
    private readonly Stack<IWritableModelChanges> _redoStack;

    public ManualSaveHistory(BaseModel model, IStorage storage)
    {
        _model = model;
        _storage = storage;

        _undoStack = new Stack<IWritableModelChanges>();
        _redoStack = new Stack<IWritableModelChanges>();

        _model.ModelChanged += OnModelChanged;
    }

    public event EventHandler? Changed;

    public async Task Save(string path)
    {
        var changes = _undoStack.Reverse().ToArray();

        try
        {
            if (changes.Any())
            {
                await _model.Save(_storage, path, changes);
            }
            else
            {
                return;
            }

            // TODO(#8): saving operation executes in thread pool
            // and launched by 'async void' methods. If two
            // sequential savings happened, clearing of the stacks
            // can cause data race (just after first save we will clear stack
            // with new changes, made after first save started).
            _undoStack.Clear();
        }
        catch (Exception ex)
        {
            throw new ModelSaveException("Model save has failed", ex);
        }

        _redoStack.Clear();
        Changed?.Invoke(this, EventArgs.Empty);
    }

    public async Task SaveAs(string path)
    {
        try
        {
            await _model.Save(_storage, path);

            // TODO(#8): saving operation executes in thread pool
            // and launched by 'async void' methods. If two
            // sequential savings happened, clearing of the stacks
            // can cause data race (just after first save we will clear stack
            // with new changes, made after first save started).
            _undoStack.Clear();
        }
        catch (Exception ex)
        {
            throw new ModelSaveException("Model save has failed", ex);
        }

        _redoStack.Clear();
        Changed?.Invoke(this, EventArgs.Empty);
    }

    public async Task Load(string path)
    {
        await _model.Load(_storage, path);
    }

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
