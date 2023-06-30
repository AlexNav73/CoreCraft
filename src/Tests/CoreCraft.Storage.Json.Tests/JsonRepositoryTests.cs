using CoreCraft.Engine.ChangesTracking;
using CoreCraft.Storage.Json.Model;

namespace CoreCraft.Storage.Json.Tests;

public class JsonRepositoryTests
{
    [Test]
    public void InsertForCollectionTest()
    {
        var entity1Id = Guid.NewGuid();
        var entity2Id = Guid.NewGuid();
        var value1 = "value1";
        var value2 = "value2";

        var shards = new List<ModelShard>();
        var repo = new JsonRepository(shards);

        repo.Insert(FakeModelShardStorage.FirstCollectionInfo, new[]
        {
            new KeyValuePair<FirstEntity, FirstEntityProperties>(
                new FirstEntity(entity1Id),
                new() { NullableStringProperty = value1 }),
            new KeyValuePair<FirstEntity, FirstEntityProperties>(
                new FirstEntity(entity2Id),
                new() { NullableStringProperty = value2 })
        });

        Assert.That(shards.Count, Is.EqualTo(1));
        Assert.That(shards.Single().Name, Is.EqualTo(FakeModelShardStorage.FirstCollectionInfo.ShardName));
        Assert.That(shards.Single().Collections.Count, Is.EqualTo(1));
        var collection = shards.Single().Collections.OfType<Collection<FirstEntityProperties>>().Single();
        Assert.That(collection.Name, Is.EqualTo(FakeModelShardStorage.FirstCollectionInfo.Name));
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
        var repo = new JsonRepository(shards);

        repo.Insert(FakeModelShardStorage.OneToOneRelationInfo, new[]
        {
            new KeyValuePair<FirstEntity, SecondEntity>(
                new FirstEntity(entity1Id),
                new SecondEntity(entity2Id)),
            new KeyValuePair<FirstEntity, SecondEntity>(
                new FirstEntity(entity3Id),
                new SecondEntity(entity4Id))
        });

        Assert.That(shards.Count, Is.EqualTo(1));
        Assert.That(shards.Single().Name, Is.EqualTo(FakeModelShardStorage.OneToOneRelationInfo.ShardName));
        Assert.That(shards.Single().Relations.Count, Is.EqualTo(1));
        var relation = shards.Single().Relations.Single();
        Assert.That(relation.Name, Is.EqualTo(FakeModelShardStorage.OneToOneRelationInfo.Name));
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
        var repo = new JsonRepository(shards);

        Assert.Throws<InvalidOperationException>(() => repo.Update(FakeModelShardStorage.FirstCollectionInfo, new[]
        {
            new CollectionChange<FirstEntity, FirstEntityProperties>(
                CollectionAction.Modify,
                new FirstEntity(Guid.Empty),
                new(),
                new())
        }));
    }

    [Test]
    public void UpdateWithoutCollectionShouldThrowTest()
    {
        var shards = new List<ModelShard>()
        {
            new ModelShard(FakeModelShardStorage.FirstCollectionInfo.ShardName)
        };
        var repo = new JsonRepository(shards);

        Assert.Throws<InvalidOperationException>(() => repo.Update(FakeModelShardStorage.FirstCollectionInfo, new[]
        {
            new CollectionChange<FirstEntity, FirstEntityProperties>(
                CollectionAction.Modify,
                new FirstEntity(Guid.Empty),
                new(),
                new())
        }));
    }

    [Test]
    public void UpdateWithoutItemsShouldThrowTest()
    {
        var shards = new List<ModelShard>()
        {
            new ModelShard(FakeModelShardStorage.FirstCollectionInfo.ShardName)
            {
                Collections = new[]
                {
                    new Collection<FirstEntityProperties>(FakeModelShardStorage.FirstCollectionInfo.Name)
                }
            }
        };
        var repo = new JsonRepository(shards);

        Assert.Throws<InvalidOperationException>(() => repo.Update(FakeModelShardStorage.FirstCollectionInfo, new[]
        {
            new CollectionChange<FirstEntity, FirstEntityProperties>(
                CollectionAction.Modify,
                new FirstEntity(Guid.Empty),
                new(),
                new())
        }));
    }

