namespace PricingCalc.Model.Engine.Persistence;

public interface IHaveStorage
{
    IModelShardStorage Storage { get; }
}
