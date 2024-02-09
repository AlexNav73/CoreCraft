namespace CoreCraft.Persistence.Lazy;

internal sealed class ModelShardLoader<T> : IModelShardLoader<T>
    where T : IMutableModelShard
{
    private readonly T _shard;
    private readonly HashSet<ILoadable> _collections;
    private readonly List<Action<IRepository>> _relations;

    public ModelShardLoader(T shard)
    {
        _shard = shard;

        _collections = [];
        _relations = [];
    }

    public IModelShardLoader<T> Collection<TEntity, TProperty>(Func<T, IMutableCollection<TEntity, TProperty>> collection)
        where TEntity : Entity
        where TProperty : Properties
    {
        _collections.Add(collection(_shard));

        return this;
    }

    public IModelShardLoader<T> Relation<TParent, TChild>(
        Func<T, IMutableRelation<TParent, TChild>> relation,
        Func<T, IEnumerable<TParent>> parents,
        Func<T, IEnumerable<TChild>> children)
        where TParent : Entity
        where TChild : Entity
    {
        var parentCollection = parents(_shard);
        var childrenCollection = children(_shard);

        if (parentCollection is ILoadable loadableP)
        {
            _collections.Add(loadableP);
        }

        if (childrenCollection is ILoadable loadableC)
        {
            _collections.Add(loadableC);
        }

        _relations.Add(r => relation(_shard).Load(r, parentCollection, childrenCollection));

        return this;
    }

    public void Load(IRepository repository)
    {
        foreach (var collection in _collections)
        {
            collection.Load(repository);
        }

        foreach (var relation in _relations)
        {
            relation(repository);
        }
    }
}
