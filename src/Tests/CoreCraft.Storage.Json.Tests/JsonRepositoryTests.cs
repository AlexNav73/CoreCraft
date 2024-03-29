﻿using CoreCraft.ChangesTracking;
using CoreCraft.Storage.Json.Model;

namespace CoreCraft.Storage.Json.Tests;

public class JsonRepositoryTests
{
    [Test]
    public void SaveCollectionWithItemsTest()
    {
        var entity1Id = Guid.NewGuid();
        var entity2Id = Guid.NewGuid();
        var value1 = "value1";
        var value2 = "value2";

        var collection = new Collection<FirstEntity, FirstEntityProperties>("", id => new FirstEntity(id), () => new())
        {
            { new FirstEntity(entity1Id), new() { NullableStringProperty = value1 } },
            { new FirstEntity(entity2Id), new() { NullableStringProperty = value2 } }
        };

        var shards = new List<ModelShard>();
        var repository = new JsonRepository(shards);

        repository.Save(FakeModelShardInfo.FirstCollectionInfo, collection);

        Assert.That(shards.Count, Is.EqualTo(1));
        Assert.That(shards.Single().Name, Is.EqualTo(FakeModelShardInfo.FirstCollectionInfo.ShardName));
        Assert.That(shards.Single().Collections.Count, Is.EqualTo(1));
        var jsonCollection = shards.Single().Collections.OfType<Collection<FirstEntityProperties>>().Single();
        Assert.That(jsonCollection.Name, Is.EqualTo(FakeModelShardInfo.FirstCollectionInfo.Name));
        Assert.That(jsonCollection.Items.Count, Is.EqualTo(2));
        Assert.That(jsonCollection.Items.First().Id, Is.EqualTo(entity1Id));
        Assert.That(jsonCollection.Items.First().Properties, Is.EqualTo(new FirstEntityProperties() { NullableStringProperty = value1 }));
        Assert.That(jsonCollection.Items.Last().Id, Is.EqualTo(entity2Id));
        Assert.That(jsonCollection.Items.Last().Properties, Is.EqualTo(new FirstEntityProperties() { NullableStringProperty = value2 }));
    }

    [Test]
    public void SaveRelationWithItemsTest()
    {
        var entity1Id = Guid.NewGuid();
        var entity2Id = Guid.NewGuid();
        var entity3Id = Guid.NewGuid();
        var entity4Id = Guid.NewGuid();

        var shards = new List<ModelShard>();
        var repository = new JsonRepository(shards);
        var relation = new Relation<FirstEntity, SecondEntity>("", new OneToOne<FirstEntity, SecondEntity>(), new OneToOne<SecondEntity, FirstEntity>())
        {
            { new FirstEntity(entity1Id), new SecondEntity(entity2Id) },
            { new FirstEntity(entity3Id), new SecondEntity(entity4Id) }
        };

        repository.Save(FakeModelShardInfo.OneToOneRelationInfo, relation);

        Assert.That(shards.Count, Is.EqualTo(1));
        Assert.That(shards.Single().Name, Is.EqualTo(FakeModelShardInfo.OneToOneRelationInfo.ShardName));
        Assert.That(shards.Single().Relations.Count, Is.EqualTo(1));
        var jsonRelation = shards.Single().Relations.Single();
        Assert.That(jsonRelation.Name, Is.EqualTo(FakeModelShardInfo.OneToOneRelationInfo.Name));
        Assert.That(jsonRelation.Pairs.Count, Is.EqualTo(2));
        Assert.That(jsonRelation.Pairs.First().Parent, Is.EqualTo(entity1Id));
        Assert.That(jsonRelation.Pairs.First().Child, Is.EqualTo(entity2Id));
        Assert.That(jsonRelation.Pairs.Last().Parent, Is.EqualTo(entity3Id));
        Assert.That(jsonRelation.Pairs.Last().Child, Is.EqualTo(entity4Id));
    }

