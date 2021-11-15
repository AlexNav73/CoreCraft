namespace PricingCalc.Model.Engine.Core;

public interface ICopy<out T>
{
    T Copy();
}
