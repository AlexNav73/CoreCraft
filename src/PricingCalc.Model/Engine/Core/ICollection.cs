namespace PricingCalc.Model.Engine.Core;

public interface ICollection<TEntity, TData> : IEnumerable<TEntity>, ICopy<ICollection<TEntity, TData>>
    where TEntity : Entity
    where TData : Properties
{
    int Count { get; }

    TData Get(TEntity entity);
}
