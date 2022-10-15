namespace Navitski.Crystalized.Model.Engine.Core;

/// <summary>
///     A mapping describes the type of relationships between parents and children.
///     Currently supported mappings include: <see cref="OneToOne{TParent, TChild}"/> and <see cref="OneToMany{TParent, TChild}"/>
/// </summary>
/// <remarks>
///     A mapping describes a type of relation for a one direction, for example from parent to child.
///     Using two mappings both directions can be specified (parent->child and child->parent). Using
///     a combination of multiple mappings it is easy to represent such relations like one-to-one,
///     one-to-many, many-to-many. Mappings should only be used to specify type of relation when
///     the instance of <see cref="Relation{TParent, TChild}"/> is constructed. Mappings should not
///     be used in the user code.
/// </remarks>
/// <typeparam name="TParent">A type of parent entity</typeparam>
/// <typeparam name="TChild">A type of child entity</typeparam>
public interface IMapping<TParent, TChild> : IEnumerable<TParent>, ICopy<IMapping<TParent, TChild>>
    where TParent : Entity
    where TChild : Entity
{
    /// <summary>
    ///     Adds a new relation form parent to child
    /// </summary>
    /// <param name="parent">A parent entity</param>
    /// <param name="child">A child entity</param>
    /// <exception cref="Exceptions.DuplicatedRelationException">Throws when a relation is already exists</exception>
    void Add(TParent parent, TChild child);

    /// <summary>
    ///     Removes a relation between parent and child
    /// </summary>
    /// <param name="parent">A parent entity</param>
    /// <param name="child">A child entity</param>
    /// <exception cref="Exceptions.MissingRelationException">Throws then a relation is not exists</exception>
    void Remove(TParent parent, TChild child);

    /// <summary>
    ///     Tests if a mapping contains parent entity
    /// </summary>
    /// <param name="parent">An entity to test</param>
    /// <returns>True - if a relation contains parent entity</returns>
    bool Contains(TParent parent);

    /// <summary>
    ///     Returns all related children for a given parent
    /// </summary>
    /// <param name="parent">A parent entity</param>
    /// <returns>A collection of children for a given parent</returns>
    IEnumerable<TChild> Children(TParent parent);
}
