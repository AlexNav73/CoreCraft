using CoreCraft.Core;

namespace CoreCraft.Storage.Linq2Db;

public interface IEntityCache<TEntity, TProperties>
    where TEntity : Entity
    where TProperties : Properties, IHaveEntityId<TEntity>
{
    void Cache(TProperties properties);
}
