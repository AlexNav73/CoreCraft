using CoreCraft.ChangesTracking;

namespace CoreCraft.Persistence;

/// <summary>
///     An abstraction over some physical storage of the model data.
/// </summary>
/// <remarks>
///     To store or load model from file or a database <see cref="IRepository"/> is used.
///     <see cref="IRepository"/> provides methods to store and load for both collections and relations.
///     <see cref="CollectionInfo"/> is a description of a collection type. It contains a list of properties
///     with they names, types of value, null-ability flag and so one.
///     <see cref="RelationInfo"/> is a description of a relation type. It contains shard and relation names.
///     They are used instead of reflection when it's not clear, how to save or load entities and their properties.
/// </remarks>
public interface IRepository
{
    /// <summary>
    ///     Inserts a collection of entity-properties pairs to a physical storage
    /// </summary>
    /// <typeparam name="TEntity">A type of an entity</typeparam>
    /// <typeparam name="TProperties">A type of properties</typeparam>
    /// <param name="scheme">A scheme of the collection</param>
    /// <param name="items">Items of the collection</param>
    void Insert<TEntity, TProperties>(CollectionInfo scheme, IReadOnlyCollection<KeyValuePair<TEntity, TProperties>> items)
        where TEntity : Entity
        where TProperties : Properties;

    /// <summary>
    ///     Inserts a collection of parent-child relations to a physical storage
    /// </summary>
    /// <typeparam name="TParent">A type of a parent entity</typeparam>
    /// <typeparam name="TChild">A type of a child entity</typeparam>
    /// <param name="scheme">A scheme of the relation</param>
    /// <param name="relations">A collection of parent-child pairs representing relation</param>
    void Insert<TParent, TChild>(RelationInfo scheme, IReadOnlyCollection<KeyValuePair<TParent, TChild>> relations)
        where TParent : Entity
        where TChild : Entity;

    /// <summary>
    ///     Updates data in the physical storage using the given version of entity properties
    /// </summary>
    /// <typeparam name="TEntity">A type of an entity</typeparam>
    /// <typeparam name="TProperties">A type of properties</typeparam>
    /// <param name="scheme">A scheme of the collection</param>
    /// <param name="changes">A collection of changes</param>
    void Update<TEntity, TProperties>(CollectionInfo scheme, IReadOnlyCollection<ICollectionChange<TEntity, TProperties>> changes)
        where TEntity : Entity
        where TProperties : Properties;

    /// <summary>
    ///     Deletes entities from the physical storage
    /// </summary>
    /// <typeparam name="TEntity">A type of an entity</typeparam>
    /// <param name="scheme">A scheme of the collection</param>
    /// <param name="entities">A collection of entities to delete</param>
    void Delete<TEntity>(CollectionInfo scheme, IReadOnlyCollection<TEntity> entities)
        where TEntity : Entity;

    /// <summary>
    ///     Deletes relations between parent and child entities from the physical storage
    /// </summary>
    /// <typeparam name="TParent">A type of a parent entity</typeparam>
    /// <typeparam name="TChild">A type of a child entity</typeparam>
    /// <param name="scheme">A scheme of the relation</param>
    /// <param name="relations">A collection of parent-child pairs representing relation</param>
    void Delete<TParent, TChild>(RelationInfo scheme, IReadOnlyCollection<KeyValuePair<TParent, TChild>> relations)
        where TParent : Entity
        where TChild : Entity;

    /// <summary>
    ///     Reads all data of the given collection from the physical storage
    /// </summary>
    /// <typeparam name="TEntity">A type of an entity</typeparam>
    /// <typeparam name="TProperties">A type of properties</typeparam>
    /// <param name="scheme">A scheme of the collection</param>
    /// <param name="collection">A collection in which data will be inserted</param>
    void Select<TEntity, TProperties>(CollectionInfo scheme, IMutableCollection<TEntity, TProperties> collection)
        where TEntity : Entity
        where TProperties : Properties;

    /// <summary>
    ///     Reads all data of the given relation from the physical storage
    /// </summary>
    /// <remarks>
    ///     Both parent and child collections are used to ensure, that relation will have
    ///     the same instances of entities. This will reduce a memory footprint after data
    ///     have been loaded so we won't have a lot of different objects represented the
    ///     same entity.
    /// </remarks>
    /// <typeparam name="TParent">A type of a parent entity</typeparam>
    /// <typeparam name="TChild">A type of a child entity</typeparam>
    /// <param name="scheme">A scheme of the relation</param>
    /// <param name="relation">A relation in which data will be inserted</param>
    /// <param name="parentCollection">A collection of parent entities to retrieve instances</param>
    /// <param name="childCollection">A collection of child entities to retrieve instances</param>
    void Select<TParent, TChild>(RelationInfo scheme, IMutableRelation<TParent, TChild> relation, IEnumerable<TParent> parentCollection, IEnumerable<TChild> childCollection)
        where TParent : Entity
        where TChild : Entity;
}
