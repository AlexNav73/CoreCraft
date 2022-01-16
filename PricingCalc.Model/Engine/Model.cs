using PricingCalc.Model.Engine.Exceptions;

namespace PricingCalc.Model.Engine;

internal class Model : IModel
{
    public Model(IEnumerable<IModelShard> shards)
    {
        Shards = shards.ToArray();
    }

    internal IReadOnlyCollection<IModelShard> Shards;

    public T Shard<T>() where T : IModelShard
    {
        var shard = Shards.OfType<T>().SingleOrDefault();
        if (shard == null)
        {
            throw new ModelShardNotFoundException(typeof(T).Name);
        }

        return shard;
    }
}
