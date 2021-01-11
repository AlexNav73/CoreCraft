using System;
using PricingCalc.Model.Engine.Core;

namespace PricingCalc.Model.Engine.Fluent
{
    internal class CollectionCreate<TEntity, TData> : ICollectionCreate<TEntity, TData>
        where TEntity : IEntity, ICopy<TEntity>
        where TData : ICopy<TData>
    {
        private readonly ICollection<TEntity, TData> _collection;
        private readonly Guid _id;

        public CollectionCreate(
            Guid id,
            ICollection<TEntity, TData> collection)
        {
            _id = id;
            _collection = collection;
        }

        public ICollectionFinish<TEntity> Initialize(Action<TData> action)
        {
            var entity = _collection.Factory.Create(_id);
            var properties = _collection.Factory.CreateData();

            action(properties);

            return new CollectionFinish<TEntity, TData>(_collection, entity, properties);
        }
    }
}
