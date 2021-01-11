using System;
using System.Collections.Generic;
using System.Linq;
using PricingCalc.Core;

namespace PricingCalc.Model.Engine.Commands
{
    public abstract class ModelCommand<TModel> : IModelCommand
        where TModel : IBaseModel
    {
        private readonly TModel _model;
        private readonly ICommandRunner _runner;
        private readonly IList<ICommandParameter> _parameters;

        protected ModelCommand(TModel model, ICommandRunner runner)
        {
            _model = model;
            _runner = runner;
            _parameters = new List<ICommandParameter>();
        }

        public virtual ExecutionResult Execute()
        {
            if (_parameters.Any(x => !x.IsInitialized))
            {
                var parameter = _parameters.First(x => !x.IsInitialized);

                throw new ArgumentException($"Parameter '{parameter.Name}' is not initialized");
            }

            Logger.Information("Running '{Command}' command. Parameters {Parameters}", GetType().Name, _parameters);

            return _runner.Run(_model, ExecuteInternal);
        }

        protected abstract void ExecuteInternal(IModel model);

        protected ICommandParameter<T> Parameter<T>(string name)
        {
            var parameter = new CommandParameter<T>(name);
            _parameters.Add(parameter);
            return parameter;
        }
    }
}