    [Test]
    public void InsertForCollectionTest()
    {
        var entity1Id = Guid.NewGuid();
        var entity2Id = Guid.NewGuid();
        var value1 = "value1";
        var value2 = "value2";

        var shards = new List<ModelShard>();
        var repository = new JsonRepository(shards);

        repository.Save(
            FakeModelShardInfo.FirstCollectionInfo,
            new CollectionChangeSet<FirstEntity, FirstEntityProperties>("")
            {
                { CollectionAction.Add, new FirstEntity(entity1Id), null, new() { NullableStringProperty = value1 } },
                { CollectionAction.Add, new FirstEntity(entity2Id), null, new() { NullableStringProperty = value2 } }
            });

        Assert.That(shards.Count, Is.EqualTo(1));
        Assert.That(shards.Single().Name, Is.EqualTo(FakeModelShardInfo.FirstCollectionInfo.ShardName));
        Assert.That(shards.Single().Collections.Count, Is.EqualTo(1));
        var collection = shards.Single().Collections.OfType<Collection<FirstEntityProperties>>().Single();
        Assert.That(collection.Name, Is.EqualTo(FakeModelShardInfo.FirstCollectionInfo.Name));
        Assert.That(collection.Items.Count, Is.EqualTo(2));
        Assert.That(collection.Items.First().Id, Is.EqualTo(entity1Id));
        Assert.That(collection.Items.First().Properties, Is.EqualTo(new FirstEntityProperties() { NullableStringProperty = value1 }));
        Assert.That(collection.Items.Last().Id, Is.EqualTo(entity2Id));
        Assert.That(collection.Items.Last().Properties, Is.EqualTo(new FirstEntityProperties() { NullableStringProperty = value2 }));
    }

    [Test]
    public void InsertForRelationTest()
    {
        var entity1Id = Guid.NewGuid();
        var entity2Id = Guid.NewGuid();
        var entity3Id = Guid.NewGuid();
        var entity4Id = Guid.NewGuid();

        var shards = new List<ModelShard>();
        var repository = new JsonRepository(shards);

        repository.Save(
            FakeModelShardInfo.OneToOneRelationInfo,
            new RelationChangeSet<FirstEntity, SecondEntity>("")
            {
                { RelationAction.Linked, new FirstEntity(entity1Id), new SecondEntity(entity2Id) },
                { RelationAction.Linked, new FirstEntity(entity3Id), new SecondEntity(entity4Id) }
            });

        Assert.That(shards.Count, Is.EqualTo(1));
        Assert.That(shards.Single().Name, Is.EqualTo(FakeModelShardInfo.OneToOneRelationInfo.ShardName));
        Assert.That(shards.Single().Relations.Count, Is.EqualTo(1));
        var relation = shards.Single().Relations.Single();
        Assert.That(relation.Name, Is.EqualTo(FakeModelShardInfo.OneToOneRelationInfo.Name));
        Assert.That(relation.Pairs.Count, Is.EqualTo(2));
        Assert.That(relation.Pairs.First().Parent, Is.EqualTo(entity1Id));
        Assert.That(relation.Pairs.First().Child, Is.EqualTo(entity2Id));
        Assert.That(relation.Pairs.Last().Parent, Is.EqualTo(entity3Id));
        Assert.That(relation.Pairs.Last().Child, Is.EqualTo(entity4Id));
    }

    [Test]
    public void UpdateWithoutShardsShouldThrowTest()
    {
        var shards = new List<ModelShard>();
        var repository = new JsonRepository(shards);

        Assert.Throws<InvalidOperationException>(() => repository.Save(
                FakeModelShardInfo.FirstCollectionInfo,
                new CollectionChangeSet<FirstEntity, FirstEntityProperties>("")
                {
                    { CollectionAction.Modify, new FirstEntity(Guid.Empty), new(), new() }
                }));
    }

