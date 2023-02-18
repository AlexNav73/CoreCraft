namespace Navitski.Crystalized.Model.Engine.Core;

/// <summary>
///     Relation represents a relationships between parents and children entities.
/// </summary>
/// <remarks>
///     Using <see cref="ICollection{TEntity, TProperties}"/> it is possible to store entities
///     with they properties, but it is a flat list. If entities are connected logically then
///     this connection should be stored somewhere. The <see cref="IRelation{TParent, TChild}"/>
///     type is used for storing the connection (relationship) between entities in the form of
///     one-to-one, one-to-many, many-to-many. Using relations is it possible to describe hierarchical
///     structure of entities. It is also possible to store a tree structure in the <see cref="IRelation{TParent, TChild}"/>.
/// </remarks>
/// <typeparam name="TParent">A type of parent entity</typeparam>
/// <typeparam name="TChild">A type of child entity</typeparam>
public interface IRelation<TParent, TChild> : IEnumerable<TParent>, IHaveId, ICopy<IRelation<TParent, TChild>>
    where TParent : Entity
    where TChild : Entity
{
    /// <summary>
    ///     Tests if a relation contains an entity
    /// </summary>
    /// <param name="entity">An entity to test</param>
    /// <returns>True - if relation contains parent entity</returns>
    bool ContainsParent(TParent entity);

    /// <summary>
    ///     Tests if a relation contains an entity
    /// </summary>
    /// <param name="entity">An entity to test</param>
    /// <returns>True - if relation contains child entity</returns>
    bool ContainsChild(TChild entity);

    /// <summary>
    ///     Retrieves all children entities for a given parent entity
    /// </summary>
    /// <param name="parent">A parent entity</param>
    /// <returns>A collection of child entities</returns>
    IEnumerable<TChild> Children(TParent parent);

    /// <summary>
    ///     Retrieves all parent entities for a given child entity
    /// </summary>
    /// <param name="child">A child entity</param>
    /// <returns>A collection of parent entities</returns>
    IEnumerable<TParent> Parents(TChild child);
}
