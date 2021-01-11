using System;
using PricingCalc.Core;

namespace PricingCalc.Model.Engine.Commands
{
    internal class InstantCommandRunner : ICommandRunner
    {
        public ExecutionResult Run(IBaseModel model, Action<IModel> action)
        {
            try
            {
                ((BaseModel)model).View.Mutate(action);

                return ExecutionResult.Success;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Command execution failed");
                return ExecutionResult.Error(ex.Message);
            }
        }
    }
}