    [Test]
    public void UpdateWithoutCollectionShouldThrowTest()
    {
        var shards = new List<ModelShard>()
        {
            new ModelShard(FakeModelShardInfo.FirstCollectionInfo.ShardName)
        };
        var repository = new JsonRepository(shards);

        Assert.Throws<InvalidOperationException>(() => repository.Save(
            FakeModelShardInfo.FirstCollectionInfo,
            new CollectionChangeSet<FirstEntity, FirstEntityProperties>("")
            {
                { CollectionAction.Modify, new FirstEntity(Guid.Empty), new(), new() }
            }));
    }

    [Test]
    public void UpdateWithoutItemsShouldThrowTest()
    {
        var shards = new List<ModelShard>()
        {
            new ModelShard(FakeModelShardInfo.FirstCollectionInfo.ShardName)
            {
                Collections = new[]
                {
                    new Collection<FirstEntityProperties>(FakeModelShardInfo.FirstCollectionInfo.Name)
                }
            }
        };
        var repository = new JsonRepository(shards);

        Assert.Throws<InvalidOperationException>(() => repository.Save(
            FakeModelShardInfo.FirstCollectionInfo,
            new CollectionChangeSet<FirstEntity, FirstEntityProperties>("")
            {
                { CollectionAction.Modify, new FirstEntity(Guid.Empty), new(), new() }
            }));
    }

    [Test]
    public void UpdateForCollectionTest()
    {
        var entityId = Guid.NewGuid();
        var value = "value";

        var shards = new List<ModelShard>()
        {
            new ModelShard(FakeModelShardInfo.FirstCollectionInfo.ShardName)
            {
                Collections = new[]
                {
                    new Collection<FirstEntityProperties>(FakeModelShardInfo.FirstCollectionInfo.Name)
                    {
                        Items = new[]
                        {
                            new Item<FirstEntityProperties>(entityId, new())
                        }
                    }
                }
            }
        };
        var repository = new JsonRepository(shards);

        repository.Save(
            FakeModelShardInfo.FirstCollectionInfo,
            new CollectionChangeSet<FirstEntity, FirstEntityProperties>("")
            {
                { CollectionAction.Modify, new FirstEntity(entityId), new(), new() { NullableStringProperty = value } }
            });

        var item = shards.Single()
            .Collections
            .OfType<Collection<FirstEntityProperties>>()
            .Single()
            .Items
            .Single();

        Assert.That(item.Id, Is.EqualTo(entityId));
        Assert.That(item.Properties.NullableStringProperty, Is.EqualTo(value));
    }

    [Test]
    public void DeleteForCollectionTest()
    {
        var entityId = Guid.NewGuid();

        var shards = new List<ModelShard>()
        {
            new ModelShard(FakeModelShardInfo.FirstCollectionInfo.ShardName)
            {
                Collections = new List<ICollection>()
                {
                    new Collection<FirstEntityProperties>(FakeModelShardInfo.FirstCollectionInfo.Name)
                    {
                        Items = new List<Item<FirstEntityProperties>>()
                        {
                            new Item<FirstEntityProperties>(entityId, new())
                        }
                    }
                }
            }
        };
        var repository = new JsonRepository(shards);

        repository.Save(
            FakeModelShardInfo.FirstCollectionInfo,
            new CollectionChangeSet<FirstEntity, FirstEntityProperties>("")
            {
                { CollectionAction.Remove, new FirstEntity(entityId), null, null }
            });

        Assert.That(
            shards.Single().Collections.OfType<Collection<FirstEntityProperties>>().SingleOrDefault(),
            Is.Null);
    }

    [Test]
    public void DeleteWithoutShardShouldThrowTest()
    {
        var entityId = Guid.NewGuid();

        var shards = new List<ModelShard>();
        var repository = new JsonRepository(shards);

        Assert.Throws<InvalidOperationException>(() => repository.Save(
            FakeModelShardInfo.FirstCollectionInfo,
            new CollectionChangeSet<FirstEntity, FirstEntityProperties>("")
            {
                 { CollectionAction.Remove, new FirstEntity(entityId), null, null }
            }));
    }

