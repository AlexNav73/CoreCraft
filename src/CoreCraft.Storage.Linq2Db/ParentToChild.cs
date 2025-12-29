using CoreCraft.Core;

namespace CoreCraft.Storage.Linq2Db;

public interface IRelationType<out TEntity>
    where TEntity : Entity
{
    TEntity Id { get; }
}

public record One<TEntity>(TEntity Id) : IRelationType<TEntity>
    where TEntity : Entity
{
    public static implicit operator TEntity(One<TEntity> one) => one.Id;
    public static implicit operator One<TEntity>(TEntity entity) => new(entity);
}

public record Many<TEntity>(TEntity Id) : IRelationType<TEntity>
    where TEntity : Entity
{
    public static implicit operator TEntity(Many<TEntity> one) => one.Id;
    public static implicit operator Many<TEntity>(TEntity entity) => new(entity);
}

public sealed record ParentToChild<TParent, TChild>(TParent Parent, TChild Child)
    where TParent : IRelationType<Entity>
    where TChild : IRelationType<Entity>;
