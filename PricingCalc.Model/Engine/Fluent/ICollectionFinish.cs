using PricingCalc.Model.Engine.Core;

namespace PricingCalc.Model.Engine.Fluent
{
    public interface ICollectionFinish<TEntity>
        where TEntity : IEntity, ICopy<TEntity>
    {
        TEntity Finish();
    }
}
