using CoreCraft.ChangesTracking;
using CoreCraft.Exceptions;

namespace CoreCraft.Persistence;

/// <inheritdoc cref="IModelShardStorage"/>
public abstract class ModelShardStorage : IModelShardStorage
{
    /// <inheritdoc cref="IModelShardStorage.Update(IRepository, IModelChanges)"/>
    public abstract void Update(IRepository repository, IModelChanges changes);

    /// <inheritdoc cref="IModelShardStorage.Save(IRepository, IModel)"/>
    public abstract void Save(IRepository repository, IModel model);

    /// <inheritdoc cref="IModelShardStorage.Load(IRepository, IModel)"/>
    public abstract void Load(IRepository repository, IModel model);

    /// <summary>
    ///     Call this method for a specific collection of a model shard to update data
    /// </summary>
    /// <typeparam name="TEntity">A type of an entity</typeparam>
    /// <typeparam name="TProperties">A type of properties</typeparam>
    /// <param name="repository">A repository</param>
    /// <param name="changes">Changes to apply</param>
    /// <param name="scheme">Scheme of properties</param>
    protected void Update<TEntity, TProperties>(IRepository repository, CollectionInfo scheme, ICollectionChangeSet<TEntity, TProperties> changes)
        where TEntity : Entity
        where TProperties : Properties
    {
        if (!changes.HasChanges())
        {
            return;
        }

        var added = new List<KeyValuePair<TEntity, TProperties>>();
        var modified = new List<ICollectionChange<TEntity, TProperties>>();
        var removed = new List<TEntity>();

        foreach (var change in changes)
        {
            switch (change.Action)
            {
                case CollectionAction.Add:
                    added.Add(new KeyValuePair<TEntity, TProperties>(change.Entity, change.NewData!));
                    break;

                case CollectionAction.Modify:
                    modified.Add(change);
                    break;

                case CollectionAction.Remove:
                    removed.Add(change.Entity);
                    break;
            }
        }

        if (added.Count > 0)
        {
            repository.Insert(scheme, added);
        }
        if (modified.Count > 0)
        {
            repository.Update(scheme, modified);
        }
        if (removed.Count > 0)
        {
            repository.Delete(scheme, removed);
        }
    }

    /// <summary>
    ///     Call this method for a specific collection of a model shard to store all it's data
    /// </summary>
    /// <typeparam name="TEntity">A type of an entity</typeparam>
    /// <typeparam name="TProperties">A type of properties</typeparam>
    /// <param name="repository">A repository</param>
    /// <param name="scheme">A collection scheme</param>
    /// <param name="collection">A collection to store</param>
    protected void Save<TEntity, TProperties>(IRepository repository, CollectionInfo scheme, ICollection<TEntity, TProperties> collection)
        where TEntity : Entity
        where TProperties : Properties
    {
        var items = collection.Pairs().Select(x => new KeyValuePair<TEntity, TProperties>(x.entity, x.properties)).ToArray();

        repository.Insert(scheme, items);
    }

    /// <summary>
    ///     Call this method for a specific relation of a model shard to update data
    /// </summary>
    /// <typeparam name="TParent">A type of a parent entity</typeparam>
    /// <typeparam name="TChild">A type of a child entity</typeparam>
    /// <param name="repository">A repository</param>
    /// <param name="scheme">A relation scheme</param>
    /// <param name="changes">Changes to apply</param>
    protected void Update<TParent, TChild>(IRepository repository, RelationInfo scheme, IRelationChangeSet<TParent, TChild> changes)
        where TParent : Entity
        where TChild : Entity
    {
        if (!changes.HasChanges())
        {
            return;
        }

        var linked = new List<KeyValuePair<TParent, TChild>>();
        var unlinked = new List<KeyValuePair<TParent, TChild>>();

        foreach (var change in changes)
        {
            switch (change.Action)
            {
                case RelationAction.Linked:
                    linked.Add(new KeyValuePair<TParent, TChild>(change.Parent, change.Child));
                    break;
                case RelationAction.Unlinked:
                    unlinked.Add(new KeyValuePair<TParent, TChild>(change.Parent, change.Child));
                    break;
            }
        }

        if (linked.Count > 0)
        {
            repository.Insert(scheme, linked);
        }
        if (unlinked.Count > 0)
        {
            repository.Delete(scheme, unlinked);
        }
    }

    /// <summary>
    ///     Call this method for a specific relation of a model shard to store all it's data
    /// </summary>
    /// <typeparam name="TParent">A type of a parent entity</typeparam>
    /// <typeparam name="TChild">A type of a child entity</typeparam>
    /// <param name="repository">A repository</param>
    /// <param name="scheme">A relation scheme</param>
    /// <param name="relation">A relation to store</param>
    protected void Save<TParent, TChild>(IRepository repository, RelationInfo scheme, IRelation<TParent, TChild> relation)
        where TParent : Entity
        where TChild : Entity
    {
        var pairs = relation
            .SelectMany(x => relation.Children(x).Select(y => new KeyValuePair<TParent, TChild>(x, y)));

        repository.Insert(scheme, pairs.ToArray());
    }

    /// <summary>
    ///     Call this method for a specific collection of a model shard to load all it's data
    /// </summary>
    /// <typeparam name="TEntity">A type of an entity</typeparam>
    /// <typeparam name="TProperties">A type of properties</typeparam>
    /// <param name="repository">A repository</param>
    /// <param name="scheme">Scheme of properties</param>
    /// <param name="collection">A collection to store</param>
    protected void Load<TEntity, TProperties>(
        IRepository repository,
        CollectionInfo scheme,
        IMutableCollection<TEntity, TProperties> collection)
        where TEntity : Entity
        where TProperties : Properties
    {
        if (collection.Count != 0)
        {
            throw new NonEmptyModelException($"The [{scheme.ShardName}.{scheme.Name}] is not empty. Clear or recreate the model before loading data");
        }

        repository.Select(scheme, collection);
    }

    /// <summary>
    ///     Call this method for a specific relation of a model shard to load all it's data
    /// </summary>
    /// <typeparam name="TParent">A type of a parent entity</typeparam>
    /// <typeparam name="TChild">A type of a child entity</typeparam>
    /// <param name="repository">A repository</param>
    /// <param name="scheme">A relation scheme</param>
    /// <param name="relation">A relation to store</param>
    /// <param name="parents">A parent entities collection</param>
    /// <param name="children">A child entities collection</param>
    protected void Load<TParent, TChild>(
        IRepository repository,
        RelationInfo scheme,
        IMutableRelation<TParent, TChild> relation,
        IEnumerable<TParent> parents,
        IEnumerable<TChild> children)
        where TParent : Entity
        where TChild : Entity
    {
        if (relation.Any())
        {
            throw new NonEmptyModelException($"The [{scheme.ShardName}.{scheme.Name}] is not empty. Clear or recreate the model before loading data");
        }

        repository.Select(scheme, relation, parents, children);
    }
}
