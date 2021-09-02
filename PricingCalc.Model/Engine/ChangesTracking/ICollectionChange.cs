using PricingCalc.Model.Engine.Core;

namespace PricingCalc.Model.Engine.ChangesTracking
{
    public interface ICollectionChange<TEntity, TData>
        where TEntity : IEntity
        where TData : ICopy<TData>
    {
        CollectionAction Action { get; }

        TEntity Entity { get; }

        TData? OldData { get; }

        TData? NewData { get; }

        ICollectionChange<TEntity, TData> Invert();
    }
}
