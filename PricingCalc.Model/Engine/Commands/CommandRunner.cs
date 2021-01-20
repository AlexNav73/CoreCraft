using System;
using System.Threading;
using System.Threading.Tasks;
using PricingCalc.Core;

namespace PricingCalc.Model.Engine.Commands
{
    internal class CommandRunner : ICommandRunner
    {
        public void Run<TModel>(ModelCommand<TModel> command, TModel model)
            where TModel : IBaseModel
        {
            var UITaskScheduler = TaskScheduler.FromCurrentSynchronizationContext();

            Task.Factory.StartNew(
                () => RunCommand(command, model, UITaskScheduler),
                CancellationToken.None,
                TaskCreationOptions.DenyChildAttach,
                SequentialTaskScheduler.Instance);
        }

        private static void RunCommand<TModel>(ModelCommand<TModel> command, TModel model, TaskScheduler scheduler)
            where TModel : IBaseModel
        {
            try
            {
                var result = (model as BaseModel)!.Run(command);

                Task.Factory.StartNew(
                    () => (model as BaseModel)?.RaiseEvent(result),
                    CancellationToken.None,
                    TaskCreationOptions.None,
                    scheduler);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Command execution failed");
            }
        }
    }
}
