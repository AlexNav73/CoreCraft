using PricingCalc.Model.Engine.ChangesTracking;

namespace PricingCalc.Model.Engine;

public class ModelChangedEventArgs
{
    public IModel OldModel { get; private set; }

    public IModel NewModel { get; private set; }

    public IModelChanges Changes { get; private set; }

    internal ModelChangedEventArgs(IModel oldModel, IModel newModel, IModelChanges changes)
    {
        OldModel = oldModel;
        NewModel = newModel;
        Changes = changes;
    }
}
