using System;
using PricingCalc.Model.Engine.Core;

namespace PricingCalc.Model.Engine.Fluent
{
    public interface ICollectionCreate<TEntity, TData>
        where TEntity : IEntity, ICopy<TEntity>
        where TData : ICopy<TData>
    {
        ICollectionFinish<TEntity> Initialize(Action<TData> action);
    }
}
