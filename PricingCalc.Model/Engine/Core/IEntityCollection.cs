using System.Collections.Generic;

namespace PricingCalc.Model.Engine.Core
{
    public interface IEntityCollection<out TEntity> : IEnumerable<TEntity>
        where TEntity : IEntity
    {
        int Count { get; }
    }
}
