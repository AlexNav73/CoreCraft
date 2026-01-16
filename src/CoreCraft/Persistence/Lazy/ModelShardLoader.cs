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

    public IModelShardLoader<T> Relation<TParent, TParentProperties, TChild, TChildProperties>(
        Func<T, IMutableRelation<TParent, TChild>> relation,
        Func<T, IMutableCollection<TParent, TParentProperties>> parents,
        Func<T, IMutableCollection<TChild, TChildProperties>> children)
        where TParent : Entity
        where TChild : Entity
        where TParentProperties : Properties
        where TChildProperties : Properties
    {
        var parentCollection = parents(_shard);
        var childrenCollection = children(_shard);

        _collections.Add(parentCollection);
        _collections.Add(childrenCollection);
        _relations.Add(r => relation(_shard).Load(r, parentCollection.Entities, childrenCollection.Entities));

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
