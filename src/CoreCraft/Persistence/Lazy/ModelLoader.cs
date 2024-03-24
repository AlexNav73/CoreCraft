namespace CoreCraft.Persistence.Lazy;

internal sealed class ModelLoader<T> : ILazyLoader
    where T : IMutableModelShard
{
    private readonly T _shard;
    private readonly bool _force;

    public ModelLoader(T shard, bool force)
    {
        _shard = shard;
        _force = force;
    }

    public void Load(IRepository repository)
    {
        _shard.Load(repository, _force);
    }
}
