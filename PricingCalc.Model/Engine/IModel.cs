namespace PricingCalc.Model.Engine;

public interface IModel : IModelShardAccessor, IEnumerable<IModelShard>
{
}
