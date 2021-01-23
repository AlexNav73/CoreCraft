using System;
using System.Threading;
using System.Threading.Tasks;

namespace PricingCalc.Model.Engine
{
    public class AsyncJobService : IJobService
    {
        public Task<T> Enqueue<T>(Func<T> job)
        {
            return Task.Factory.StartNew(
                job,
                CancellationToken.None,
                TaskCreationOptions.DenyChildAttach,
                SequentialTaskScheduler.Instance);
        }

        public Task Enqueue(Action job)
        {
            return Task.Factory.StartNew(
                job,
                CancellationToken.None,
                TaskCreationOptions.DenyChildAttach,
                SequentialTaskScheduler.Instance);
        }
    }
}
