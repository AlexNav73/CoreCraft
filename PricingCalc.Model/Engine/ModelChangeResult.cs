using PricingCalc.Model.Engine.Core;

namespace PricingCalc.Model.Engine
{
    internal record ModelChangeResult(IModel OldModel, IModel NewModel, IWritableModelChanges Changes);
}
