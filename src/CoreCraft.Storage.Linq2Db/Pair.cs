using CoreCraft.Core;

namespace CoreCraft.Storage.Linq2Db;

public record Pair<TParent, TChild>(TParent Parent, TChild Child)
    where TParent : Entity
    where TChild : Entity;
