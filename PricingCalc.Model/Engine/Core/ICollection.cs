using System;

namespace PricingCalc.Model.Engine.Core
{
    public interface ICollection<TEntity, TData> : IEntityCollection<TEntity>, ICopy<ICollection<TEntity, TData>>
        where TEntity : IEntity, ICopy<TEntity>
        where TData : ICopy<TData>
    {
        EntityBuilder<TEntity, TData> Create();

        void Modify(TEntity entity, Action<TData> modifier);

        TData Get(TEntity entity);

        void Remove(TEntity entity);
    }
}
