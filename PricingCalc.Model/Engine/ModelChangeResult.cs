using PricingCalc.Model.Engine.ChangesTracking;

namespace PricingCalc.Model.Engine
{
    internal record ModelChangeResult(IModel OldModel, IModel NewModel, IWritableModelChanges Changes);
}
