using System.Collections;
using System.Diagnostics;

namespace PricingCalc.Model.Engine.ChangesTracking;

[DebuggerDisplay("{_relation}")]
public class TrackableRelation<TParent, TChild> : IMutableRelation<TParent, TChild>
    where TParent : Entity
    where TChild : Entity
{
    private readonly IRelationChangeSet<TParent, TChild> _changes;
    private readonly IMutableRelation<TParent, TChild> _relation;

    public TrackableRelation(IRelationChangeSet<TParent, TChild> changesCollection,
        IRelation<TParent, TChild> modelRelation)
    {
        _changes = changesCollection;
        _relation = (IMutableRelation<TParent, TChild>)modelRelation;
    }

    public void Add(TParent parent, TChild child)
    {
        _relation.Add(parent, child);
        _changes.Add(RelationAction.Linked, parent, child);
    }

    public void Remove(TParent parent, TChild child)
    {
        _relation.Remove(parent, child);
        _changes.Add(RelationAction.Unlinked, parent, child);
    }

    public IEnumerable<TChild> Children(TParent parent)
    {
        return _relation.Children(parent);
    }

    public IEnumerable<TParent> Parents(TChild child)
    {
        return _relation.Parents(child);
    }

    public IEnumerator<TParent> GetEnumerator()
    {
        return _relation.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public IRelation<TParent, TChild> Copy()
    {
        throw new InvalidOperationException("Relation can't be copied because it is attached to changes tracking system");
    }
}
