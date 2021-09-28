using System;
using System.Threading.Tasks;
using PricingCalc.Model.Engine;

namespace PricingCalc.Model.Tests.Infrastructure
{
    public class SyncJobService : IJobService
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
}
