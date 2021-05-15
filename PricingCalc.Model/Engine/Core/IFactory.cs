using System;

namespace PricingCalc.Model.Engine.Core
{
    internal interface IFactory<TEntity, TData>
        where TEntity : IEntity, ICopy<TEntity>
        where TData : ICopy<TData>
    {
        Func<Guid, TEntity> EntityFactory { get; }

        Func<TData> DataFactory { get; }
    }
}
