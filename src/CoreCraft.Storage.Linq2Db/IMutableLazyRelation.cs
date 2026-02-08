using CoreCraft.ChangesTracking;
using CoreCraft.Core;

namespace CoreCraft.Storage.Linq2Db;

public interface IMutableLazyRelation<TParent, TChild> : ILazyRelation<TParent, TChild>
    where TParent : Entity
    where TChild : Entity
{
    void Add(TParent parent, TChild child);

    Task AddAsync(TParent parent, TChild child, CancellationToken token = default);
    
    void Remove(TParent parent);

    Task RemoveAsync(TParent parent, CancellationToken token = default);

    void Remove(TParent parent, TChild child);

    Task RemoveAsync(TParent parent, TChild child, CancellationToken token = default);

    Task ApplyAsync(IRelationChangeSet<TParent, TChild> changeSet, CancellationToken token = default);
}
