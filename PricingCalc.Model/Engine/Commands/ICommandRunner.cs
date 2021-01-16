namespace PricingCalc.Model.Engine.Commands
{
    public interface ICommandRunner
    {
        void Run<TModel>(ModelCommand<TModel> command, TModel model)
            where TModel : IBaseModel;
    }
}
