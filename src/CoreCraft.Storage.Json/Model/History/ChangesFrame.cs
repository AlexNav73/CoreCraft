namespace CoreCraft.Storage.Json.Model.History;

internal sealed class ChangesFrame
{
    public ChangesFrame()
    {
        CollectionChanges = new List<ICollectionChangeSet>();
        RelationChanges = new List<IRelationChangeSet>();
    }

    public string Name { get; set; }

    public IList<ICollectionChangeSet> CollectionChanges { get; set; }

    public IList<IRelationChangeSet> RelationChanges { get; set; }
}
