using CoreCraft.ChangesTracking;
using CoreCraft.Core;
using CoreCraft.Persistence.Operations;
using CoreCraft.Storage.Json.Model;
using CoreCraft.Storage.Json.Model.History;

namespace CoreCraft.Storage.Json;

internal sealed class JsonRepository : IJsonRepository
{
    private readonly Model.Model _model;

    public JsonRepository(Model.Model model)
    {
        _model = model;
    }

    public void Update<TEntity, TProperties>(ICollectionChangeSet<TEntity, TProperties> changes)
        where TEntity : Entity
        where TProperties : Properties
    {
        if (!changes.HasChanges())
        {
            return;
        }

        var shard = GetOrCreateModelShard(changes.Info.ShardName);
        var collection = GetOrCreateCollection<TProperties>(changes.Info, shard);

        foreach (var change in changes)
        {
            switch (change.Action)
            {
                case CollectionAction.Add:
                    collection.Items.Add(new Item<TProperties>(change.Entity.Id, change.NewData!));
                    break;

                case CollectionAction.Modify:
                    var itemToAdd = collection.Items.SingleOrDefault(x => x.Id == change.Entity.Id);
                    if (itemToAdd is null)
                    {
                        throw new InvalidOperationException($"Unable to update item with id [{change.Entity.Id}]");
                    }

                    itemToAdd.Properties = change.NewData!;
                    break;

                case CollectionAction.Remove:
                    var itemToDelete = collection.Items.SingleOrDefault(x => x.Id == change.Entity.Id);
                    if (itemToDelete is null)
                    {
                        throw new InvalidOperationException($"Unable to delete item with id [{change.Entity.Id}]");
                    }

                    collection.Items.Remove(itemToDelete);
                    break;
            }
        }

        if (collection.Items.Count == 0)
        {
            shard.Collections.Remove(collection);
        }
    }

    public void Update<TParent, TChild>(IRelationChangeSet<TParent, TChild> changes)
        where TParent : Entity
        where TChild : Entity
    {
        if (!changes.HasChanges())
        {
            return;
        }

        var shard = GetOrCreateModelShard(changes.Info.ShardName);
        var relation = GetOrCreateRelation(changes.Info, shard);

        foreach (var change in changes)
        {
            switch (change.Action)
            {
                case RelationAction.Linked:
                    relation.Pairs.Add(new Pair(change.Parent.Id, change.Child.Id));
                    break;

                case RelationAction.Unlinked:
                    var item = relation.Pairs.SingleOrDefault(x => x.Parent == change.Parent.Id && x.Child == change.Child.Id);
                    if (item is null)
                    {
                        throw new InvalidOperationException($"Unable to remove pair [{change.Parent.Id}, {change.Child.Id}]");
                    }

                    relation.Pairs.Remove(item);
                    break;
            }
        }

        if (relation.Pairs.Count == 0)
        {
            shard.Relations.Remove(relation);
        }
    }

    public void Save<TEntity, TProperties>(ICollection<TEntity, TProperties> collection)
        where TEntity : Entity
        where TProperties : Properties
    {
        if (collection.Count == 0)
        {
            return;
        }

        var shard = GetOrCreateModelShard(collection.Info.ShardName);
        var jsonCollection = GetOrCreateCollection<TProperties>(collection.Info, shard);

        foreach (var (entity, properties) in collection.Pairs())
        {
            jsonCollection.Items.Add(new Item<TProperties>(entity.Id, properties));
        }
    }

    public void Save<TParent, TChild>(IRelation<TParent, TChild> relation)
        where TParent : Entity
        where TChild : Entity
    {
        if (!relation.Any())
        {
            return;
        }

        var shard = GetOrCreateModelShard(relation.Info.ShardName);
        var jsonRelation = GetOrCreateRelation(relation.Info, shard);

        foreach (var parent in relation)
        {
            foreach (var child in relation.Children(parent))
            {
                jsonRelation.Pairs.Add(new Pair(parent.Id, child.Id));
            }
        }
    }

