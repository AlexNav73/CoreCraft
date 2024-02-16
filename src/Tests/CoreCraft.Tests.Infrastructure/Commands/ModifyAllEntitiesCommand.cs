using CoreCraft.Commands;
using CoreCraft.Core;

namespace CoreCraft.Tests.Infrastructure.Commands;

internal class ModifyAllEntitiesCommand : ICommand
{
    public void Execute(IMutableModel model, CancellationToken token = default)
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