    [Test]
    public void UpdateForCollectionTest()
    {
        var entityId = Guid.NewGuid();
        var value = "value";

        var shards = new List<ModelShard>()
        {
            new ModelShard(FakeModelShardStorage.FirstCollectionInfo.ShardName)
            {
                Collections = new[]
                {
                    new Collection<FirstEntityProperties>(FakeModelShardStorage.FirstCollectionInfo.Name)
                    {
                        Items = new[]
                        {
                            new Item<FirstEntityProperties>(entityId, new())
                        }
                    }
                }
            }
        };
        var repo = new JsonRepository(shards);

        repo.Update(FakeModelShardStorage.FirstCollectionInfo, new[]
        {
            new CollectionChange<FirstEntity, FirstEntityProperties>(
                CollectionAction.Modify,
                new FirstEntity(entityId),
                new(),
                new() { NullableStringProperty = value })
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
            new ModelShard(FakeModelShardStorage.FirstCollectionInfo.ShardName)
            {
                Collections = new List<ICollection>()
                {
                    new Collection<FirstEntityProperties>(FakeModelShardStorage.FirstCollectionInfo.Name)
                    {
                        Items = new List<Item<FirstEntityProperties>>()
                        {
                            new Item<FirstEntityProperties>(entityId, new())
                        }
                    }
                }
            }
        };
        var repo = new JsonRepository(shards);

        repo.Delete(FakeModelShardStorage.FirstCollectionInfo, new[] { new FirstEntity(entityId) });

        Assert.That(
            shards.Single().Collections.OfType<Collection<FirstEntityProperties>>().Single().Items.Count,
            Is.EqualTo(0));
    }

    [Test]
    public void DeleteWithoutShardShouldThrowTest()
    {
        var entityId = Guid.NewGuid();

        var shards = new List<ModelShard>();
        var repo = new JsonRepository(shards);

        Assert.Throws<InvalidOperationException>(() => repo.Delete(FakeModelShardStorage.FirstCollectionInfo, new[] { new FirstEntity(entityId) }));
    }

    [Test]
    public void DeleteWithoutCollectionShouldThrowTest()
    {
        var entityId = Guid.NewGuid();

        var shards = new List<ModelShard>()
        {
            new ModelShard(FakeModelShardStorage.FirstCollectionInfo.ShardName)
        };
        var repo = new JsonRepository(shards);

        Assert.Throws<InvalidOperationException>(() => repo.Delete(FakeModelShardStorage.FirstCollectionInfo, new[] { new FirstEntity(entityId) }));
    }

    [Test]
    public void DeleteWithoutItemShouldThrowTest()
    {
        var entityId = Guid.NewGuid();

        var shards = new List<ModelShard>()
        {
            new ModelShard(FakeModelShardStorage.FirstCollectionInfo.ShardName)
            {
                Collections = new List<ICollection>()
                {
                    new Collection<FirstEntityProperties>(FakeModelShardStorage.FirstCollectionInfo.Name)
                }
            }
        };
        var repo = new JsonRepository(shards);

        Assert.Throws<InvalidOperationException>(() => repo.Delete(FakeModelShardStorage.FirstCollectionInfo, new[] { new FirstEntity(entityId) }));
    }

    [Test]
    public void DeleteForRelationTest()
    {
        var parentId = Guid.NewGuid();
        var childId = Guid.NewGuid();

        var shards = new List<ModelShard>()
        {
            new ModelShard(FakeModelShardStorage.OneToOneRelationInfo.ShardName)
            {
                Relations = new List<Relation>()
                {
                    new Relation(FakeModelShardStorage.OneToOneRelationInfo.Name)
                    {
                        Pairs = new List<Pair>()
                        {
                            new Pair(parentId, childId)
                        }
                    }
                }
            }
        };
        var repo = new JsonRepository(shards);

        repo.Delete(FakeModelShardStorage.OneToOneRelationInfo, new[]
        {
            new KeyValuePair<FirstEntity, SecondEntity>(new FirstEntity(parentId), new SecondEntity(childId))
        });

        Assert.That(shards.Single().Relations.Single().Pairs.Count, Is.EqualTo(0));
    }

    [Test]
    public void DeleteRelationWithoutShardShouldThrowTest()
    {
        var parentId = Guid.NewGuid();
        var childId = Guid.NewGuid();

        var shards = new List<ModelShard>();
        var repo = new JsonRepository(shards);

        Assert.Throws<InvalidOperationException>(() => repo.Delete(FakeModelShardStorage.OneToOneRelationInfo, new[]
        {
            new KeyValuePair<FirstEntity, SecondEntity>(new FirstEntity(parentId), new SecondEntity(childId))
        }));
    }

    [Test]
    public void DeleteWithoutRelationShouldThrowTest()
    {
        var parentId = Guid.NewGuid();
        var childId = Guid.NewGuid();

        var shards = new List<ModelShard>()
        {
            new ModelShard(FakeModelShardStorage.OneToOneRelationInfo.ShardName)
        };
        var repo = new JsonRepository(shards);

        Assert.Throws<InvalidOperationException>(() => repo.Delete(FakeModelShardStorage.OneToOneRelationInfo, new[]
        {
            new KeyValuePair<FirstEntity, SecondEntity>(new FirstEntity(parentId), new SecondEntity(childId))
        }));
    }

    [Test]
    public void DeleteWithoutPairShouldThrowTest()
    {
        var parentId = Guid.NewGuid();
        var childId = Guid.NewGuid();

        var shards = new List<ModelShard>()
        {
            new ModelShard(FakeModelShardStorage.OneToOneRelationInfo.ShardName)
            {
                Relations = new List<Relation>()
                {
                    new Relation(FakeModelShardStorage.OneToOneRelationInfo.Name)
                }
            }
        };
        var repo = new JsonRepository(shards);

        Assert.Throws<InvalidOperationException>(() => repo.Delete(FakeModelShardStorage.OneToOneRelationInfo, new[]
        {
            new KeyValuePair<FirstEntity, SecondEntity>(new FirstEntity(parentId), new SecondEntity(childId))
        }));
    }

    [Test]
    public void SelectCollectionWithoutShardShouldReturnEmptyResultTest()
    {
        var entityId = Guid.NewGuid();
        var props = new FirstEntityProperties() { NullableStringProperty = "value" };

        var shards = new List<ModelShard>();
        var repo = new JsonRepository(shards);
        var collection = A.Fake<IMutableCollection<FirstEntity, FirstEntityProperties>>();

        repo.Select(FakeModelShardStorage.FirstCollectionInfo, collection);

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
            new ModelShard(FakeModelShardStorage.FirstCollectionInfo.ShardName)
        };
        var repo = new JsonRepository(shards);
        var collection = A.Fake<IMutableCollection<FirstEntity, FirstEntityProperties>>();

        repo.Select(FakeModelShardStorage.FirstCollectionInfo, collection);

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
            new ModelShard(FakeModelShardStorage.FirstCollectionInfo.ShardName)
            {
                Collections = new List<ICollection>()
                {
                    new Collection<FirstEntityProperties>(FakeModelShardStorage.FirstCollectionInfo.Name)
                }
            }
        };
        var repo = new JsonRepository(shards);
        var collection = A.Fake<IMutableCollection<FirstEntity, FirstEntityProperties>>();

