namespace Navitski.Crystalized.Model.Engine.Persistence;

/// <summary>
///     An abstraction over some physical storage of the model data.
/// </summary>
/// <remarks>
///     To store or load model from file or a database <see cref="IRepository"/> is used.
///     <see cref="IRepository"/> provides methods to store and load for both collections and relations.
///     <see cref="Scheme"/> is a description of the properties type. It contains a list of properties
///     with they names, types of value, null-ability flag and so one. It is used instead of reflection
///     when it's not clear, how to save or load entity properties.
/// </remarks>
public interface IRepository
{
    /// <summary>
    ///     Inserts a collection of entity-properties pairs to a physical storage
    /// </summary>
    /// <typeparam name="TEntity">A type of an entity</typeparam>
    /// <typeparam name="TProperties">A type of properties</typeparam>
    /// <param name="name">A collection name</param>
    /// <param name="items">Items of the collection</param>
    /// <param name="scheme">A scheme of the property type</param>
    void Insert<TEntity, TProperties>(string name, IReadOnlyCollection<KeyValuePair<TEntity, TProperties>> items, Scheme scheme)
        where TEntity : Entity
        where TProperties : Properties;

    /// <summary>
    ///     Inserts a collection of parent-child relations to a physical storage
    /// </summary>
    /// <typeparam name="TParent">A type of a parent entity</typeparam>
    /// <typeparam name="TChild">A type of a child entity</typeparam>
    /// <param name="name">A relation name</param>
    /// <param name="relations">A collection of parent-child pairs representing relation</param>
    void Insert<TParent, TChild>(string name, IReadOnlyCollection<KeyValuePair<TParent, TChild>> relations)
        where TParent : Entity
        where TChild : Entity;

    /// <summary>
    ///     Updates data in the physical storage using the given version of entity properties
    /// </summary>
    /// <typeparam name="TEntity">A type of an entity</typeparam>
    /// <typeparam name="TProperties">A type of properties</typeparam>
    /// <param name="name">A collection name</param>
    /// <param name="items">Items of the collection</param>
    /// <param name="scheme">A scheme of the property type</param>
    void Update<TEntity, TProperties>(string name, IReadOnlyCollection<KeyValuePair<TEntity, TProperties>> items, Scheme scheme)
        where TEntity : Entity
        where TProperties : Properties;

    /// <summary>
    ///     Deletes entities from the physical storage
    /// </summary>
    /// <typeparam name="TEntity">A type of an entity</typeparam>
    /// <param name="name">A collection name</param>
    /// <param name="entities">A collection of entities to delete</param>
    void Delete<TEntity>(string name, IReadOnlyCollection<TEntity> entities)
        where TEntity : Entity;

    /// <summary>
    ///     Deletes relations between parent and child entities from the physical storage
    /// </summary>
    /// <typeparam name="TParent">A type of a parent entity</typeparam>
    /// <typeparam name="TChild">A type of a child entity</typeparam>
    /// <param name="name">A relation name</param>
    /// <param name="relations">A collection of parent-child pairs representing relation</param>
    void Delete<TParent, TChild>(string name, IReadOnlyCollection<KeyValuePair<TParent, TChild>> relations)
        where TParent : Entity
        where TChild : Entity;

    /// <summary>
    ///     Reads all data of the given collection from the physical storage
    /// </summary>
    /// <typeparam name="TEntity">A type of an entity</typeparam>
    /// <typeparam name="TProperties">A type of properties</typeparam>
    /// <param name="name">A collection name</param>
    /// <param name="collection">A collection in which data will be inserted</param>
    /// <param name="scheme">A scheme of the property type</param>
    void Select<TEntity, TProperties>(string name, IMutableCollection<TEntity, TProperties> collection, Scheme scheme)
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
    /// <param name="name">A relation name</param>
    /// <param name="relation">A relation in which data will be inserted</param>
    /// <param name="parentCollection">A collection of parent entities to retrieve instances</param>
    /// <param name="childCollection">A collection of child entities to retrieve instances</param>
    void Select<TParent, TChild>(string name, IMutableRelation<TParent, TChild> relation, IEnumerable<TParent> parentCollection, IEnumerable<TChild> childCollection)
        where TParent : Entity
        where TChild : Entity;
}
