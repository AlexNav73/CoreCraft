using PricingCalc.Model.Engine.Core;

namespace PricingCalc.Model.Engine
{
    internal record MutateResult(IModel OldModel, IModel NewModel, IWritableModelChanges Changes);
}
