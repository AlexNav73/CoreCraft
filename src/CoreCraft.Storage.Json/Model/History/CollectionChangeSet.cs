using CoreCraft.Core;

namespace CoreCraft.Storage.Json.Model.History;

internal sealed class CollectionChangeSet<TEntity, TProperties> : ICollectionChangeSet
    where TEntity : Entity
    where TProperties : Properties
{
    public CollectionChangeSet()
    {
        Changes = new List<CollectionChange<TEntity, TProperties>>();
        Name = string.Empty;
    }

    public CollectionChangeSet(string name) : this()
    {
        Name = name;
    }

    public string Name { get; set; }

    public IList<CollectionChange<TEntity, TProperties>> Changes { get; set; }

    public CollectionChange<TEntity, TProperties> Create(TEntity entity, TProperties properties)
    {
        var change = CollectionChange<TEntity, TProperties>.Create(entity, properties);
        Changes.Add(change);
        return change;
    }
}
