using System.Diagnostics.CodeAnalysis;

namespace CoreCraft.Scheduling;

/// <summary>
///     Schedules jobs asynchronously
/// </summary>
[ExcludeFromCodeCoverage]
public sealed class AsyncScheduler : IScheduler
{
    /// <summary>
    ///     Enqueues job into the common processing queue.
    /// </summary>
    /// <remarks>
    ///     Queue ensures that all commands, loading and saving operations
    ///     would not interfere with each other.
    /// </remarks>
    /// <param name="job">A job to schedule</param>
    /// <param name="token">Cancellation token</param>
    /// <returns>A task to await</returns>
    public Task Enqueue(Action job, CancellationToken token)
    {
        return Task.Factory.StartNew(
            job,
            token,
            TaskCreationOptions.DenyChildAttach,
            SequentialTaskScheduler.Instance);
    }

    /// <summary>
    ///     Enqueues job into the common processing queue.
    /// </summary>
    /// <param name="job">A job to schedule</param>
    /// <param name="token">Cancellation token</param>
    /// <returns>A task to await</returns>
    public Task<T> Enqueue<T>(Func<T> job, CancellationToken token)
    {
        return Task.Factory.StartNew(
            job,
            token,
            TaskCreationOptions.DenyChildAttach,
            SequentialTaskScheduler.Instance);
    }

    /// <summary>
    ///     Queues an asynchronous job for sequential execution within the scheduler.
    /// </summary>
    /// <remarks>
    ///     Jobs are executed in the order they are enqueued. If the cancellation token is triggered
    ///     before the job starts, the job will not be executed and the returned task will be canceled.
    /// </remarks>
    /// <param name="job">A delegate that represents the asynchronous operation to execute. Cannot be null.</param>
    /// <param name="token">A cancellation token that can be used to cancel the scheduled job before it starts.</param>
    /// <returns>A task that represents the scheduled job. The task completes when the job has finished executing.</returns>
    public async Task Enqueue(Func<Task> job, CancellationToken token)
    {
        var childTask = await Task.Factory.StartNew(
            job,
            token,
            TaskCreationOptions.AttachedToParent,
            SequentialTaskScheduler.Instance);

        await childTask;
    }

    /// <summary>
    ///     Starts the job in parallel.
    /// </summary>
    /// <remarks>
    ///     These jobs are safe to run in parallel.
    ///     They would not break consistency of the model
    /// </remarks>
    /// <param name="job"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    public Task RunParallel(Action job, CancellationToken token)
    {
        return Task.Run(job, token);
    }
}
