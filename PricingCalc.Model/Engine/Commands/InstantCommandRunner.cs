using System;
using PricingCalc.Core;

namespace PricingCalc.Model.Engine.Commands
{
    internal class InstantCommandRunner : ICommandRunner
    {
        public ExecutionResult Run<TModel>(ModelCommand<TModel> command, TModel model)
            where TModel : IBaseModel
        {
            try
            {
                if (model is BaseModel baseModel)
                {
                    baseModel.View.Mutate(command.Run);

                    return ExecutionResult.Success;
                }

                throw new InvalidOperationException($"Unable to run command on model of type {model.GetType()}");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Command execution failed");
                return ExecutionResult.Error(ex.Message);
            }
        }
    }
}
