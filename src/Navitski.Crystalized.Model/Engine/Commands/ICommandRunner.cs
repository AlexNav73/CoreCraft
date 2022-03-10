namespace Navitski.Crystalized.Model.Engine.Commands;

internal interface ICommandRunner
{
    Task Run(IRunnable runnable);
}
