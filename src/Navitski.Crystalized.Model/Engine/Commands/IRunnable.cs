namespace Navitski.Crystalized.Model.Engine.Commands;

internal interface IRunnable
{
    void Run(IModel model, CancellationToken token);
}
