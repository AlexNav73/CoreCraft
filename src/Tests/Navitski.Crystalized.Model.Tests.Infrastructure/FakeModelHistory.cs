using Navitski.Crystalized.Model.Engine;
using Navitski.Crystalized.Model.Engine.ChangesTracking;

namespace Navitski.Crystalized.Model.Tests.Infrastructure;

public class FakeModelHistory : DisposableBase
{
    public FakeModelHistory(FakeModel model)
    {
        UndoStack = new Stack<IWritableModelChanges>();

        model.ModelChanged += OnModelChanged;
    }

    public event EventHandler? Changed;

    public Stack<IWritableModelChanges> UndoStack { get; }

    internal void OnModelChanged(object? sender, ModelChangedEventArgs args)
    {
        UndoStack.Push((IWritableModelChanges)args.Changes);
        Changed?.Invoke(this, EventArgs.Empty);
    }
}
