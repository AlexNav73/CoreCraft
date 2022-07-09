namespace Navitski.Crystalized.Model.Engine.Subscription;

/// <summary>
///     A container for changes
/// </summary>
/// <typeparam name="T">A type of changes</typeparam>
public class Message<T>
{
    /// <summary>
    ///     Ctor
    /// </summary>
    /// <param name="oldModel">An old model snapshot</param>
    /// <param name="newModel">A new model snapshot</param>
    /// <param name="changes">Changes</param>
    public Message(IModel oldModel, IModel newModel, T changes)
    {
        OldModel = oldModel;
        NewModel = newModel;
        Changes = changes;
    }

    /// <summary>
    ///     An old model snapshot
    /// </summary>
    public IModel OldModel { get; }

    /// <summary>
    ///     A new model snapshot
    /// </summary>
    public IModel NewModel { get; }

    /// <summary>
    ///     Changes
    /// </summary>
    public T Changes { get; }
}
