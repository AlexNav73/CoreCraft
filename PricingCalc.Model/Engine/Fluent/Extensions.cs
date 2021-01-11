using System;
using PricingCalc.Model.Engine.Core;

namespace PricingCalc.Model.Engine.Fluent
{
    public static class Extensions
    {
        public static ICollectionCreate<TEntity, TData> Create<TEntity, TData>(this ICollection<TEntity, TData> collection)
            where TEntity : IEntity, ICopy<TEntity>
            where TData : ICopy<TData>
        {
            return new CollectionCreate<TEntity, TData>(Guid.NewGuid(), collection);
        }

        public static ICollectionCreate<TEntity, TData> Create<TEntity, TData>(this ICollection<TEntity, TData> collection, Guid id)
            where TEntity : IEntity, ICopy<TEntity>
            where TData : ICopy<TData>
        {
            return new CollectionCreate<TEntity, TData>(id, collection);
        }
    }
}
