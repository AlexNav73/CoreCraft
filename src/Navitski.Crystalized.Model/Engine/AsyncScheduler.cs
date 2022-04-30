namespace Navitski.Crystalized.Model.Engine;

internal class AsyncScheduler : IScheduler
{
    public Task Enqueue(Action job, CancellationToken token)
    {
        return Task.Factory.StartNew(
            job,
            token,
            TaskCreationOptions.DenyChildAttach,
            SequentialTaskScheduler.Instance);
    }

    public Task RunParallel(Action job, CancellationToken token)
    {
        return Task.Run(job, token);
    }
}
