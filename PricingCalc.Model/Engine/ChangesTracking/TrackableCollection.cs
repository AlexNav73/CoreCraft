using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using PricingCalc.Model.Engine.Core;

namespace PricingCalc.Model.Engine.ChangesTracking
{
    [DebuggerDisplay("{_collection}")]
    public class TrackableCollection<TEntity, TData> : ICollection<TEntity, TData>
        where TEntity : IEntity, ICopy<TEntity>
        where TData : ICopy<TData>, IEquatable<TData>
    {
        private readonly ICollectionChanges<TEntity, TData> _changes;
        private readonly ICollection<TEntity, TData> _collection;

        public TrackableCollection(ICollectionChanges<TEntity, TData> changesCollection,
            ICollection<TEntity, TData> modelCollection)
        {
            _changes = changesCollection;
            _collection = modelCollection;
        }

        public int Count => _collection.Count;

        public IEntityBuilder<TEntity, TData> Create()
        {
            return new EntityBuilder<TEntity, TData>(this, (IFactory<TEntity, TData>)_collection);
        }

        public void Add(TEntity entity, TData data)
        {
#pragma warning disable CS8604 // Possible null reference argument.
            _changes.Add(EntityAction.Add, entity, default, data);
#pragma warning restore CS8604 // Possible null reference argument.
            _collection.Add(entity, data);
        }

        public TData Get(TEntity entity)
        {
            return _collection.Get(entity);
        }

        public void Modify(TEntity entity, Action<TData> modifier)
        {
            var oldData = _collection.Get(entity).Copy();
            _collection.Modify(entity, modifier);
            var newData = _collection.Get(entity).Copy();

            if (!oldData.Equals(newData))
            {
                _changes.Add(EntityAction.Modify, entity, oldData, newData);
            }
        }

        public void Remove(TEntity entity)
        {
            var data = _collection.Get(entity);
            _changes.Add(EntityAction.Remove, entity, data, default!);
            _collection.Remove(entity);
        }

        public IEnumerator<TEntity> GetEnumerator()
        {
            return _collection.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public ICollection<TEntity, TData> Copy()
        {
            throw new InvalidOperationException("Collection can't be copied because it is attached to changes tracking system");
        }
    }
}
