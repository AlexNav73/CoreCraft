using System.Diagnostics.CodeAnalysis;
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

    [SetsRequiredMembers]
    public RelationChange(IRelationChange<TParent, TChild> change)
    {
        Action = change.Action;
        Parent = change.Parent;
        Child = change.Child;
    }

    public RelationAction Action { get; set; }

    public required TParent Parent { get; set; }

    public required TChild Child { get; set; }

    public static RelationChange<TParent, TChild> Create(TParent parent, TChild child)
    {
        return new RelationChange<TParent, TChild>()
        {
            Action = RelationAction.Linked,
            Parent = parent,
            Child = child
        };
    }
}
