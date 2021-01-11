using System.Collections.Generic;

namespace PricingCalc.Model.Engine.Core
{
    public interface IEntityCollection<TEntity> : IEnumerable<TEntity>
        where TEntity : IEntity
    {
        int IndexOf(TEntity entity);

        TEntity ElementAt(int index);
    }
}
