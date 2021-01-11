using System.Collections.Generic;
using PricingCalc.Model.Engine.Core;

namespace PricingCalc.Model.Engine.ChangesTracking
{
    public interface ICollectionChanges<TEntity, TData> : IEnumerable<IEntityChange<TEntity, TData>>
        where TEntity : IEntity, ICopy<TEntity>
        where TData : ICopy<TData>
    {
        void Add(EntityAction action, TEntity entity, TData oldData, TData newData);

        ICollectionChanges<TEntity, TData> Invert();

        void Apply(ICollection<TEntity, TData> collection);

        bool HasChanges();
    }
}
