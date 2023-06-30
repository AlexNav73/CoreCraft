using CoreCraft.Engine.Exceptions;

namespace CoreCraft.Engine.Core;

internal sealed class Model : IModel
{
    public Model(IEnumerable<IModelShard> shards)
    {
        Shards = shards.ToArray();
    }

    internal IReadOnlyCollection<IModelShard> Shards;

    public T Shard<T>() where T : IModelShard
    {
        try
        {
            return Shards.OfType<T>().Single();
        }
        catch (Exception ex)
        {
            throw new ModelShardNotFoundException($"Model shard [{typeof(T).Name}] not found", ex);
        }
    }
}
