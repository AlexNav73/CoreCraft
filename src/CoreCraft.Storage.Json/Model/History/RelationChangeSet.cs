using CoreCraft.Core;

namespace CoreCraft.Storage.Json.Model.History;

internal sealed class RelationChangeSet<TParent, TChild> : IRelationChangeSet
    where TParent : Entity
    where TChild : Entity
{
    public RelationChangeSet()
    {
        Changes = new List<RelationChange<TParent, TChild>>();
    }

    public string Name { get; set; }

    public IList<RelationChange<TParent, TChild>> Changes { get; set; }
}
