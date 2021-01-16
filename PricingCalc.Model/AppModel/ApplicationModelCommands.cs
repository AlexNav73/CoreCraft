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

        public void Clear()
        {
            var command = _commandFactory.Create<IClearModelCommand<IApplicationModel>>();
            command.Execute();
        }
    }
}