    [Test]
    public void DeleteWithoutCollectionShouldThrowTest()
    {
        var entityId = Guid.NewGuid();

        var shards = new List<ModelShard>()
        {
            new ModelShard(FakeModelShardInfo.FirstCollectionInfo.ShardName)
        };
        var repository = new JsonRepository(shards);

        Assert.Throws<InvalidOperationException>(() => repository.Save(
            FakeModelShardInfo.FirstCollectionInfo,
            new CollectionChangeSet<FirstEntity, FirstEntityProperties>("")
            {
                { CollectionAction.Remove, new FirstEntity(entityId), null, null }
            }));
    }

    [Test]
    public void DeleteWithoutItemShouldThrowTest()
    {
        var entityId = Guid.NewGuid();

        var shards = new List<ModelShard>()
        {
            new ModelShard(FakeModelShardInfo.FirstCollectionInfo.ShardName)
            {
                Collections = new List<ICollection>()
                {
                    new Collection<FirstEntityProperties>(FakeModelShardInfo.FirstCollectionInfo.Name)
                }
            }
        };
        var repository = new JsonRepository(shards);

        Assert.Throws<InvalidOperationException>(() => repository.Save(
            FakeModelShardInfo.FirstCollectionInfo,
            new CollectionChangeSet<FirstEntity, FirstEntityProperties>("")
            {
                { CollectionAction.Remove, new FirstEntity(entityId), null, null }
            }));
    }

    [Test]
    public void DeleteForRelationTest()
    {
        var parentId = Guid.NewGuid();
        var childId = Guid.NewGuid();

        var shards = new List<ModelShard>()
        {
            new ModelShard(FakeModelShardInfo.OneToOneRelationInfo.ShardName)
            {
                Relations = new List<Relation>()
                {
                    new Relation(FakeModelShardInfo.OneToOneRelationInfo.Name)
                    {
                        Pairs = new List<Pair>()
                        {
                            new Pair(parentId, childId)
                        }
                    }
                }
            }
        };
        var repository = new JsonRepository(shards);

        repository.Save(
            FakeModelShardInfo.OneToOneRelationInfo,
            new RelationChangeSet<FirstEntity, SecondEntity>("")
            {
                { RelationAction.Unlinked, new FirstEntity(parentId), new SecondEntity(childId) }
            });

        Assert.That(shards.Single().Relations.SingleOrDefault(), Is.Null);
    }

    [Test]
    public void DeleteRelationWithoutShardShouldThrowTest()
    {
        var parentId = Guid.NewGuid();
        var childId = Guid.NewGuid();

        var shards = new List<ModelShard>();
        var repository = new JsonRepository(shards);

        Assert.Throws<InvalidOperationException>(() => repository.Save(
            FakeModelShardInfo.OneToOneRelationInfo,
            new RelationChangeSet<FirstEntity, SecondEntity>("")
            {
                { RelationAction.Unlinked, new FirstEntity(parentId), new SecondEntity(childId) }
            }));
    }

    [Test]
    public void DeleteWithoutRelationShouldThrowTest()
    {
        var parentId = Guid.NewGuid();
        var childId = Guid.NewGuid();

        var shards = new List<ModelShard>()
        {
            new ModelShard(FakeModelShardInfo.OneToOneRelationInfo.ShardName)
        };
        var repository = new JsonRepository(shards);

        Assert.Throws<InvalidOperationException>(() => repository.Save(
            FakeModelShardInfo.OneToOneRelationInfo,
            new RelationChangeSet<FirstEntity, SecondEntity>("")
            {
                { RelationAction.Unlinked, new FirstEntity(parentId), new SecondEntity(childId) }
            }));
    }

