using CoreCraft.ChangesTracking;

namespace CoreCraft.Core;

/// <summary>
///     This interface contains an <see cref="Info"/> property which is used
///     to connect a collection/relation types with related changes sets.
/// </summary>
/// <remarks>
///     The interfaces <see cref="IChangesFrame" /> and <see cref="IModelShard" />
///     cannot be connected in any way. It is impossible to have a common interface
///     encompassing all collections/relations that will differ by type.
///     <see cref="IChangesFrame" /> will always contain change sets,
///     while <see cref="IModelShard" />`s will always contain collections/relations.
///     To facilitate finding a related change set for a given collection/relation
///     in <see cref="IChangesFrame" />, the following interface was introduced.
///     Each collection and relation contains the property <see cref="Info" />,
///     which should hold the same value for a related <see cref="ICollectionChangeSet{TEntity, TProperties}" />
///     and <see cref="ICollection{TParent, TChild}" /> or <see cref="IRelationChangeSet{TParent, TChild}"/>.
///     and <see cref="IRelation{TParent, TChild}"/>. Using this <see cref = "Info" /> property,
///     <see cref="IChangesFrame" /> can match collections and relations to
///     their corresponding change sets and return the information to the user.
/// </remarks>
public interface IHaveInfo<T>
{
    /// <summary>
    ///     The information about collection/relation which should match an <see cref="Info"/>
    ///     property of related changes sets
    /// </summary>
    T Info { get; }
}
