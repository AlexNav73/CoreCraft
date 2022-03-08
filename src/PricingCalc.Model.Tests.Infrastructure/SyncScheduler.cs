using PricingCalc.Model.Engine;

namespace PricingCalc.Model.Tests.Infrastructure;

public class SyncScheduler : IScheduler
{
    public Task Enqueue(Action job)
    {
        job();

        return Task.CompletedTask;
    }

    public Task RunParallel(Action job)
    {
        throw new NotImplementedException();
    }
}
