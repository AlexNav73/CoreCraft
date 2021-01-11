using System;

namespace PricingCalc.Model.Engine.Core
{
    public interface IEntityFactory<out TEntity>
        where TEntity : IEntity
    {
        TEntity Create();

        TEntity Create(Guid id);
    }

    public interface IEntityFactory<out TEntity, out TData> : IEntityFactory<TEntity>
        where TEntity : IEntity
    {
        TData CreateData();
    }
}
