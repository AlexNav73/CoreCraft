namespace PricingCalc.Model.Engine.Core;

public interface ICollection<TEntity, TData> : IEntityCollection<TEntity>, ICopy<ICollection<TEntity, TData>>
    where TEntity : Entity
    where TData : Properties
{
    TData Get(TEntity entity);
}
