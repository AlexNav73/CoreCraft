using System.Diagnostics.CodeAnalysis;
using CoreCraft.ChangesTracking;
using CoreCraft.Persistence.History;

namespace CoreCraft.Persistence.Operations;

/// <summary>
///     TODO: write documentation
/// </summary>
/// <param name="timestamp"></param>
/// <param name="repository"></param>
[ExcludeFromCodeCoverage]
public readonly struct LoadChangesFrameOperation(long timestamp, IHistoryRepository repository) : IChangesFrameOperation
{
    private readonly long _timestamp = timestamp;
    private readonly IHistoryRepository _repository = repository;

    /// <inheritdoc />
    public void OnCollection<TEntity, TProperties>(ICollectionChangeSet<TEntity, TProperties> collection)
        where TEntity : Entity
        where TProperties : Properties
    {
        _repository.Load(_timestamp, collection);
    }

    /// <inheritdoc />
    public void OnRelation<TParent, TChild>(IRelationChangeSet<TParent, TChild> relation)
        where TParent : Entity
        where TChild : Entity
    {
        _repository.Load(_timestamp, relation);
    }
}
