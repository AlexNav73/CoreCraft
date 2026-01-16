using CoreCraft.Commands;
using CoreCraft.Core;

namespace CoreCraft.Tests.Infrastructure.Commands;

internal class ModifyAllEntitiesCommand : IAsyncCommand
{
    public Task ExecuteAsync(IMutableModel model, CancellationToken token = default)
    {
        var modelShard = model.Shard<IMutableFakeModelShard>();

        foreach (var entity in modelShard.FirstCollection.Entities.ToArray())
        {
            modelShard.FirstCollection.Modify(entity, p => p with { NonNullableStringProperty = "test2" });
        }

        foreach (var entity in modelShard.SecondCollection.Entities.ToArray())
        {
            modelShard.SecondCollection.Modify(entity, p => p with { FloatProperty = 1f });
        }

        return Task.CompletedTask;
    }
}
