using CoreCraft.Core;
using CoreCraft.ChangesTracking;

namespace CoreCraft.Storage.Linq2Db;

/// <summary>
/// 
/// </summary>
/// <typeparam name="TParent"></typeparam>
/// <typeparam name="TChild"></typeparam>
public interface ILazyRelation<TParent, TChild> : IHaveInfo<RelationInfo>
    where TParent : Entity
    where TChild : Entity
{

}

public interface IMutableLazyRelation<TParent, TChild> : ILazyRelation<TParent, TChild>
    where TParent : Entity
    where TChild : Entity
{
    Task ApplyAsync(IRelationChangeSet<TParent, TChild> changeSet, CancellationToken token = default);
}

public sealed class LazyRelation<TParent, TChild> :
    IMutableLazyRelation<TParent, TChild>,
    IMutableState<ILazyRelation<TParent, TChild>>
    where TParent : Entity
    where TChild : Entity
{

    private readonly IMapping<TParent, TChild> _parentToChildRelations;
    private readonly IMapping<TChild, TParent> _childToParentRelations;

    /// <summary>
    ///     Ctor
    /// </summary>
    public LazyRelation(
        RelationInfo info,
        IMapping<TParent, TChild> parentToChildRelation,
        IMapping<TChild, TParent> childToParentRelation)
    {
        _parentToChildRelations = parentToChildRelation;
        _childToParentRelations = childToParentRelation;

        Info = info;
    }

    /// <inheritdoc cref="IHaveInfo{T}.Info"/>
    public RelationInfo Info { get; }

    public ILazyRelation<TParent, TChild> AsReadOnly()
    {
        return this;
    }

    /// <summary>
    /// Applies changes from a relation change set to this lazy relation.
    /// This will perform mapping operations (Add/Remove) on the underlying mapping abstraction.
    /// </summary>
    public Task ApplyAsync(IRelationChangeSet<TParent, TChild> changeSet, CancellationToken token = default)
    {
        foreach (var change in changeSet)
        {
            switch (change.Action)
            {
                case RelationAction.Linked:
                    _parentToChildRelations.Add(change.Parent, change.Child);
                    _childToParentRelations.Add(change.Child, change.Parent);
                    break;
                case RelationAction.Unlinked:
                    _parentToChildRelations.Remove(change.Parent, change.Child);
                    _childToParentRelations.Remove(change.Child, change.Parent);
                    break;
                default:
                    throw new NotSupportedException($"An action [{change.Action}] is not supported.");
            }
        }

        return Task.CompletedTask;
    }
}