    [Test]
    public void DeleteWithoutPairShouldThrowTest()
    {
        var parentId = Guid.NewGuid();
        var childId = Guid.NewGuid();

        var shards = new List<ModelShard>()
        {
            new ModelShard(FakeModelShardInfo.OneToOneRelationInfo.ShardName)
            {
                Relations = new List<Relation>()
                {
                    new Relation(FakeModelShardInfo.OneToOneRelationInfo.Name)
                }
            }
        };
        var repository = new JsonRepository(shards);

        Assert.Throws<InvalidOperationException>(() => repository.Save(
            FakeModelShardInfo.OneToOneRelationInfo,
            new RelationChangeSet<FirstEntity, SecondEntity>("")
            {
                { RelationAction.Unlinked, new FirstEntity(parentId), new SecondEntity(childId) }
            }));
    }

    [Test]
    public void SelectCollectionWithoutShardShouldReturnEmptyResultTest()
    {
        var entityId = Guid.NewGuid();
        var props = new FirstEntityProperties() { NullableStringProperty = "value" };

        var shards = new List<ModelShard>();
        var repository = new JsonRepository(shards);
        var collection = A.Fake<IMutableCollection<FirstEntity, FirstEntityProperties>>();

        repository.Load(FakeModelShardInfo.FirstCollectionInfo, collection);

        A.CallTo(() => collection.Add(A<Guid>.Ignored, A<Func<FirstEntityProperties, FirstEntityProperties>>.Ignored))
            .MustNotHaveHappened();
    }

    [Test]
    public void SelectCollectionWithoutCollectionShouldReturnEmptyResultTest()
    {
        var entityId = Guid.NewGuid();
        var props = new FirstEntityProperties() { NullableStringProperty = "value" };

        var shards = new List<ModelShard>()
        {
            new ModelShard(FakeModelShardInfo.FirstCollectionInfo.ShardName)
        };
        var repository = new JsonRepository(shards);
        var collection = A.Fake<IMutableCollection<FirstEntity, FirstEntityProperties>>();

        repository.Load(FakeModelShardInfo.FirstCollectionInfo, collection);

        A.CallTo(() => collection.Add(A<Guid>.Ignored, A<Func<FirstEntityProperties, FirstEntityProperties>>.Ignored))
            .MustNotHaveHappened();
    }

    [Test]
    public void SelectCollectionWithoutItemsShouldReturnEmptyResultTest()
    {
        var entityId = Guid.NewGuid();
        var props = new FirstEntityProperties() { NullableStringProperty = "value" };

        var shards = new List<ModelShard>()
        {
            new ModelShard(FakeModelShardInfo.FirstCollectionInfo.ShardName)
            {
                Collections = new List<ICollection>()
                {
                    new Collection<FirstEntityProperties>(FakeModelShardInfo.FirstCollectionInfo.Name)
                }
            }
        };
        var repository = new JsonRepository(shards);
        var collection = A.Fake<IMutableCollection<FirstEntity, FirstEntityProperties>>();

        repository.Load(FakeModelShardInfo.FirstCollectionInfo, collection);

        A.CallTo(() => collection.Add(A<Guid>.Ignored, A<Func<FirstEntityProperties, FirstEntityProperties>>.Ignored))
            .MustNotHaveHappened();
    }

    [Test]
    public void SelectForCollectionTest()
    {
        var entityId = Guid.NewGuid();
        var props = new FirstEntityProperties() { NullableStringProperty = "value" };

        var shards = new List<ModelShard>()
        {
            new ModelShard(FakeModelShardInfo.FirstCollectionInfo.ShardName)
            {
                Collections = new List<ICollection>()
                {
                    new Collection<FirstEntityProperties>(FakeModelShardInfo.FirstCollectionInfo.Name)
                    {
                        Items = new List<Item<FirstEntityProperties>>()
                        {
                            new Item<FirstEntityProperties>(entityId, props)
                        }
                    }
                }
            }
        };
        var repository = new JsonRepository(shards);
        var collection = A.Fake<IMutableCollection<FirstEntity, FirstEntityProperties>>();

        repository.Load(FakeModelShardInfo.FirstCollectionInfo, collection);

        A.CallTo(() => collection.Add(A<Guid>.Ignored, A<Func<FirstEntityProperties, FirstEntityProperties>>.Ignored))
            .MustHaveHappenedOnceExactly();
    }

