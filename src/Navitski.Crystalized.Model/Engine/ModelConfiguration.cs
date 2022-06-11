namespace Navitski.Crystalized.Model.Engine;

/// <summary>
///     Configuration of the model
/// </summary>
public class ModelConfiguration
{
    public ModelConfiguration()
    {
        Scheduler = new AsyncScheduler();
    }

    /// <summary>
    ///     Scheduler
    /// </summary>
    public IScheduler Scheduler { get; init; }
}
