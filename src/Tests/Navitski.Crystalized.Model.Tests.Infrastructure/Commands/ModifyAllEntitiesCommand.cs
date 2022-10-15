using Navitski.Crystalized.Model.Engine.Commands;
using Navitski.Crystalized.Model.Engine.Core;

namespace Navitski.Crystalized.Model.Tests.Infrastructure.Commands;

internal class ModifyAllEntitiesCommand : ICommand
{
    public void Execute(IModel model, CancellationToken token = default)
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
