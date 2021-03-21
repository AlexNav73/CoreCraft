using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PricingCalc.Core;

namespace PricingCalc.Model.Engine.Commands
{
    public abstract class ModelCommand : IModelCommand
    {
        private readonly IList<ICommandParameter> _parameters;

        protected ModelCommand()
        {
            _parameters = new List<ICommandParameter>();
        }

        public async void Execute()
        {
            AssertParameters();

            await Run();
        }

        internal void Run(IModel model)
        {
            Logger.Information("Running '{Command}' command. Parameters {Parameters}", GetType().Name, _parameters);

            ExecuteInternal(model);
        }

        protected abstract Task Run();

        protected abstract void ExecuteInternal(IModel model);

        protected ICommandParameter<T> Parameter<T>(string name)
        {
            var parameter = new CommandParameter<T>(name);
            _parameters.Add(parameter);
            return parameter;
        }

        private void AssertParameters()
        {
            if (_parameters.Any(x => !x.IsInitialized))
            {
                var parameter = _parameters.First(x => !x.IsInitialized);

                throw new ArgumentException($"Parameter '{parameter.Name}' is not initialized");
            }
        }
    }

    public abstract class ModelCommand<TModel> : ModelCommand
        where TModel : IBaseModel
    {
        private readonly TModel _model;

        protected ModelCommand(TModel model)
        {
            _model = model;
        }

        protected sealed override async Task Run()
        {
            await _model.Run(this);
        }
    }
}
