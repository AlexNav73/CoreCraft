using CoreCraft.Core;

namespace CoreCraft.Storage.Linq2Db.Extensions;

public static class CollectionExtensions
{
    extension<TEntity, TProperties>(ILazyCollection<TEntity, TProperties> source)
        where TEntity: Entity
        where TProperties: Properties, IHaveEntityId<TEntity>
    {
        public IQueryable<TEntity> Entities => source.Select(p => p.EntityId);
    }
}
