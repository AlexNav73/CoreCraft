using System;
using System.Collections.Generic;
using System.Linq;
using PricingCalc.Core;

namespace PricingCalc.Model.Engine.Commands
{
    public abstract class ModelCommand : IModelCommand
    {
        private readonly IList<ICommandParameter> _parameters;

        public ModelCommand()
        {
            _parameters = new List<ICommandParameter>();
        }

        public void Execute()
        {
            if (_parameters.Any(x => !x.IsInitialized))
            {
                var parameter = _parameters.First(x => !x.IsInitialized);

                throw new ArgumentException($"Parameter '{parameter.Name}' is not initialized");
            }

            Run();
        }

        internal void Run(IModel model)
        {
            Logger.Information("Running '{Command}' command. Parameters {Parameters}", GetType().Name, _parameters);

            ExecuteInternal(model);
        }

        protected abstract void Run();

        protected abstract void ExecuteInternal(IModel model);

        protected ICommandParameter<T> Parameter<T>(string name)
        {
            var parameter = new CommandParameter<T>(name);
            _parameters.Add(parameter);
            return parameter;
        }
    }

    public abstract class ModelCommand<TModel> : ModelCommand
        where TModel : IBaseModel
    {
        private readonly TModel _model;
        private readonly ICommandRunner _runner;

        protected ModelCommand(TModel model, ICommandRunner runner)
        {
            _model = model;
            _runner = runner;
        }

        protected sealed override void Run()
        {
            _runner.Run(this, _model);
        }
    }
}
