using System;

namespace PricingCalc.Model.Engine.Core
{
    internal sealed class EntityBuilder<TEntity, TData> : IEntityBuilder<TEntity, TData>
        where TEntity : IEntity, ICopy<TEntity>
        where TData : ICopy<TData>
    {
        private readonly ICollectionInternal<TEntity, TData> _collection;
        private readonly IFactory<TEntity, TData> _factory;
        private readonly Guid _id;
        private readonly Action<TData> _initializer;

        public EntityBuilder(ICollectionInternal<TEntity, TData> collection, IFactory<TEntity, TData> factory)
            : this(collection, factory, Guid.NewGuid(), p => { })
        {
        }

        private EntityBuilder(
            ICollectionInternal<TEntity, TData> collection,
            IFactory<TEntity, TData> factory,
            Guid id,
            Action<TData> initializer)
        {
            _collection = collection;
            _factory = factory;
            _id = id;
            _initializer = initializer;
        }

        public IEntityBuilder<TEntity, TData> WithId(Guid id)
        {
            return new EntityBuilder<TEntity, TData>(_collection, _factory, id, _initializer);
        }

        public IEntityBuilder<TEntity, TData> WithInit(Action<TData> initializer)
        {
            return new EntityBuilder<TEntity, TData>(_collection, _factory, _id, initializer);
        }

        public TEntity Build()
        {
            var entity = _factory.EntityFactory(_id);
            var data = _factory.DataFactory();

            _initializer(data);

            _collection.Add(entity, data);

            return entity;
        }
    }
}
