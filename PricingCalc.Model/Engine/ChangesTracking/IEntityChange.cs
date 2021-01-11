using PricingCalc.Model.Engine.Core;

namespace PricingCalc.Model.Engine.ChangesTracking
{
    public interface IEntityChange<TEntity, TData>
        where TEntity : IEntity
        where TData : ICopy<TData>
    {
        EntityAction Action { get; }

        TEntity Entity { get; }

        TData OldData { get; }

        TData NewData { get; }

        IEntityChange<TEntity, TData> Invert();
    }
}
