using PricingCalc.Model.Engine;
using PricingCalc.Model.Engine.ChangesTracking;

namespace PricingCalc.Model.Tests.Infrastructure;

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
