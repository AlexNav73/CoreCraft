using System;
using System.Threading;
using System.Threading.Tasks;

namespace PricingCalc.Model.Engine
{
    public class AsyncJobService : IJobService
    {
        public Task Enqueue(Action job)
        {
            return Task.Factory.StartNew(
                job,
                CancellationToken.None,
                TaskCreationOptions.DenyChildAttach,
                SequentialTaskScheduler.Instance);
        }

        public Task RunParallel(Action job)
        {
            return Task.Run(job);
        }
    }
}
