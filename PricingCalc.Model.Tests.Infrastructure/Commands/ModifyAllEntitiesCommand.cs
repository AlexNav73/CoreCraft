using PricingCalc.Model.Engine;
using PricingCalc.Model.Engine.Commands;

namespace PricingCalc.Model.Tests.Infrastructure.Commands;

internal class ModifyAllEntitiesCommand : ModelCommand<FakeModel>
{
    public ModifyAllEntitiesCommand(FakeModel model)
        : base(model)
    {
    }

    protected override void ExecuteInternal(IModel model)
    {
        var modelShard = model.Shard<IFakeModelShard>();

        foreach (var entity in modelShard.FirstCollection)
        {
            modelShard.FirstCollection.Modify(entity, p => p with { NonNullableStringProperty = "test2" });
        }

        foreach (var entity in modelShard.SecondCollection)
        {
            modelShard.SecondCollection.Modify(entity, p => p with { FloatProperty = 1f });
        }
    }
}
