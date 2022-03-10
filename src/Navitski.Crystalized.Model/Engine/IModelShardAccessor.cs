namespace Navitski.Crystalized.Model.Engine;

public interface IModelShardAccessor
{
    T Shard<T>() where T : IModelShard;
}
