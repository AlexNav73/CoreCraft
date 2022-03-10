using Navitski.Crystalized.Model.Engine;
using Navitski.Crystalized.Model.Engine.Commands;

namespace Navitski.Crystalized.Model.Tests.Infrastructure.Commands;

internal class ModifyAllEntitiesCommand : ModelCommand<FakeModel>
{
    public ModifyAllEntitiesCommand(FakeModel model)
        : base(model)
    {
    }

    protected override void ExecuteInternal(IModel model)
    {
        var modelShard = model.Shard<IMutableFakeModelShard>();

        foreach (var entity in modelShard.FirstCollection.ToArray())
        {
            modelShard.FirstCollection.Modify(entity, p => p with { NonNullableStringProperty = "test2" });
        }

        foreach (var entity in modelShard.SecondCollection.ToArray())
        {
            modelShard.SecondCollection.Modify(entity, p => p with { FloatProperty = 1f });
        }
    }
}
