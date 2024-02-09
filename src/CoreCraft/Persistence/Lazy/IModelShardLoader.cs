namespace CoreCraft.Persistence.Lazy;

/// <summary>
///     Represents an interface for loading specific collections/relations of model shards.
/// </summary>
/// <typeparam name="T">The type of the model shard to load.</typeparam>
public interface IModelShardLoader<T> : ILazyLoader
    where T : IMutableModelShard
{
    /// <summary>
    ///     Loads a collection associated with the model shard using the provided function.
    /// </summary>
    /// <typeparam name="TEntity">The type of entities in the collection.</typeparam>
    /// <typeparam name="TProperty">The type of properties associated with the entities.</typeparam>
    /// <param name="collection">A function that returns the collection associated with the model shard.</param>
    /// <returns>The model shard loader.</returns>
    IModelShardLoader<T> Collection<TEntity, TProperty>(Func<T, IMutableCollection<TEntity, TProperty>> collection)
        where TEntity : Entity
        where TProperty : Properties;

    /// <summary>
    ///     Loads a relation associated with the model shard using the provided functions.
    /// </summary>
    /// <typeparam name="TParent">The type of the parent entities in the relation.</typeparam>
    /// <typeparam name="TChild">The type of the child entities in the relation.</typeparam>
    /// <param name="relation">A function that returns the relation associated with the model shard.</param>
    /// <param name="parents">A function that returns the parent entities' collection associated with the relation.</param>
    /// <param name="children">A function that returns the child entities' collection associated with the relation.</param>
    /// <returns>The model shard loader.</returns>
    IModelShardLoader<T> Relation<TParent, TChild>(
        Func<T, IMutableRelation<TParent, TChild>> relation,
        Func<T, IEnumerable<TParent>> parents,
        Func<T, IEnumerable<TChild>> children)
        where TParent : Entity
        where TChild : Entity;
}

