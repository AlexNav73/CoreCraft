using System;

namespace PricingCalc.Model.Engine.Core
{
    internal interface IFactory<TEntity, TData>
        where TEntity : IEntity
    {
        Func<Guid, TEntity> EntityFactory { get; }

        Func<TData> DataFactory { get; }
    }
}
