using System;

namespace PricingCalc.Model.Engine.Core
{
    public interface IEntityBuilder<TEntity, TData>
        where TEntity : IEntity
    {
        IEntityBuilder<TEntity, TData> WithId(Guid id);

        IEntityBuilder<TEntity, TData> WithInit(Action<TData> initializer);

        TEntity Build();
    }
}
