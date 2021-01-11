namespace PricingCalc.Model.Engine.Commands
{
    public interface ICommandFactory
    {
        T Create<T>() where T : IModelCommand;
    }
}
