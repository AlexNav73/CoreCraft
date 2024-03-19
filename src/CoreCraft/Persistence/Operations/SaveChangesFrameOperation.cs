using System.Diagnostics.CodeAnalysis;
using CoreCraft.ChangesTracking;
using CoreCraft.Persistence.History;

namespace CoreCraft.Persistence.Operations;

[ExcludeFromCodeCoverage]
internal readonly struct SaveChangesFrameOperation(long timestamp, IHistoryRepository repository) : IChangesFrameOperation
{
    private readonly long _timestamp = timestamp;
    private readonly IHistoryRepository _repository = repository;

    public void OnCollection<TEntity, TProperties>(ICollectionChangeSet<TEntity, TProperties> collection)
        where TEntity : Entity
        where TProperties : Properties
    {
        _repository.Save(_timestamp, collection);
    }

    public void OnRelation<TParent, TChild>(IRelationChangeSet<TParent, TChild> relation)
        where TParent : Entity
        where TChild : Entity
    {
        _repository.Save(_timestamp, relation);
    }
}
