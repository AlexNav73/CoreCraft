using CoreCraft.Core;

namespace CoreCraft.Storage.Linq2Db;

public interface IRelationType
{
}

public sealed record One : IRelationType
{
}

public sealed record Many : IRelationType
{
}

public sealed record ParentToChild<TParentRelation, TParent, TChildRelation, TChild>(TParent Parent, TChild Child) : Pair<TParent, TChild>(Parent, Child)
    where TParent : Entity
    where TChild : Entity
    where TParentRelation : IRelationType
    where TChildRelation : IRelationType;
