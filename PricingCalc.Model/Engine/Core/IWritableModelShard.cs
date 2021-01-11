namespace PricingCalc.Model.Engine.Core
{
    public interface IWritableModelShard : IModelShard
    {
        void Clear();
    }
}
