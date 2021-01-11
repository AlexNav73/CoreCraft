using System;

namespace PricingCalc.Model.Engine.Commands
{
    public interface ICommandRunner
    {
        ExecutionResult Run(IBaseModel model, Action<IModel> action);
    }
}
