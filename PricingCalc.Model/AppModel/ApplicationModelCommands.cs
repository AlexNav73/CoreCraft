using PricingCalc.Model.Engine.Commands;
using PricingCalc.Model.GenericCommands;

namespace PricingCalc.Model.AppModel
{
    internal class ApplicationModelCommands : IApplicationModelCommands
    {
        private readonly ICommandFactory _commandFactory;

        public ApplicationModelCommands(ICommandFactory commandFactory)
        {
            _commandFactory = commandFactory;
        }

        public ExecutionResult Clear()
        {
            var command = _commandFactory.Create<IClearModelCommand<IApplicationModel>>();
            return command.Execute();
        }
    }
}
