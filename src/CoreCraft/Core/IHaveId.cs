using CoreCraft.ChangesTracking;

namespace CoreCraft.Core;

/// <summary>
///     This interface contains an <see cref="Id"/> property which is used
///     to connect a collection/relation types with related changes sets.
/// </summary>
/// <remarks>
///     <see cref="IChangesFrame"/>s and <see cref="IModelShard"/>s cannot be
///     connected by any means. It is impossible to have a common interface with
///     all collections/relations which will differ by type. <see cref="IChangesFrame"/>
///     will always contain changes sets and <see cref="IModelShard"/>s will always contain
///     collections/relations. So to give the possibility to file a related changes set
///     for a given collection/relation in <see cref="IChangesFrame"/> this interface was
///     introduced. Each collection and relation contains <see cref="Id"/> this property
///     should contain the same value for a related <see cref="ICollectionChangeSet{TEntity, TProperties}"/>
///     and <see cref="IRelationChangeSet{TParent, TChild}"/>. Using this <see cref="Id"/>
///     property <see cref="IChangesFrame"/> can match collection and relation to the changes set
///     and return to the user.
/// </remarks>
public interface IHaveId
{
    /// <summary>
    ///     The unique identifier of a collection/relation which should match an <see cref="Id"/>
    ///     property of related changes sets
    /// </summary>
    string Id { get; }
}
