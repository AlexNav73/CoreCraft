using System.Collections;
using System.Diagnostics;

namespace Navitski.Crystalized.Model.Engine.ChangesTracking;

/// <inheritdoc cref="IRelationChangeSet{TParent, TChild}"/>
[DebuggerDisplay("HasChanges = {HasChanges()}")]
public sealed class RelationChangeSet<TParent, TChild> : IRelationChangeSet<TParent, TChild>
    where TParent : Entity
    where TChild : Entity
{
    private readonly IList<IRelationChange<TParent, TChild>> _changes;

    /// <summary>
    ///     Ctor
    /// </summary>
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
        for (var i = _changes.Count - 1; i >= 0; i--)
        {
            var change = _changes[i];
            if (change.Parent == parent && change.Child == child)
            {
                if (change.Action == RelationAction.Linked && action == RelationAction.Linked)
                {
                    throw new InvalidOperationException($"Can't link [{parent}] and [{child}], because they already have been linked");
                }
                else if (change.Action == RelationAction.Unlinked && action == RelationAction.Unlinked)
                {
                    throw new InvalidOperationException($"Can't unlink [{parent}] and [{child}], because they already have been unlinked");
                }
                else if (change.Action == RelationAction.Linked && action == RelationAction.Unlinked)
                {
                    _changes.RemoveAt(i);
                }
                else if (change.Action == RelationAction.Unlinked && action == RelationAction.Linked)
                {
                    _changes[i] = new RelationChange<TParent, TChild>(action, parent, child);
                }

                return;
            }
        }

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

    /// <inheritdoc cref="IRelationChangeSet{TParent, TChild}.Merge(IRelationChangeSet{TParent, TChild})" />
    public IRelationChangeSet<TParent, TChild> Merge(IRelationChangeSet<TParent, TChild> changeSet)
    {
        var result = new RelationChangeSet<TParent, TChild>(_changes.ToList());

        foreach (var change in changeSet)
        {
            result.Add(change.Action, change.Parent, change.Child);
        }

        return result;
    }
}
