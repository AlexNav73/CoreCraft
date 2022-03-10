namespace Navitski.Crystalized.Model.Engine;

public interface IScheduler
{
    Task Enqueue(Action job);

    Task RunParallel(Action job);
}
