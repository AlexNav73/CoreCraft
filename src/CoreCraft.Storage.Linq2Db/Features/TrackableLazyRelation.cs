using System.Diagnostics;
using CoreCraft.ChangesTracking;
using CoreCraft.Core;

namespace CoreCraft.Storage.Linq2Db.Features;

/// <inheritdoc cref="IMutableRelation{TParent, TChild}"/>
[DebuggerDisplay("{_relation}")]
public sealed class TrackableLazyRelation<TParent, TChild> :
    IMutableLazyRelation<TParent, TChild>,
    IMutableState<ILazyRelation<TParent, TChild>>
    where TParent : Entity
    where TChild : Entity
{
    private readonly IRelationChangeSet<TParent, TChild> _changes;
    private readonly IMutableLazyRelation<TParent, TChild> _relation;

    /// <summary>
    ///     Ctor
    /// </summary>
    public TrackableLazyRelation(
        IRelationChangeSet<TParent, TChild> changesCollection,
        IMutableLazyRelation<TParent, TChild> modelRelation)
    {
        _changes = changesCollection;
        _relation = modelRelation;
    }

    /// <inheritdoc cref="IHaveInfo{T}.Info" />
    public RelationInfo Info => _relation.Info;

    /// <inheritdoc cref="IMutableState{T}.AsReadOnly()" />
    public ILazyRelation<TParent, TChild> AsReadOnly()
    {
        return ((IMutableState<ILazyRelation<TParent, TChild>>)_relation).AsReadOnly();
    }

    /// <inheritdoc cref="IMutableRelation{TParent, TChild}.ApplyAsync(IRelationChangeSet{TParent, TChild}, CancellationToken)" />
    public Task ApplyAsync(IRelationChangeSet<TParent, TChild> changeSet, CancellationToken token = default)
    {
        throw new InvalidOperationException("Unable to apply changes to the relation");
    }
}
