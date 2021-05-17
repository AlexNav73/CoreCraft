using System;

namespace PricingCalc.Model.Engine.Core
{
    public sealed class EntityBuilder<TEntity, TData>
        where TEntity : IEntity, ICopy<TEntity>
        where TData : ICopy<TData>
    {
        private readonly Action<TEntity, TData> _adder;
        private readonly IFactory<TEntity, TData> _factory;
        private readonly Guid _id;
        private readonly Action<TData> _initializer;

        internal EntityBuilder(
            Action<TEntity, TData> adder,
            IFactory<TEntity, TData> factory)
            : this(adder, factory, Guid.NewGuid(), p => { })
        {
        }

        private EntityBuilder(
            Action<TEntity, TData> adder,
            IFactory<TEntity, TData> factory,
            Guid id,
            Action<TData> initializer)
        {
            _adder = adder;
            _factory = factory;
            _id = id;
            _initializer = initializer;
        }

        public EntityBuilder<TEntity, TData> WithId(Guid id)
        {
            return new EntityBuilder<TEntity, TData>(_adder, _factory, id, _initializer);
        }

        public EntityBuilder<TEntity, TData> WithInit(Action<TData> initializer)
        {
            return new EntityBuilder<TEntity, TData>(_adder, _factory, _id, initializer);
        }

        internal EntityBuilder<TEntity, TData> WithAddHook(Action<TEntity, TData> hook)
        {
            return new EntityBuilder<TEntity, TData>(
                (e, p) =>
                {
                    _adder(e, p);
                    hook(e, p);
                },
                _factory,
                _id,
                _initializer);
        }

        public TEntity Build()
        {
            var entity = _factory.EntityFactory(_id);
            var data = _factory.DataFactory();

            _initializer(data);

            _adder(entity, data);

            return entity;
        }

        internal void Finish(TEntity entity, TData data)
        {
            _adder(entity, data);
        }
    }
}
