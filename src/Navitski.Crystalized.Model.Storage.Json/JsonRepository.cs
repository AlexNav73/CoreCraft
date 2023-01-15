using Navitski.Crystalized.Model.Engine.ChangesTracking;
using Navitski.Crystalized.Model.Engine.Core;
using Navitski.Crystalized.Model.Storage.Json.Model;

namespace Navitski.Crystalized.Model.Storage.Json;

internal sealed class JsonRepository : IJsonRepository
{
    private readonly IList<ModelShard> _modelShards;

    public JsonRepository(IList<ModelShard> modelShards)
    {
        _modelShards = modelShards;
    }

    public void Insert<TEntity, TProperties>(CollectionInfo scheme, IReadOnlyCollection<KeyValuePair<TEntity, TProperties>> items)
        where TEntity : Entity
        where TProperties : Properties
    {
        var shard = GetOrCreateModelShard(scheme.ShardName);
        var collection = GetOrCreateCollection<TProperties>(scheme, shard);

        foreach (var item in items)
        {
            collection.Items.Add(new Item<TProperties>(item.Key.Id, item.Value));
        }
    }

    public void Insert<TParent, TChild>(RelationInfo scheme, IReadOnlyCollection<KeyValuePair<TParent, TChild>> relations)
        where TParent : Entity
        where TChild : Entity
    {
        var shard = GetOrCreateModelShard(scheme.ShardName);
        var relation = GetOrCreateRelation(shard, scheme);

        foreach (var pair in relations)
        {
            relation.Pairs.Add(new Pair(pair.Key.Id, pair.Value.Id));
        }
    }

    public void Update<TEntity, TProperties>(CollectionInfo scheme, IReadOnlyCollection<ICollectionChange<TEntity, TProperties>> changes)
        where TEntity : Entity
        where TProperties : Properties
    {
        var shard = _modelShards.SingleOrDefault(x => x.Name == scheme.ShardName);
        if (shard is null)
        {
            throw new InvalidOperationException($"Missing [{scheme.ShardName}] model shard");
        }

        var collection = shard.Collections
            .OfType<Collection<TProperties>>()
            .SingleOrDefault(x => x.Name == scheme.Name);
        if (collection is null)
        {
            throw new InvalidOperationException($"Missing [{scheme.Name}] collection");
        }

        foreach (var change in changes)
        {
            var item = collection.Items.SingleOrDefault(x => x.Id == change.Entity.Id);
            if (item is null)
            {
                throw new InvalidOperationException($"Unable to update item with id [{change.Entity.Id}]");
            }

            item.Properties = change.NewData!;
        }
    }

    public void Delete<TEntity>(CollectionInfo scheme, IReadOnlyCollection<TEntity> entities) where TEntity : Entity
    {
        var shard = _modelShards.SingleOrDefault(x => x.Name == scheme.ShardName);
        if (shard is null)
        {
            throw new InvalidOperationException($"Missing [{scheme.ShardName}] model shard");
        }

        var collection = shard.Collections.SingleOrDefault(x => x.Name == scheme.Name);
        if (collection is null)
        {
            throw new InvalidOperationException($"Missing [{scheme.Name}] collection");
        }

        foreach (var entity in entities)
        {
            collection.Delete(entity.Id);
        }
    }

    public void Delete<TParent, TChild>(RelationInfo scheme, IReadOnlyCollection<KeyValuePair<TParent, TChild>> relations)
        where TParent : Entity
        where TChild : Entity
    {
        var shard = _modelShards.SingleOrDefault(x => x.Name == scheme.ShardName);
        if (shard is null)
        {
            throw new InvalidOperationException($"Missing [{scheme.ShardName}] model shard");
        }

        var relation = shard.Relations.SingleOrDefault(x => x.Name == scheme.Name);
        if (relation is null)
        {
            throw new InvalidOperationException($"Missing [{scheme.Name}] relation");
        }

        foreach (var pair in relations)
        {
            var item = relation.Pairs.SingleOrDefault(x => x.Parent == pair.Key.Id && x.Child == pair.Value.Id);
            if (item is not null)
            {
                relation.Pairs.Remove(item);
            }
            else
            {
                throw new InvalidOperationException($"Unable to remove pair [{pair.Key.Id}, {pair.Value.Id}]");
            }
        }
    }

    public void Select<TEntity, TProperties>(CollectionInfo scheme, IMutableCollection<TEntity, TProperties> collection)
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

    public void Select<TParent, TChild>(RelationInfo scheme, IMutableRelation<TParent, TChild> relation, IEnumerable<TParent> parentCollection, IEnumerable<TChild> childCollection)
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
            var parent = parentCollection.SingleOrDefault(x => x.Id == pair.Parent);
            var child = childCollection.SingleOrDefault(x => x.Id == pair.Child);

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

    private static Relation GetOrCreateRelation(ModelShard shard, RelationInfo scheme)
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
