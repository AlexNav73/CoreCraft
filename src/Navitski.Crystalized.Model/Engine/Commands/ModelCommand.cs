namespace Navitski.Crystalized.Model.Engine.Commands;

/// <inheritdoc cref="IModelCommand"/>
public abstract class ModelCommand<TModel> : IModelCommand, IRunnable
    where TModel : IDomainModel
{
    private readonly ICommandRunner _model;
    private readonly IList<ICommandParameter> _parameters;

    /// <summary>
    ///     Ctor
    /// </summary>
    protected ModelCommand(TModel model)
    {
        _model = (ICommandRunner)model;
        _parameters = new List<ICommandParameter>();
    }

    /// <inheritdoc />
    public Task Execute(CancellationToken token = default)
    {
        AssertParameters();

        return _model.Enqueue(this, token);
    }

    void IRunnable.Run(IModel model, CancellationToken token)
    {
        ExecuteInternal(model, token);
    }

    /// <summary>
    ///     Command business logic
    /// </summary>
    /// <param name="model">Mutable model</param>
    /// <param name="token">Cancellation token</param>
    protected abstract void ExecuteInternal(IModel model, CancellationToken token);

    /// <summary>
    ///     Command parameter factory method
    /// </summary>
    /// <typeparam name="T">A type of parameter value</typeparam>
    /// <param name="name">Name of parameter</param>
    /// <returns>Wrapper for a parameter value</returns>
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