    public void Save<TEntity, TProperties>(long changeId, ICollectionChangeSet<TEntity, TProperties> changes)
        where TEntity : Entity
        where TProperties : Properties
    {
        if (!changes.Any())
        {
            return;
        }

        var modelChange = GetOrCreateModelChanges(changeId);
        var frame = GetOrCreateChangesFrame(modelChange, changes.Info.ShardName);
        var collectionChanges = GetOrCreateCollectionChanges<TEntity, TProperties>(frame, changes.Info.Name);

        foreach (var change in changes)
        {
            collectionChanges.Changes.Add(new CollectionChange<TEntity, TProperties>(change));
        }
    }

    public void Save<TParent, TChild>(long changeId, IRelationChangeSet<TParent, TChild> changes)
        where TParent : Entity
        where TChild : Entity
    {
        if (!changes.Any())
        {
            return;
        }

        var modelChange = GetOrCreateModelChanges(changeId);
        var frame = GetOrCreateChangesFrame(modelChange, changes.Info.ShardName);
        var relationChanges = GetOrCreateRelationChanges<TParent, TChild>(frame, changes.Info.Name);

        foreach (var change in changes)
        {
            relationChanges.Changes.Add(new RelationChange<TParent, TChild>(change));
        }
    }

    public void Load<TEntity, TProperties>(IMutableCollection<TEntity, TProperties> collection)
        where TEntity : Entity
        where TProperties : Properties
    {
        var shard = _model.Shards.SingleOrDefault(x => x.Name == collection.Info.ShardName);
        if (shard is null)
        {
            return;
        }

        var col = shard.Collections
            .OfType<Collection<TProperties>>()
            .SingleOrDefault(x => x.Name == collection.Info.Name);
        if (col is null)
        {
            return;
        }

        foreach (var item in col.Items)
        {
            collection.Add(item.Id, p => item.Properties);
        }
    }

    public void Load<TParent, TChild>(IMutableRelation<TParent, TChild> relation, IEnumerable<TParent> parents, IEnumerable<TChild> children)
        where TParent : Entity
        where TChild : Entity
    {
        var shard = _model.Shards.SingleOrDefault(x => x.Name == relation.Info.ShardName);
        if (shard is null)
        {
            return;
        }

        var rel = shard.Relations.SingleOrDefault(x => x.Name == relation.Info.Name);
        if (rel is null)
        {
            return;
        }

        foreach (var pair in rel.Pairs)
        {
            var parent = parents.SingleOrDefault(x => x.Id == pair.Parent);
            var child = children.SingleOrDefault(x => x.Id == pair.Child);

            if (parent is not null && child is not null)
            {
                relation.Add(parent, child);
            }
        }
    }

    public void Load<TEntity, TProperties>(long changeId, ICollectionChangeSet<TEntity, TProperties> changes)
        where TEntity : Entity
        where TProperties : Properties
    {
        var modelChange = _model.ChangesHistory.SingleOrDefault(x => x.Timestamp == changeId);
        if (modelChange is null)
        {
            return;
        }

        var frame = modelChange.Frames.SingleOrDefault(x => x.Name == changes.Info.ShardName);
        if (frame is null)
        {
            return;
        }

        var collectionChanges = frame.CollectionChanges
            .OfType<Model.History.CollectionChangeSet<TEntity, TProperties>>()
            .SingleOrDefault(x => x.Name == changes.Info.Name);
        if (collectionChanges is null)
        {
            return;
        }

        foreach (var change in collectionChanges.Changes)
        {
            changes.Add(change.Action, change.Entity, change.OldProperties, change.NewProperties);
        }
    }

