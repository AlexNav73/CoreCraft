using CoreCraft.Core;
using LinqToDB;

namespace CoreCraft.Storage.Linq2Db.Extensions;

public static class RelationExtensions
{
    extension<TParent, TChild>(ILazyRelation<TParent, TChild> source)
        where TParent : Entity
        where TChild : Entity
    {
        /// <summary>
        ///     Tests if a relation contains an entity
        /// </summary>
        /// <param name="entity">An entity to test</param>
        /// <returns>True - if relation contains parent entity</returns>
        public Task<bool> ContainsParentAsync(TParent entity, CancellationToken token = default)
            => source.Where(x => x.Parent == entity).AnyAsync(token);

        /// <summary>
        ///     Tests if a relation contains an entity
        /// </summary>
        /// <param name="entity">An entity to test</param>
        /// <returns>True - if relation contains child entity</returns>
        public Task<bool> ContainsChildAsync(TChild entity, CancellationToken token = default)
            => source.Where(x => x.Child == entity).AnyAsync(token);

        /// <summary>
        ///     Tests if the parent contains the child entity.
        /// </summary>
        /// <param name="parent">A parent entity.</param>
        /// <param name="child">A child entity.</param>
        /// <returns>True if the parent is linked to the child.</returns>
        public Task<bool> ContainsAsync(TParent parent, TChild child, CancellationToken token = default)
            => source.ContainsAsync(new Pair<TParent, TChild>(parent, child), token);

        /// <summary>
        ///     Retrieves all children entities for a given parent entity
        /// </summary>
        /// <param name="parent">A parent entity</param>
        /// <returns>A collection of child entities</returns>
        public IQueryable<TChild> Children(TParent parent)
            => source.Where(x => x.Parent == parent).Select(x => x.Child);

        /// <summary>
        ///     Retrieves all parent entities for a given child entity
        /// </summary>
        /// <param name="child">A child entity</param>
        /// <returns>A collection of parent entities</returns>
        public IQueryable<TParent> Parents(TChild child)
            => source.Where(x => x.Child == child).Select(x => x.Parent);
    }
}
