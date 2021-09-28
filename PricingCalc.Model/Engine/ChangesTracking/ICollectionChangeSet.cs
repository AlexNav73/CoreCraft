using System.Collections.Generic;
using PricingCalc.Model.Engine.Core;

namespace PricingCalc.Model.Engine.ChangesTracking
{
    public interface ICollectionChangeSet<TEntity, TData> : IEnumerable<ICollectionChange<TEntity, TData>>
        where TEntity : Entity
        where TData : Properties
    {
        void Add(CollectionAction action, TEntity entity, TData? oldData, TData? newData);

        ICollectionChangeSet<TEntity, TData> Invert();

        void Apply(ICollection<TEntity, TData> collection);

        bool HasChanges();
    }
}
