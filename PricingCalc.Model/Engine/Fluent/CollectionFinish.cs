using PricingCalc.Model.Engine.Core;

namespace PricingCalc.Model.Engine.Fluent
{
    internal class CollectionFinish<TEntity, TData> : ICollectionFinish<TEntity>
        where TEntity : IEntity, ICopy<TEntity>
        where TData : ICopy<TData>
    {
        private readonly ICollection<TEntity, TData> _collection;
        private readonly TEntity _entity;
        private readonly TData _properties;

        public CollectionFinish(ICollection<TEntity, TData> collection, TEntity entity, TData properties)
        {
            _collection = collection;
            _entity = entity;
            _properties = properties;
        }

        public TEntity Finish()
        {
            _collection.Add(_entity, _properties);

            return _entity;
        }
    }
}
