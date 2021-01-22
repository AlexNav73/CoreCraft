using System;
using System.Threading;
using System.Threading.Tasks;
using PricingCalc.Core;

namespace PricingCalc.Model.Engine
{
    internal class JobService : IJobService
    {
        public Task StartNew<T>(Func<T> job, Action<T> continueWith)
        {
            var uiTaskScheduler = TaskScheduler.FromCurrentSynchronizationContext();

            return Task.Factory.StartNew(
                () =>
                {
                    try
                    {
                        var result = job();

                        Task.Factory.StartNew(
                            () => continueWith(result),
                            CancellationToken.None,
                            TaskCreationOptions.None,
                            uiTaskScheduler);
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex, "Job failed to execute");
                    }
                },
                CancellationToken.None,
                TaskCreationOptions.DenyChildAttach,
                SequentialTaskScheduler.Instance);
        }

        public Task StartNew(Action job, Action continueWith)
        {
            var uiTaskScheduler = TaskScheduler.FromCurrentSynchronizationContext();

            return Task.Factory.StartNew(
                () =>
                {
                    try
                    {
                        job();

                        Task.Factory.StartNew(
                            continueWith,
                            CancellationToken.None,
                            TaskCreationOptions.None,
                            uiTaskScheduler);
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex, "Job failed to execute");
                    }
                },
                CancellationToken.None,
                TaskCreationOptions.DenyChildAttach,
                SequentialTaskScheduler.Instance);
        }
    }
}
