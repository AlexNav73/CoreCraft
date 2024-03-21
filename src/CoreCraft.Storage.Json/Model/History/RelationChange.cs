using CoreCraft.ChangesTracking;
using CoreCraft.Core;

namespace CoreCraft.Storage.Json.Model.History;

internal sealed class RelationChange<TParent, TChild>
    where TParent : Entity
    where TChild : Entity
{
    public RelationChange()
    {
    }

    public RelationChange(IRelationChange<TParent, TChild> change)
    {
        Action = change.Action;
        Parent = change.Parent;
        Child = change.Child;
    }

    public RelationAction Action { get; set; }

    public TParent Parent { get; set; }

    public TChild Child { get; set; }
}
