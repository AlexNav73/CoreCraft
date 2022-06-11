using System.Collections;
using System.Diagnostics;

namespace Navitski.Crystalized.Model.Engine.ChangesTracking;

/// <inheritdoc cref="IRelationChangeSet{TParent, TChild}"/>
[DebuggerDisplay("HasChanges = {HasChanges()}")]
public class RelationChangeSet<TParent, TChild> : IRelationChangeSet<TParent, TChild>
    where TParent : Entity
    where TChild : Entity
{
    private readonly IList<IRelationChange<TParent, TChild>> _changes;

    public RelationChangeSet()
        : this(new List<IRelationChange<TParent, TChild>>())
    {
    }

    private RelationChangeSet(IList<IRelationChange<TParent, TChild>> changes)
    {
        _changes = changes;
    }

    /// <inheritdoc />
    public void Add(RelationAction action, TParent parent, TChild child)
    {
        _changes.Add(new RelationChange<TParent, TChild>(action, parent, child));
    }

    /// <inheritdoc />
    public IRelationChangeSet<TParent, TChild> Invert()
    {
        var inverted = _changes.Reverse().Select(x => x.Invert()).ToList();
        return new RelationChangeSet<TParent, TChild>(inverted);
    }

    /// <inheritdoc />
    public bool HasChanges() => _changes.Count > 0;

    /// <inheritdoc />
    public void Apply(IMutableRelation<TParent, TChild> relation)
    {
        foreach (var change in _changes)
        {
            switch (change.Action)
            {
                case RelationAction.Linked:
                    relation.Add(change.Parent, change.Child);
                    break;
                case RelationAction.Unlinked:
                    relation.Remove(change.Parent, change.Child);
                    break;
                default:
                    throw new NotSupportedException($"An action [{change.Action}] is not supported.");
            }
        }
    }

    /// <inheritdoc />
    public IEnumerator<IRelationChange<TParent, TChild>> GetEnumerator()
    {
        return _changes.GetEnumerator();
    }

    /// <inheritdoc />
    IEnumerator IEnumerable.GetEnumerator()
    {
        return _changes.GetEnumerator();
    }
}
