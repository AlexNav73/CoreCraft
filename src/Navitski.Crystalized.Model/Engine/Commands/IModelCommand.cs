namespace Navitski.Crystalized.Model.Engine.Commands;

public interface IModelCommand
{
    Task Execute(CancellationToken token = default);
}
