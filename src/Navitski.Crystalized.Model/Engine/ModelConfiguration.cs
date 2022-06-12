namespace Navitski.Crystalized.Model.Engine;

/// <summary>
///     Configuration of the model
/// </summary>
public class ModelConfiguration
{
    public ModelConfiguration(IScheduler? scheduler = null)
    {
        Scheduler = scheduler ?? new AsyncScheduler();
    }

    /// <summary>
    ///     Scheduler
    /// </summary>
    public IScheduler Scheduler { get; }
}