    public void Load<TParent, TChild>(long changeId, IRelationChangeSet<TParent, TChild> changes)
        where TParent : Entity
        where TChild : Entity
    {
        var modelChange = _model.ChangesHistory.SingleOrDefault(x => x.Timestamp == changeId);
        if (modelChange is null)
        {
            return;
        }

        var frame = modelChange.Frames.SingleOrDefault(x => x.Name == changes.Info.ShardName);
        if (frame is null)
        {
            return;
        }

        var relationChanges = frame.RelationChanges
            .OfType<Model.History.RelationChangeSet<TParent, TChild>>()
            .SingleOrDefault(x => x.Name == changes.Info.Name);
        if (relationChanges is null)
        {
            return;
        }

        foreach (var change in relationChanges.Changes)
        {
            changes.Add(change.Action, change.Parent, change.Child);
        }
    }

    public IEnumerable<IModelChanges> RestoreHistory(IEnumerable<IModelShard> shards)
    {
        var changes = new List<IModelChanges>();

        foreach (var timestamp in _model.ChangesHistory.Select(x => x.Timestamp))
        {
            var modelChanges = new ChangesTracking.ModelChanges(timestamp);

            foreach (var shard in shards.Cast<IFrameFactory>())
            {
                var frame = (IChangesFrameEx)shard.Create();
                frame.Do(new LoadChangesFrameOperation(timestamp, this));
                if (frame.HasChanges())
                {
                    modelChanges.AddOrGet(frame);
                }
            }

            if (modelChanges.Any())
            {
                changes.Add(modelChanges);
            }
        }

        return changes;
    }

    private ModelShard GetOrCreateModelShard(string name)
    {
        var shard = _model.Shards.SingleOrDefault(x => x.Name == name);
        if (shard is null)
        {
            shard = new ModelShard(name);
            _model.Shards.Add(shard);
        }

        return shard;
    }

    private static Collection<TProperties> GetOrCreateCollection<TProperties>(CollectionInfo scheme, ModelShard shard)
        where TProperties : Properties
    {
        var collection = shard.Collections
            .OfType<Collection<TProperties>>()
            .SingleOrDefault(x => x.Name == scheme.Name);

        if (collection is null)
        {
            collection = new Collection<TProperties>(scheme.Name);
            shard.Collections.Add(collection);
        }

        return collection;
    }

    private static Relation GetOrCreateRelation(RelationInfo scheme, ModelShard shard)
    {
        var relation = shard.Relations.SingleOrDefault(x => x.Name == scheme.Name);
        if (relation is null)
        {
            relation = new Relation(scheme.Name);
            shard.Relations.Add(relation);
        }

        return relation;
    }

    private Model.History.ModelChanges GetOrCreateModelChanges(long changeId)
    {
        return GetOrCreate(
            _model.ChangesHistory,
            x => x.Timestamp == changeId,
            () => new Model.History.ModelChanges() { Timestamp = changeId });
    }

    private static ChangesFrame GetOrCreateChangesFrame(Model.History.ModelChanges change, string name)
    {
        return GetOrCreate(
            change.Frames,
            x => x.Name == name,
            () => new ChangesFrame() { Name = name });
    }

    private static Model.History.CollectionChangeSet<TEntity, TProperties> GetOrCreateCollectionChanges<TEntity, TProperties>(
        ChangesFrame frame,
        string name)
        where TEntity : Entity
        where TProperties : Properties
    {
        return GetOrCreate(
            frame.CollectionChanges,
            x => x.Name == name,
            () => new Model.History.CollectionChangeSet<TEntity, TProperties>() { Name = name });
    }

    private static Model.History.RelationChangeSet<TParent, TChild> GetOrCreateRelationChanges<TParent, TChild>(
        ChangesFrame frame,
        string name)
        where TParent : Entity
        where TChild : Entity
    {
        return GetOrCreate(
            frame.RelationChanges,
            x => x.Name == name,
            () => new Model.History.RelationChangeSet<TParent, TChild>() { Name = name });
    }

    private static T GetOrCreate<T, E>(IList<E> collection, Func<E, bool> predicate, Func<T> create)
        where T : E
    {
        var change = collection.SingleOrDefault(predicate);
        if (change is null)
        {
            change = create();
            collection.Add(change);
        }

        return (T)change!;
    }
}
