using CoreCraft.ChangesTracking;
using CoreCraft.Core;
using CoreCraft.Storage.Json.Model;

namespace CoreCraft.Storage.Json;

internal sealed class JsonRepository : IJsonRepository
{
    private readonly IList<ModelShard> _modelShards;

    public JsonRepository(IList<ModelShard> modelShards)
    {
        _modelShards = modelShards;
    }

    public void Update<TEntity, TProperties>(CollectionInfo scheme, ICollectionChangeSet<TEntity, TProperties> changes)
        where TEntity : Entity
        where TProperties : Properties
    {
        if (!changes.HasChanges())
        {
            return;
        }

        var shard = GetOrCreateModelShard(scheme.ShardName);
        var collection = GetOrCreateCollection<TProperties>(scheme, shard);

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

    public void Update<TParent, TChild>(RelationInfo scheme, IRelationChangeSet<TParent, TChild> changes)
        where TParent : Entity
        where TChild : Entity
    {
        if (!changes.HasChanges())
        {
            return;
        }

        var shard = GetOrCreateModelShard(scheme.ShardName);
        var relation = GetOrCreateRelation(scheme, shard);

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

    public void Save<TEntity, TProperties>(CollectionInfo scheme, ICollection<TEntity, TProperties> collection)
        where TEntity : Entity
        where TProperties : Properties
    {
        if (collection.Count == 0)
        {
            return;
        }

        var shard = GetOrCreateModelShard(scheme.ShardName);
        var jsonCollection = GetOrCreateCollection<TProperties>(scheme, shard);

        foreach (var (entity, properties) in collection.Pairs())
        {
            jsonCollection.Items.Add(new Item<TProperties>(entity.Id, properties));
        }
    }

    public void Save<TParent, TChild>(RelationInfo scheme, IRelation<TParent, TChild> relation)
        where TParent : Entity
        where TChild : Entity
    {
        if (!relation.Any())
        {
            return;
        }

        var shard = GetOrCreateModelShard(scheme.ShardName);
        var jsonRelation = GetOrCreateRelation(scheme, shard);

        foreach (var parent in relation)
        {
            foreach (var child in relation.Children(parent))
            {
                jsonRelation.Pairs.Add(new Pair(parent.Id, child.Id));
            }
        }
    }

    public void Load<TEntity, TProperties>(CollectionInfo scheme, IMutableCollection<TEntity, TProperties> collection)
        where TEntity : Entity
        where TProperties : Properties
    {
        var shard = _modelShards.SingleOrDefault(x => x.Name == scheme.ShardName);
        if (shard is null)
        {
            return;
        }

        var col = shard.Collections
            .OfType<Collection<TProperties>>()
            .SingleOrDefault(x => x.Name == scheme.Name);
        if (col is null)
        {
            return;
        }

        foreach (var item in col.Items)
        {
            collection.Add(item.Id, p => item.Properties);
        }
    }

    public void Load<TParent, TChild>(RelationInfo scheme, IMutableRelation<TParent, TChild> relation, IEnumerable<TParent> parents, IEnumerable<TChild> children)
        where TParent : Entity
        where TChild : Entity
    {
        var shard = _modelShards.SingleOrDefault(x => x.Name == scheme.ShardName);
        if (shard is null)
        {
            return;
        }

        var rel = shard.Relations.SingleOrDefault(x => x.Name == scheme.Name);
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

    private ModelShard GetOrCreateModelShard(string name)
    {
        var shard = _modelShards.SingleOrDefault(x => x.Name == name);
        if (shard is null)
        {
            shard = new ModelShard(name);
            _modelShards.Add(shard);
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
}
