using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace PricingCalc.Model.Engine.Core
{
    [DebuggerDisplay("Count = {Count}")]
    public class Collection<TEntity, TData> : ICollection<TEntity, TData>, IFactory<TEntity, TData>
        where TEntity : IEntity, ICopy<TEntity>
        where TData : ICopy<TData>
    {
        private readonly Func<Guid, TEntity> _entityCreator;
        private readonly Func<TData> _dataCreator;

        private readonly IDictionary<Guid, TData> _relation;

        public Collection(Func<Guid, TEntity> entityCreator, Func<TData> dataCreator)
            : this(new Dictionary<Guid, TData>(), entityCreator, dataCreator)
        {
        }

        private Collection(
            IDictionary<Guid, TData> relation,
            Func<Guid, TEntity> entityCreator,
            Func<TData> dataCreator)
        {
            _relation = relation;
            _entityCreator = entityCreator;
            _dataCreator = dataCreator;
        }

        public int Count => _relation.Count;

        public Func<Guid, TEntity> EntityFactory => _entityCreator;

        public Func<TData> DataFactory => _dataCreator;

        public EntityBuilder<TEntity, TData> Create()
        {
            void Add(TEntity entity, TData data)
            {
                if (!_relation.TryAdd(entity.Id, data))
                {
                    throw new InvalidOperationException($"Entity [{entity}] can't be added to the collection");
                }
            }

            return new EntityBuilder<TEntity, TData>(Add, this);
        }

        public TData Get(TEntity entity)
        {
            if (_relation.TryGetValue(entity.Id, out var data))
            {
                return data;
            }

            throw new KeyNotFoundException($"Collection doesn't contain entity [{entity}]");
        }

        public void Modify(TEntity entity, Action<TData> modifier)
        {
            if (_relation.TryGetValue(entity.Id, out var data))
            {
                modifier(data);
            }
            else
            {
                throw new KeyNotFoundException($"Collection doesn't contain entity [{entity}]");
            }
        }

        public void Remove(TEntity entity)
        {
            if (_relation.ContainsKey(entity.Id))
            {
                _relation.Remove(entity.Id);
            }
            else
            {
                throw new KeyNotFoundException($"Collection doesn't contain entity [{entity}]");
            }
        }

        public ICollection<TEntity, TData> Copy()
        {
            var relation = new Dictionary<Guid, TData>();

            foreach (var pair in _relation)
            {
                relation.Add(pair.Key, pair.Value.Copy());
            }

            return new Collection<TEntity, TData>(
                relation,
                _entityCreator,
                _dataCreator);
        }

        public IEnumerator<TEntity> GetEnumerator()
        {
            return _relation.Keys.Select(_entityCreator).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
