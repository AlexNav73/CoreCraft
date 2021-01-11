using System;

namespace PricingCalc.Model.Engine.Core
{
    public class EntityFactory<TEntity, TData> : IEntityFactory<TEntity, TData>
        where TEntity : IEntity, new()
        where TData : new()
    {
        private readonly Func<Guid, TEntity> _entityCreator;

        public EntityFactory(Func<Guid, TEntity> entityCreator)
        {
            _entityCreator = entityCreator;
        }

        public TEntity Create()
        {
            return _entityCreator(Guid.NewGuid());
        }

        public TEntity Create(Guid id)
        {
            return _entityCreator(id);
        }

        public TData CreateData()
        {
            return new TData();
        }
    }
}
