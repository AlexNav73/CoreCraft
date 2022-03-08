namespace PricingCalc.Model.Engine.Core;

public abstract record Properties
{
    public abstract void WriteTo(IPropertiesBag bag);

    public abstract Properties ReadFrom(IPropertiesBag bag);
}
