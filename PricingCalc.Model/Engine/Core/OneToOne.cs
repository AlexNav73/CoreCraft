using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace PricingCalc.Model.Engine.Core
{
    [DebuggerDisplay("Count = {_relation.Keys.Count}")]
    public class OneToOne<TParent, TChild> : IMapping<TParent, TChild>
        where TParent : Entity
        where TChild : Entity
    {
        private readonly IDictionary<TParent, TChild> _relation;

        public OneToOne() : this(new Dictionary<TParent, TChild>())
        {
        }

        private OneToOne(IDictionary<TParent, TChild> relation)
        {
            _relation = relation;
        }

        public void Add(TParent parent, TChild child)
        {
            if (_relation.ContainsKey(parent))
            {
                throw new InvalidOperationException($"Linking {parent} with {child} has failed");
            }

            _relation.Add(parent, child);
        }

        public IEnumerable<TChild> Children(TParent parent)
        {
            if (_relation.TryGetValue(parent, out var child))
            {
                yield return child;
            }
        }

        public void Remove(TParent parent, TChild child)
        {
            if (_relation.TryGetValue(parent, out var c) && c.Equals(child))
            {
                _relation.Remove(parent);
            }
            else
            {
                throw new InvalidOperationException($"Can't remove {parent} - {child} link");
            }
        }

        public void Clear()
        {
            _relation.Clear();
        }

        public IEnumerator<TParent> GetEnumerator()
        {
            return _relation.Keys.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IMapping<TParent, TChild> Copy()
        {
            var relation = new Dictionary<TParent, TChild>();
            foreach (var pair in _relation)
            {
                relation[pair.Key] = pair.Value;
            }
            return new OneToOne<TParent, TChild>(relation);
        }
    }
}
