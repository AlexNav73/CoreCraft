﻿using System.Diagnostics.CodeAnalysis;

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
