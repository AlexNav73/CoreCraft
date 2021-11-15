namespace PricingCalc.Model.Engine.Core;

public interface IEntityCollection<out TEntity> : IEnumerable<TEntity>
    where TEntity : Entity
{
    int Count { get; }
}
