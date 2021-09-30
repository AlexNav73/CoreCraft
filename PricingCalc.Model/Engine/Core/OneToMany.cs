using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace PricingCalc.Model.Engine.Core
{
    [DebuggerDisplay("Count = {_relation.Keys.Count}")]
    public class OneToMany<TParent, TChild> : IMapping<TParent, TChild>
        where TParent : Entity
        where TChild : Entity
    {
        private readonly IDictionary<TParent, IList<TChild>> _relation;

        public OneToMany() : this(new Dictionary<TParent, IList<TChild>>())
        {
        }

        private OneToMany(IDictionary<TParent, IList<TChild>> relation)
        {
            _relation = relation;
        }

        public void Add(TParent parent, TChild child)
        {
            if (_relation.TryGetValue(parent, out var children))
            {
                if (!children.Contains(child))
                {
                    children.Add(child);
                }
                else
                {
                    throw new InvalidOperationException($"Link between {parent} and {child} already exists");
                }
            }
            else
            {
                if (_relation.ContainsKey(parent))
                {
                    throw new InvalidOperationException($"Linking {parent} with {child} has failed");
                }

                _relation.Add(parent, new List<TChild>() { child });
            }
        }

        public IEnumerable<TChild> Children(TParent parent)
        {
            if (_relation.TryGetValue(parent, out var children))
            {
                return children;
            }

            return Array.Empty<TChild>();
        }

        public void Remove(TParent parent, TChild child)
        {
            if (!_relation.TryGetValue(parent, out var children))
            {
                throw new InvalidOperationException($"Can't remove {parent} - {child} link");
            }

            if (children.Contains(child))
            {
                children.Remove(child);
            }

            if (children.Count == 0)
            {
                _relation.Remove(parent);
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
            var relation = new Dictionary<TParent, IList<TChild>>();
            foreach (var pair in _relation)
            {
                relation[pair.Key] = new List<TChild>(pair.Value);
            }
            return new OneToMany<TParent, TChild>(relation);
        }
    }
}
