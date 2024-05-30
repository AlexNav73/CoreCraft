using CoreCraft.Core;

namespace CoreCraft.Storage.Json.Model.History;

internal sealed class ChangesFrame
{
    public ChangesFrame()
    {
        CollectionChanges = new List<ICollectionChangeSet>();
        RelationChanges = new List<IRelationChangeSet>();
    }

    public ChangesFrame(string name) : this()
    {
        Name = name;
    }

    public string Name { get; set; } = string.Empty;

    public IList<ICollectionChangeSet> CollectionChanges { get; set; }

    public IList<IRelationChangeSet> RelationChanges { get; set; }

    public CollectionChangeSet<TEntity, TProperties> CreateCollectionChangeSet<TEntity, TProperties>(CollectionInfo info)
        where TEntity : Entity
        where TProperties : Properties
    {
        var collectionChangeSet = new CollectionChangeSet<TEntity, TProperties>(info.Name);
        CollectionChanges.Add(collectionChangeSet);
        return collectionChangeSet;
    }

    public RelationChangeSet<TParent, TChild> CreateRelationChangeSet<TParent, TChild>(RelationInfo info)
        where TParent : Entity
        where TChild : Entity
    {
        var relationChangeSet = new RelationChangeSet<TParent, TChild>(info.Name);
        RelationChanges.Add(relationChangeSet);
        return relationChangeSet;
    }
}
