namespace Navitski.Crystalized.Model.Engine.Commands;

public abstract class ModelCommand<TModel> : IModelCommand, IRunnable
    where TModel : IBaseModel
{
    private readonly ICommandRunner _model;
    private readonly IList<ICommandParameter> _parameters;

    protected ModelCommand(TModel model)
    {
        _model = (ICommandRunner)model;
        _parameters = new List<ICommandParameter>();
    }

    public Task Execute(CancellationToken token = default)
    {
        AssertParameters();

        return _model.Enqueue(this, token);
    }

    void IRunnable.Run(IModel model)
    {
        ExecuteInternal(model);
    }

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
