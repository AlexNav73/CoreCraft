using PricingCalc.Model.Engine.Core;

namespace PricingCalc.Model.Engine
{
    public record ModelChangeResult(IModel OldModel, IModel NewModel, IWritableModelChanges Changes);
}
