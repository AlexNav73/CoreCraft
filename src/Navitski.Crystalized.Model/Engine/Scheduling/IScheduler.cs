namespace Navitski.Crystalized.Model.Engine.Scheduling;

/// <summary>
///     A scheduler is a logic which decides how to schedule jobs (for example synchronously or asynchronously)
/// </summary>
/// <remarks>
///     Depending of the application type, different approaches can be used.
///     For a single threaded applications, all jobs can be run immediately
///     like in unit tests. In multi-threaded applications some jobs can be run
///     in the Thread Pool like commands, loading of the model or applying changes
///     to the model. It can shift some long running jobs from the main thread to
///     the Thread Pool threads.
/// </remarks>
public interface IScheduler
{
    /// <summary>
    ///     Enqueues the job to be processed sequentially after all previous jobs will be finished
    /// </summary>
    /// <remarks>
    ///     This method is used to schedule command execution. Commands must
    ///     be executed sequentially one after another to provide consistency
    ///     of the data. An implementation of this method should run jobs
    ///     in a way to guarantee the consistency of the data between running jobs.
    /// </remarks>
    /// <param name="job">A job to run</param>
    /// <param name="token">Cancellation token</param>
    Task Enqueue(Action job, CancellationToken token);

    /// <summary>
    ///     Runs the job in parallel to the execution of jobs,
    ///     scheduled by <see cref="Enqueue(Action, CancellationToken)"/> method
    /// </summary>
    /// <remarks>
    ///     This method is used for jobs which don't break consistency of the model
    ///     if they will run in parallel to the jobs scheduled by <see cref="Enqueue(Action, CancellationToken)"/> method
    /// </remarks>
    /// <param name="job">A job to run</param>
    /// <param name="token">Cancellation token</param>
    Task RunParallel(Action job, CancellationToken token);
}
