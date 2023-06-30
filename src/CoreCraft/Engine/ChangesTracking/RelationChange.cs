using System.Diagnostics;

namespace CoreCraft.Engine.ChangesTracking;

/// <inheritdoc cref="IRelationChange{TParent, TChild}"/>
[DebuggerDisplay("Action = {Action}")]
public sealed class RelationChange<TParent, TChild> : IRelationChange<TParent, TChild>
    where TParent : Entity
    where TChild : Entity
{
    /// <summary>
    ///     Ctor
    /// </summary>
    public RelationChange(RelationAction action, TParent parent, TChild child)
    {
        Action = action;
        Parent = parent;
        Child = child;
    }

    /// <inheritdoc />
    public RelationAction Action { get; }

    /// <inheritdoc />
    public TParent Parent { get; }

    /// <inheritdoc />
    public TChild Child { get; }

    /// <inheritdoc />
    public IRelationChange<TParent, TChild> Invert()
    {
        return new RelationChange<TParent, TChild>(InvertAction(Action), Parent, Child);
    }

    private static RelationAction InvertAction(RelationAction action)
    {
        return action switch
        {
            RelationAction.Linked => RelationAction.Unlinked,
            RelationAction.Unlinked => RelationAction.Linked,
            _ => throw new NotSupportedException($"Action {action} is not supported")
        };
    }
}
