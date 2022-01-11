namespace PricingCalc.Model.Engine;

public class ModelConfiguration
{
    public ModelConfiguration()
    {
        Scheduler = new AsyncScheduler();
    }

    public IScheduler Scheduler { get; init; }
}