    [Test]
    public void SelectRelationWithoutShardShouldReturnEmptyResultTest()
    {
        var parentId = Guid.NewGuid();
        var childId = Guid.NewGuid();

        var shards = new List<ModelShard>();
        var repository = new JsonRepository(shards);
        var relation = A.Fake<IMutableRelation<FirstEntity, SecondEntity>>();
        var parentCollection = new[] { new FirstEntity(parentId) };
        var childCollection = new[] { new SecondEntity(childId) };

        repository.Load(FakeModelShardInfo.OneToOneRelationInfo, relation, parentCollection, childCollection);

        A.CallTo(() => relation.Add(A<FirstEntity>.Ignored, A<SecondEntity>.Ignored)).MustNotHaveHappened();
    }

    [Test]
    public void SelectRelationWithoutRelationShouldReturnEmptyResultTest()
    {
        var parentId = Guid.NewGuid();
        var childId = Guid.NewGuid();

        var shards = new List<ModelShard>()
        {
            new ModelShard(FakeModelShardInfo.OneToOneRelationInfo.ShardName)
        };
        var repository = new JsonRepository(shards);
        var relation = A.Fake<IMutableRelation<FirstEntity, SecondEntity>>();
        var parentCollection = new[] { new FirstEntity(parentId) };
        var childCollection = new[] { new SecondEntity(childId) };

        repository.Load(FakeModelShardInfo.OneToOneRelationInfo, relation, parentCollection, childCollection);

        A.CallTo(() => relation.Add(A<FirstEntity>.Ignored, A<SecondEntity>.Ignored)).MustNotHaveHappened();
    }

    [Test]
    public void SelectRelationWithoutPairsShouldReturnEmptyResultTest()
    {
        var parentId = Guid.NewGuid();
        var childId = Guid.NewGuid();

        var shards = new List<ModelShard>()
        {
            new ModelShard(FakeModelShardInfo.OneToOneRelationInfo.ShardName)
            {
                Relations = new List<Relation>()
                {
                    new Relation(FakeModelShardInfo.OneToOneRelationInfo.Name)
                }
            }
        };
        var repository = new JsonRepository(shards);
        var relation = A.Fake<IMutableRelation<FirstEntity, SecondEntity>>();
        var parentCollection = new[] { new FirstEntity(parentId) };
        var childCollection = new[] { new SecondEntity(childId) };

        repository.Load(FakeModelShardInfo.OneToOneRelationInfo, relation, parentCollection, childCollection);

        A.CallTo(() => relation.Add(A<FirstEntity>.Ignored, A<SecondEntity>.Ignored)).MustNotHaveHappened();
    }

    [Test]
    public void SelectForRelationTest()
    {
        var parentId = Guid.NewGuid();
        var childId = Guid.NewGuid();

        var shards = new List<ModelShard>()
        {
            new ModelShard(FakeModelShardInfo.OneToOneRelationInfo.ShardName)
            {
                Relations = new List<Relation>()
                {
                    new Relation(FakeModelShardInfo.OneToOneRelationInfo.Name)
                    {
                        Pairs = new List<Pair>() { new Pair(parentId, childId) }
                    }
                }
            }
        };
        var repository = new JsonRepository(shards);
        var relation = A.Fake<IMutableRelation<FirstEntity, SecondEntity>>();
        var parentCollection = new[] { new FirstEntity(parentId) };
        var childCollection = new[] { new SecondEntity(childId) };

        repository.Load(FakeModelShardInfo.OneToOneRelationInfo, relation, parentCollection, childCollection);

        A.CallTo(() => relation.Add(A<FirstEntity>.Ignored, A<SecondEntity>.Ignored)).MustHaveHappenedOnceExactly();
    }
}
