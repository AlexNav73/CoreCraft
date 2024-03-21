using CoreCraft.Core;

namespace CoreCraft.Storage.Json.Model.History;

internal sealed class CollectionChangeSet<TEntity, TProperties> : ICollectionChangeSet
    where TEntity : Entity
    where TProperties : Properties
{
    public CollectionChangeSet()
    {
        Changes = new List<CollectionChange<TEntity, TProperties>>();
    }

    public string Name { get; set; }

    public IList<CollectionChange<TEntity, TProperties>> Changes { get; set; }
}
