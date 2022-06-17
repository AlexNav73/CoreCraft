using System.Diagnostics.CodeAnalysis;

namespace Navitski.Crystalized.Model.Engine.Scheduling;

/// <summary>
///     Schedules jobs synchronously
/// </summary>
[ExcludeFromCodeCoverage]
public class SyncScheduler : IScheduler
{
    /// <summary>
    ///     Immediately starts the job
    /// </summary>
    /// <param name="job">A job to start</param>
    /// <param name="token">Cancellation token</param>
    /// <returns>A task to await</returns>
    public Task Enqueue(Action job, CancellationToken token = default)
    {
        job();

        return Task.CompletedTask;
    }

    /// <summary>
    ///     Immediately starts the job
    /// </summary>
    /// <param name="job">A job to start</param>
    /// <param name="token">Cancellation token</param>
    /// <returns>A task to await</returns>
    public Task RunParallel(Action job, CancellationToken token = default)
    {
        job();

        return Task.CompletedTask;
    }
}
