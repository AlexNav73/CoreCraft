using System.Collections.Generic;
using PricingCalc.Model.Engine.Core;

namespace PricingCalc.Model.Engine.Persistence
{
    public interface IRepository
    {
        void UpdateVersionInfo(string modelShardName, string version);

        void Migrate(string modelShardName, Version version);

        void Insert<TEntity, TData>(string name, IReadOnlyCollection<KeyValuePair<TEntity, TData>> items, Scheme scheme)
            where TEntity : IEntity
            where TData : IEntityProperties;

        void Insert<TParent, TChild>(string name, IReadOnlyCollection<KeyValuePair<TParent, TChild>> relations)
            where TParent : IEntity
            where TChild : IEntity;

        void Update<TEntity, TData>(string name, IReadOnlyCollection<KeyValuePair<TEntity, TData>> items, Scheme scheme)
            where TEntity : IEntity
            where TData : IEntityProperties;

        void Delete<TEntity>(string name, IReadOnlyCollection<TEntity> entities)
            where TEntity : IEntity;

        void Delete<TParent, TChild>(string name, IReadOnlyCollection<KeyValuePair<TParent, TChild>> relations)
            where TParent : IEntity
            where TChild : IEntity;

        void Select<TEntity, TData>(string name, ICollection<TEntity, TData> collection, Scheme scheme)
            where TEntity : IEntity, ICopy<TEntity>
            where TData : IEntityProperties, ICopy<TData>;

        void Select<TParent, TChild>(string name, IRelation<TParent, TChild> relation, IEntityCollection<TParent> parentCollection, IEntityCollection<TChild> childCollection)
            where TParent : IEntity
            where TChild : IEntity;
    }
}
