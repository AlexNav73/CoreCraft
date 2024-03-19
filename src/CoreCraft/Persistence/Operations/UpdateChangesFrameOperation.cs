using System.Diagnostics.CodeAnalysis;
using CoreCraft.ChangesTracking;

namespace CoreCraft.Persistence.Operations;

/// <summary>
///     TODO: write documentation
/// </summary>
/// <param name="repository"></param>
[ExcludeFromCodeCoverage]
public readonly struct UpdateChangesFrameOperation(IRepository repository) : IChangesFrameOperation
{
    private readonly IRepository _repository = repository;

    /// <inheritdoc />
    public void OnCollection<TEntity, TProperties>(ICollectionChangeSet<TEntity, TProperties> collection)
        where TEntity : Entity
        where TProperties : Properties
    {
        _repository.Update(collection);
    }

    /// <inheritdoc />
    public void OnRelation<TParent, TChild>(IRelationChangeSet<TParent, TChild> relation)
        where TParent : Entity
        where TChild : Entity
    {
        _repository.Update(relation);
    }
}
