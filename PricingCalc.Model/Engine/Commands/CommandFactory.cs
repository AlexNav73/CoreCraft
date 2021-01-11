using Autofac;

namespace PricingCalc.Model.Engine.Commands
{
    internal class CommandFactory : ICommandFactory
    {
        private readonly ILifetimeScope _scope;

        public CommandFactory(ILifetimeScope scope)
        {
            _scope = scope;
        }

        public T Create<T>() where T : IModelCommand
        {
            return _scope.Resolve<T>();
        }
    }
}
