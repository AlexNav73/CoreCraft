using System.Diagnostics;

namespace CoreCraft.ChangesTracking;

/// <inheritdoc cref="ICollectionChange{TEntity, TProperties}"/>
[DebuggerDisplay("Action = {Action}")]
internal sealed class CollectionChange<TEntity, TProperties> : ICollectionChange<TEntity, TProperties>
    where TEntity : Entity
    where TProperties : Properties
{
    public CollectionChange(CollectionAction action, TEntity entity, TProperties? oldData, TProperties? newData)
    {
        Action = action;
        Entity = entity;
        OldData = oldData;
        NewData = newData;
    }

    /// <inheritdoc />
    public CollectionAction Action { get; }

    /// <inheritdoc />
    public TEntity Entity { get; }

    /// <inheritdoc />
    public TProperties? OldData { get; }

    /// <inheritdoc />
    public TProperties? NewData { get; }

    /// <inheritdoc />
    public ICollectionChange<TEntity, TProperties> Invert()
    {
        return new CollectionChange<TEntity, TProperties>(InvertAction(Action), Entity, NewData, OldData);
    }

    private static CollectionAction InvertAction(CollectionAction action)
    {
        return action switch
        {
            CollectionAction.Add => CollectionAction.Remove,
            CollectionAction.Remove => CollectionAction.Add,
            CollectionAction.Modify => CollectionAction.Modify,
            _ => throw new NotSupportedException($"Action type {action} is not supported")
        };
    }
}
