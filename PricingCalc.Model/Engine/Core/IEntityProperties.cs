namespace PricingCalc.Model.Engine.Core
{
    public interface IEntityProperties
    {
        void WriteTo(IPropertiesBag bag);

        void ReadFrom(IPropertiesBag bag);
    }
}
