using System.Collections;
using System.Diagnostics;

namespace Navitski.Crystalized.Model.Engine.ChangesTracking;

/// <inheritdoc cref="IMutableRelation{TParent, TChild}"/>
[DebuggerDisplay("{_relation}")]
public sealed class TrackableRelation<TParent, TChild> : IMutableRelation<TParent, TChild>
    where TParent : Entity
    where TChild : Entity
{
    private readonly IRelationChangeSet<TParent, TChild> _changes;
    private readonly IMutableRelation<TParent, TChild> _relation;

    /// <summary>
    ///     Ctor
    /// </summary>
    public TrackableRelation(IRelationChangeSet<TParent, TChild> changesCollection,
        IRelation<TParent, TChild> modelRelation)
    {
        _changes = changesCollection;
        _relation = (IMutableRelation<TParent, TChild>)modelRelation;
    }

    /// <inheritdoc />
    public void Add(TParent parent, TChild child)
    {
        _relation.Add(parent, child);
        _changes.Add(RelationAction.Linked, parent, child);
    }

    /// <inheritdoc />
    public void Remove(TParent parent, TChild child)
    {
        _relation.Remove(parent, child);
        _changes.Add(RelationAction.Unlinked, parent, child);
    }

    /// <inheritdoc />
    public IEnumerable<TChild> Children(TParent parent)
    {
        return _relation.Children(parent);
    }

    /// <inheritdoc />
    public IEnumerable<TParent> Parents(TChild child)
    {
        return _relation.Parents(child);
    }

    /// <inheritdoc />
    public IEnumerator<TParent> GetEnumerator()
    {
        return _relation.GetEnumerator();
    }

    /// <inheritdoc />
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    /// <inheritdoc/>
    public IRelation<TParent, TChild> Copy()
    {
        throw new InvalidOperationException("Relation can't be copied because it is attached to changes tracking system");
    }
}
