using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace PricingCalc.Model.Engine.Core
{
    [DebuggerDisplay("Count = {_relation.Keys.Count}")]
    public class OneToMany<TParent, TChild> : IMapping<TParent, TChild>
        where TParent : IEntity
        where TChild : IEntity, ICopy<TChild>
    {
        private readonly IDictionary<TParent, IList<TChild>> _relation;

        public OneToMany() : this(new Dictionary<TParent, IList<TChild>>())
        {
        }

        private OneToMany(IDictionary<TParent, IList<TChild>> relation)
        {
            _relation = relation;
        }

        public bool CanAdd(TParent parent, TChild child)
        {
            return !_relation.TryGetValue(parent, out var children) || !children.Contains(child);
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
                if (!_relation.TryAdd(parent, new List<TChild>() { child }))
                {
                    throw new InvalidOperationException($"Linking {parent} with {child} has failed");
                }
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
                var values = new List<TChild>(pair.Value.Count);
                for (var i = 0; i < pair.Value.Count; i++)
                {
                    values.Add(pair.Value[i].Copy());
                }
                relation[pair.Key] = values;
            }
            return new OneToMany<TParent, TChild>(relation);
        }
    }
}
