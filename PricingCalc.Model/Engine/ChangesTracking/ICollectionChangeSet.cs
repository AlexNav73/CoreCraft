using System.Collections.Generic;
using PricingCalc.Model.Engine.Core;

namespace PricingCalc.Model.Engine.ChangesTracking
{
    public interface ICollectionChangeSet<TEntity, TData> : IEnumerable<ICollectionChange<TEntity, TData>>
        where TEntity : IEntity, ICopy<TEntity>
        where TData : ICopy<TData>
    {
        void Add(CollectionAction action, TEntity entity, TData? oldData, TData? newData);

        ICollectionChangeSet<TEntity, TData> Invert();

        void Apply(ICollection<TEntity, TData> collection);

        bool HasChanges();
    }
}
