using System.Linq.Expressions;
using CoreCraft.Core;
using LinqToDB;

namespace CoreCraft.Storage.Linq2Db.Extensions;

public static class CollectionExtensions
{
    extension<TEntity, TProperties>(ILazyCollection<TEntity, TProperties> source)
        where TEntity: Entity
        where TProperties: Properties, IHaveEntityId<TEntity>
    {
        public IQueryable<TEntity> Entities => source.Select(p => p.EntityId);

        public IQueryable<TResult> JoinWith<TChild, TResult>(
            ILazyRelation<TEntity, TChild> relation,
            Expression<Func<TProperties, Pair<TEntity, TChild>, TResult>> resultSelector)
            where TChild : Entity
            => source.Join(
                relation,
                x => x.EntityId,
                x => x.Parent,
                resultSelector);
    }

    extension<TEntity, TProperties, TModel>(IQueryable<TModel> source)
        where TEntity : Entity
        where TProperties : Properties, IHaveEntityId<TEntity>
    {
        public IQueryable<TResult> JoinWith<TResult>(
            ILazyCollection<TEntity, TProperties> collection,
            Expression<Func<TModel, TEntity>> property,
            Expression<Func<TModel, TProperties, TResult>> resultSelector)
            => source.Join(
                collection,
                property,
                x => x.EntityId,
                resultSelector);
    }
}
