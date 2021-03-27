using PricingCalc.Model.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace PricingCalc.Model.Engine.Core
{
    [DebuggerDisplay("Count = {_entities.Count}")]
    public class Collection<TEntity, TData> : ICollection<TEntity, TData>, ICopy<Collection<TEntity, TData>>
        where TEntity : IEntity, ICopy<TEntity>
        where TData : ICopy<TData>
    {
        private readonly IDictionary<Guid, int> _relation;
        private readonly IList<TEntity> _entities;
        private readonly IList<TData> _data;

        public Collection(IEntityFactory<TEntity, TData> entityFactory)
            : this(new Dictionary<Guid, int>(), new List<TEntity>(), new List<TData>(), entityFactory)
        {
        }

        private Collection(
            IDictionary<Guid, int> relation,
            IList<TEntity> entities,
            IList<TData> data,
            IEntityFactory<TEntity, TData> factory)
        {
            _relation = relation;
            _entities = entities;
            _data = data;

            Factory = factory;
        }

        public IEntityFactory<TEntity, TData> Factory { get; }

        public void Add(TEntity entity, TData data)
        {
            if (_relation.TryAdd(entity.Id, _data.Count))
            {
                _entities.Add(entity);
                _data.Add(data);
            }
        }

        public TData Get(TEntity entity)
        {
            if (_relation.TryGetValue(entity.Id, out var index))
            {
                return _data[index];
            }

            throw new KeyNotFoundException();
        }

        public int IndexOf(TEntity entity)
        {
            return _entities.IndexOf(entity);
        }

        public TEntity ElementAt(int index)
        {
            return _entities[index];
        }

        public void Modify(TEntity entity, Action<TData> modifier)
        {
            if (_relation.TryGetValue(entity.Id, out var index))
            {
                modifier(_data[index]);
            }
        }

        public void Remove(TEntity entity)
        {
            if (_relation.TryGetValue(entity.Id, out var index))
            {
                _data.RemoveAt(index);
                _relation.Remove(entity.Id);

                foreach (var pair in _relation)
                {
                    if (pair.Value > index)
                    {
                        _relation[pair.Key] = pair.Value - 1;
                    }
                }
            }

            _entities.Remove(entity);
        }

        public Collection<TEntity, TData> Copy()
        {
            return new Collection<TEntity, TData>(
                _relation.Copy(),
                _entities.Copy(),
                _data.Copy(),
                Factory);
        }

        public IEnumerator<TEntity> GetEnumerator()
        {
            return _entities.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
