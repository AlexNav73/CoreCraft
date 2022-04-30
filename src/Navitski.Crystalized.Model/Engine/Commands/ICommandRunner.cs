namespace Navitski.Crystalized.Model.Engine.Commands;

internal interface ICommandRunner
{
    Task Enqueue(IRunnable runnable, CancellationToken token);
}
