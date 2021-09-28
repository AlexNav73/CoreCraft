using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using PricingCalc.Model.Engine.Core;

namespace PricingCalc.Model.Engine.ChangesTracking
{
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

        public void Add(RelationAction action, TParent parent, TChild child)
        {
            _changes.Add(new RelationChange<TParent, TChild>(action, parent, child));
        }

        public IRelationChangeSet<TParent, TChild> Invert()
        {
            var inverted = _changes.Reverse().Select(x => x.Invert()).ToList();
            return new RelationChangeSet<TParent, TChild>(inverted);
        }

        public bool HasChanges() => _changes.Count > 0;

        public void Apply(IRelation<TParent, TChild> relation)
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
                        break;
                }
            }
        }

        public IEnumerator<IRelationChange<TParent, TChild>> GetEnumerator()
        {
            return _changes.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _changes.GetEnumerator();
        }
    }
}
