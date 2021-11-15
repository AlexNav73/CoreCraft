namespace PricingCalc.Model.Engine.Commands;

internal interface IRunnable
{
    void Run(IModel model);
}
