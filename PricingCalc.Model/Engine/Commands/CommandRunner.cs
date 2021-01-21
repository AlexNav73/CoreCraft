using System;
using System.Threading;
using System.Threading.Tasks;
using PricingCalc.Core;

namespace PricingCalc.Model.Engine.Commands
{
    internal class CommandRunner : ICommandRunner
    {
        public void Run(ModelCommand command, IBaseModel model)
        {
            var UITaskScheduler = TaskScheduler.FromCurrentSynchronizationContext();

            Task.Factory.StartNew(
                () => RunCommand(command, model, UITaskScheduler),
                CancellationToken.None,
                TaskCreationOptions.DenyChildAttach,
                SequentialTaskScheduler.Instance);
        }

        private static void RunCommand(ModelCommand command, IBaseModel model, TaskScheduler scheduler)
        {
            try
            {
                var result = model.Run(command);

                Task.Factory.StartNew(
                    () => model.RaiseEvent(result),
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