        repo.Select(FakeModelShardStorage.FirstCollectionInfo, collection);

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
            new ModelShard(FakeModelShardStorage.FirstCollectionInfo.ShardName)
            {
                Collections = new List<ICollection>()
                {
                    new Collection<FirstEntityProperties>(FakeModelShardStorage.FirstCollectionInfo.Name)
                    {
                        Items = new List<Item<FirstEntityProperties>>()
                        {
                            new Item<FirstEntityProperties>(entityId, props)
                        }
                    }
                }
            }
        };
        var repo = new JsonRepository(shards);
        var collection = A.Fake<IMutableCollection<FirstEntity, FirstEntityProperties>>();

        repo.Select(FakeModelShardStorage.FirstCollectionInfo, collection);

        A.CallTo(() => collection.Add(A<Guid>.Ignored, A<Func<FirstEntityProperties, FirstEntityProperties>>.Ignored))
            .MustHaveHappenedOnceExactly();
    }

    [Test]
    public void SelectRelationWithoutShardShouldReturnEmptyResultTest()
    {
        var parentId = Guid.NewGuid();
        var childId = Guid.NewGuid();

        var shards = new List<ModelShard>();
        var repo = new JsonRepository(shards);
        var relation = A.Fake<IMutableRelation<FirstEntity, SecondEntity>>();
        var parentCollection = new[] { new FirstEntity(parentId) };
        var childCollection = new[] { new SecondEntity(childId) };

        repo.Select(FakeModelShardStorage.OneToOneRelationInfo, relation, parentCollection, childCollection);

        A.CallTo(() => relation.Add(A<FirstEntity>.Ignored, A<SecondEntity>.Ignored)).MustNotHaveHappened();
    }

    [Test]
    public void SelectRelationWithoutRelationShouldReturnEmptyResultTest()
    {
        var parentId = Guid.NewGuid();
        var childId = Guid.NewGuid();

        var shards = new List<ModelShard>()
        {
            new ModelShard(FakeModelShardStorage.OneToOneRelationInfo.ShardName)
        };
        var repo = new JsonRepository(shards);
        var relation = A.Fake<IMutableRelation<FirstEntity, SecondEntity>>();
        var parentCollection = new[] { new FirstEntity(parentId) };
        var childCollection = new[] { new SecondEntity(childId) };

        repo.Select(FakeModelShardStorage.OneToOneRelationInfo, relation, parentCollection, childCollection);

        A.CallTo(() => relation.Add(A<FirstEntity>.Ignored, A<SecondEntity>.Ignored)).MustNotHaveHappened();
    }

    [Test]
    public void SelectRelationWithoutPairsShouldReturnEmptyResultTest()
    {
        var parentId = Guid.NewGuid();
        var childId = Guid.NewGuid();

        var shards = new List<ModelShard>()
        {
            new ModelShard(FakeModelShardStorage.OneToOneRelationInfo.ShardName)
            {
                Relations = new List<Relation>()
                {
                    new Relation(FakeModelShardStorage.OneToOneRelationInfo.Name)
                }
            }
        };
        var repo = new JsonRepository(shards);
        var relation = A.Fake<IMutableRelation<FirstEntity, SecondEntity>>();
        var parentCollection = new[] { new FirstEntity(parentId) };
        var childCollection = new[] { new SecondEntity(childId) };

        repo.Select(FakeModelShardStorage.OneToOneRelationInfo, relation, parentCollection, childCollection);

        A.CallTo(() => relation.Add(A<FirstEntity>.Ignored, A<SecondEntity>.Ignored)).MustNotHaveHappened();
    }

    [Test]
    public void SelectForRelationTest()
    {
        var parentId = Guid.NewGuid();
        var childId = Guid.NewGuid();

        var shards = new List<ModelShard>()
        {
            new ModelShard(FakeModelShardStorage.OneToOneRelationInfo.ShardName)
            {
                Relations = new List<Relation>()
                {
                    new Relation(FakeModelShardStorage.OneToOneRelationInfo.Name)
                    {
                        Pairs = new List<Pair>() { new Pair(parentId, childId) }
                    }
                }
            }
        };
        var repo = new JsonRepository(shards);
        var relation = A.Fake<IMutableRelation<FirstEntity, SecondEntity>>();
        var parentCollection = new[] { new FirstEntity(parentId) };
        var childCollection = new[] { new SecondEntity(childId) };

        repo.Select(FakeModelShardStorage.OneToOneRelationInfo, relation, parentCollection, childCollection);

        A.CallTo(() => relation.Add(A<FirstEntity>.Ignored, A<SecondEntity>.Ignored)).MustHaveHappenedOnceExactly();
    }
}
