using CoreCraft.Core;

namespace CoreCraft.Storage.Linq2Db;

/// <summary>
/// 
/// </summary>
/// <typeparam name="TParent"></typeparam>
/// <typeparam name="TChild"></typeparam>
public interface ILazyRelation<TParent, TChild> : IQueryable<Pair<TParent, TChild>>, IHaveInfo<RelationInfo>
    where TParent : Entity
    where TChild : Entity
{
}
