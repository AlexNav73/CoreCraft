namespace PricingCalc.Model.Engine.Persistence;

public interface IRepository
{
    void Insert<TEntity, TData>(string name, IReadOnlyCollection<KeyValuePair<TEntity, TData>> items, Scheme scheme)
        where TEntity : Entity
        where TData : Properties;

    void Insert<TParent, TChild>(string name, IReadOnlyCollection<KeyValuePair<TParent, TChild>> relations)
        where TParent : Entity
        where TChild : Entity;

    void Update<TEntity, TData>(string name, IReadOnlyCollection<KeyValuePair<TEntity, TData>> items, Scheme scheme)
        where TEntity : Entity
        where TData : Properties;

    void Delete<TEntity>(string name, IReadOnlyCollection<TEntity> entities)
        where TEntity : Entity;

    void Delete<TParent, TChild>(string name, IReadOnlyCollection<KeyValuePair<TParent, TChild>> relations)
        where TParent : Entity
        where TChild : Entity;

    void Select<TEntity, TData>(string name, IMutableCollection<TEntity, TData> collection, Scheme scheme)
        where TEntity : Entity
        where TData : Properties;

    void Select<TParent, TChild>(string name, IMutableRelation<TParent, TChild> relation, IEnumerable<TParent> parentCollection, IEnumerable<TChild> childCollection)
        where TParent : Entity
        where TChild : Entity;
}
