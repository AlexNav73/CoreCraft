using System.Threading;
using System.Threading.Tasks;
using PricingCalc.Core;

namespace PricingCalc.Model.Engine.Commands.Runners
{
    internal class AsyncCommandRunner : IAsyncCommandRunner
    {
        public ExecutionResult Run<TModel>(ModelCommand<TModel> command, TModel model)
            where TModel : IBaseModel
        {
            var UITaskScheduler = TaskScheduler.FromCurrentSynchronizationContext();

            Task.Factory.StartNew(
                () =>
                {
                    try
                    {
                        return (model as BaseModel)!.Mutate(command.Run, false);
                    }
                    catch (System.Exception ex)
                    {
                        Logger.Error(ex, "Command execution failed");

                        return null;
                    }
                },
                CancellationToken.None,
                TaskCreationOptions.None,
                SequentialTaskScheduler.Instance)
                .ContinueWith(t =>
                {
                    try
                    {
                        (model as BaseModel)?.RaiseEvent(t.Result);
                    }
                    catch (System.Exception ex)
                    {
                        Logger.Error(ex, "Error occurred while notifying application about model changes");
                    }
                },
                UITaskScheduler);

            return ExecutionResult.Success;
        }
    }
}
