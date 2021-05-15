using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace PricingCalc.Model.Engine.Core
{
    [DebuggerDisplay(@"Parent [{_parentToChildRelations}] Children [{_childToParentRelations}]")]
    public class Relation<TParent, TChild> : IRelation<TParent, TChild>
        where TParent : IEntity
        where TChild : IEntity
    {
        private readonly IMapping<TParent, TChild> _parentToChildRelations;
        private readonly IMapping<TChild, TParent> _childToParentRelations;

        public Relation(
            IMapping<TParent, TChild> parentToChildRelation,
            IMapping<TChild, TParent> childToParentRelation)
        {
            _parentToChildRelations = parentToChildRelation;
            _childToParentRelations = childToParentRelation;
        }

        public void Add(TParent parent, TChild child)
        {
            if (!CanAdd(parent, child))
            {
                throw new InvalidOperationException($"Can't add {parent} - {child} link");
            }

            _parentToChildRelations.Add(parent, child);
            _childToParentRelations.Add(child, parent);
        }

        public IEnumerable<TChild> Children(TParent parent)
        {
            return _parentToChildRelations.Children(parent);
        }

        public IEnumerable<TParent> Parents(TChild child)
        {
            return _childToParentRelations.Children(child);
        }

        public void Remove(TParent parent, TChild child)
        {
            _parentToChildRelations.Remove(parent, child);
            _childToParentRelations.Remove(child, parent);
        }

        public IRelation<TParent, TChild> Copy()
        {
            return new Relation<TParent, TChild>(
                _parentToChildRelations.Copy(),
                _childToParentRelations.Copy());
        }

        private bool CanAdd(TParent parent, TChild child)
        {
            var canLinkParent = _parentToChildRelations.CanAdd(parent, child);
            var canLinkChild = _childToParentRelations.CanAdd(child, parent);

            return canLinkParent && canLinkChild;
        }

        public IEnumerator<TParent> GetEnumerator()
        {
            return _parentToChildRelations.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
