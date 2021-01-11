namespace PricingCalc.Model.Engine.Commands
{
    public interface ICommandRunner
    {
        ExecutionResult Run<TModel>(ModelCommand<TModel> command, TModel model)
            where TModel : IBaseModel;
    }
}
