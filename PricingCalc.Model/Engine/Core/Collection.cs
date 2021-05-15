using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace PricingCalc.Model.Engine.Core
{
    [DebuggerDisplay("Count = {Count}")]
    public class Collection<TEntity, TData> : ICollection<TEntity, TData>, IFactory<TEntity, TData>
        where TEntity : IEntity, ICopy<TEntity>
        where TData : ICopy<TData>
    {
        private readonly Func<Guid, TEntity> _entityCreator;
        private readonly Func<TData> _dataCreator;

        // TODO: Can the _relation be removed without performance degradation?
        private readonly IDictionary<Guid, int> _relation;
        private readonly IList<TEntity> _entities;
        private readonly IList<TData> _data;

        public Collection(Func<Guid, TEntity> entityCreator, Func<TData> dataCreator)
            : this(new Dictionary<Guid, int>(), new List<TEntity>(), new List<TData>(), entityCreator, dataCreator)
        {
        }

        private Collection(
            IDictionary<Guid, int> relation,
            IList<TEntity> entities,
            IList<TData> data,
            Func<Guid, TEntity> entityCreator,
            Func<TData> dataCreator)
        {
            _relation = relation;
            _entities = entities;
            _data = data;
            _entityCreator = entityCreator;
            _dataCreator = dataCreator;
        }

        public int Count => _entities.Count;

        public Func<Guid, TEntity> EntityFactory => _entityCreator;

        public Func<TData> DataFactory => _dataCreator;

        public IEntityBuilder<TEntity, TData> Create()
        {
            return new EntityBuilder<TEntity, TData>(this, this);
        }

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

            throw new KeyNotFoundException($"Collection doesn't contain entity [{entity}]");
        }

        public void Modify(TEntity entity, Action<TData> modifier)
        {
            if (_relation.TryGetValue(entity.Id, out var index))
            {
                modifier(_data[index]);
            }
            else
            {
                throw new KeyNotFoundException($"Collection doesn't contain entity [{entity}]");
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

        public ICollection<TEntity, TData> Copy()
        {
            var i = 0;
            var relation = new Dictionary<Guid, int>();
            var entities = new List<TEntity>(_entities.Count);
            var data = new List<TData>(_data.Count);

            foreach (var pair in _relation)
            {
                relation.Add(pair.Key, pair.Value);
                entities.Add(_entities[i].Copy());
                data.Add(_data[i].Copy());

                i++;
            }

            return new Collection<TEntity, TData>(
                relation,
                entities,
                data,
                _entityCreator,
                _dataCreator);
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
