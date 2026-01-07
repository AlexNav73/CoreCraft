using CoreCraft.ChangesTracking;
using CoreCraft.Core;

namespace CoreCraft.Storage.Linq2Db;

public interface IMutableLazyRelation<TParent, TChild> : ILazyRelation<TParent, TChild>
    where TParent : Entity
    where TChild : Entity
{
    Task AddAsync(TParent parent, TChild child, CancellationToken token = default);
    
    Task RemoveAsync(TParent parent, CancellationToken token = default);

    Task RemoveAsync(TParent parent, TChild child, CancellationToken token = default);

    Task ApplyAsync(IRelationChangeSet<TParent, TChild> changeSet, CancellationToken token = default);
}
