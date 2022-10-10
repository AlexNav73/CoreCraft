namespace Navitski.Crystalized.Model.Engine.ChangesTracking;

/// <summary>
///     An mutable counterpart of a <see cref="IRelation{TParent, TChild}"/> interface
/// </summary>
/// <remarks>
///     When a <see cref="Commands.ICommand"/> executes it receives a mutable model.
///     When a <see cref="Commands.ICommand"/> finishes, model notifies all it's subscribers
///     that model has been changed and passes a new and old model to the subscriber. When subscriber
///     receives models it can only read them, because all modifications must happen inside the commands
///     to keep track of changes. Only commands can provide an access to a mutable model shard.
///     A mutable model shard has mutable collections and relations so they can be modified
///     and all modifications will be recorded and available when command finishes.
/// </remarks>
/// <typeparam name="TParent">A type of a parent entity</typeparam>
/// <typeparam name="TChild">A type of a child entity</typeparam>
public interface IMutableRelation<TParent, TChild> : IRelation<TParent, TChild>
    where TParent : Entity
    where TChild : Entity
{
    /// <summary>
    ///     Adds a new relation between parent and child entities
    /// </summary>
    /// <param name="parent">A parent entity</param>
    /// <param name="child">A child entity</param>
    void Add(TParent parent, TChild child);

    /// <summary>
    ///     Removes a relation between parent and child entities
    /// </summary>
    /// <param name="parent">A parent entity</param>
    /// <param name="child">A child entity</param>
    void Remove(TParent parent, TChild child);
}
