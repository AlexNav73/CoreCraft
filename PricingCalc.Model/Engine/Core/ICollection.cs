using System;

namespace PricingCalc.Model.Engine.Core
{
    public interface ICollection<TEntity, TData> : IEntityCollection<TEntity>, ICopy<ICollection<TEntity, TData>>
        where TEntity : Entity
        where TData : Properties
    {
        TEntity Add(TData data);

        TEntity Add(Guid id, Func<TData, TData> init);

        void Add(TEntity entity, TData data);

        void Modify(TEntity entity, Func<TData, TData> modifier);

        TData Get(TEntity entity);

        void Remove(TEntity entity);
    }
}
