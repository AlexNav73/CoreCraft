namespace Navitski.Crystalized.Model.Engine.Core;

public interface IMutableCollection<TEntity, TData> : ICollection<TEntity, TData>
    where TEntity : Entity
    where TData : Properties
{
    TEntity Add(TData data);

    TEntity Add(Guid id, Func<TData, TData> init);

    void Add(TEntity entity, TData data);

    void Modify(TEntity entity, Func<TData, TData> modifier);

    void Remove(TEntity entity);
}
