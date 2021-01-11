using System;

namespace PricingCalc.Model.Engine.Core
{
    public interface ICollection<TEntity, TData> : IEntityCollection<TEntity>
        where TEntity : IEntity, ICopy<TEntity>
        where TData : ICopy<TData>
    {
        IEntityFactory<TEntity, TData> Factory { get; }

        void Add(TEntity entity, TData data);

        void Modify(TEntity entity, Action<TData> modifier);

        TData Get(TEntity entity);

        void Remove(TEntity entity);

        void Clear();
    }
}
