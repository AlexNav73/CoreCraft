namespace Navitski.Crystalized.Model.Engine;

public interface IScheduler
{
    Task Enqueue(Action job, CancellationToken token);

    Task RunParallel(Action job, CancellationToken token);
}
