namespace Navitski.Crystalized.Model.Engine.Commands;

/// <summary>
///     A command is a atomic pies of logic which modifies a domain model
/// </summary>
/// <remarks>
///     A model for the whole application is a read-only piece of data.
///     Commands are used for model modification. A command is a small piece of logic
///     which will only modify the model, therefore it doesn't return any values.
///     Using the separation of reading and writing, it is possible to run
///     commands in the another thread and don't block the whole application.
///     Commands are executed sequentially and each time a command finishes,
///     subscribers of a model receive notification that model has been changed
///     and all recorded changes are propagated to the subscribers to analyze
///     what was changed and how to react to these changes.
/// </remarks>
public interface ICommand
{
    /// <summary>
    ///     Command business logic
    /// </summary>
    /// <remarks>
    ///     <see cref="IModel"/> in this case, contains mutable versions of
    ///     model shards. For example: if there is a "IExampleModelShard", a
    ///     command can only extract "IMutableExampleModelShard" from the
    ///     <see cref="IModel"/> object.
    /// </remarks>
    /// <param name="model">Mutable model</param>
    /// <param name="token">Cancellation token</param>
    void Execute(IModel model, CancellationToken token);
}
