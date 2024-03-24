using CoreCraft.ChangesTracking;
using CoreCraft.Core;

namespace CoreCraft.Storage.Json.Model.History;

internal sealed class CollectionChange<TEntity, TProperties>
    where TEntity : Entity
    where TProperties : Properties
{
    public CollectionChange()
    {
    }

    public CollectionChange(ICollectionChange<TEntity, TProperties> change)
    {
        Action = change.Action;
        Entity = change.Entity;
        OldProperties = change.OldData;
        NewProperties = change.NewData;
    }

    public CollectionAction Action { get; set; }

    public TEntity Entity { get; set; }

    public TProperties? OldProperties { get; set; }

    public TProperties? NewProperties { get; set; }

    public static CollectionChange<TEntity, TProperties> Create(TEntity entity, TProperties props)
    {
        return new CollectionChange<TEntity, TProperties>()
        {
            Action = CollectionAction.Add,
            Entity = entity,
            NewProperties = props
        };
    }
}
