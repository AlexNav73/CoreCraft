namespace PricingCalc.Model.Engine.Commands
{
    public interface ICommandRunner
    {
        void Run(ModelCommand command, IBaseModel model);
    }
}
