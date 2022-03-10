namespace Navitski.Crystalized.Model.Engine;

internal class AsyncScheduler : IScheduler
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
