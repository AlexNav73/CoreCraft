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

        var collection = new Collection<FirstEntity, FirstEntityProperties>(FakeModelShardInfo.FirstCollectionInfo, id => new FirstEntity(id), () => new())
        {
            { new FirstEntity(entity1Id), new() { NullableStringProperty = value1 } },
            { new FirstEntity(entity2Id), new() { NullableStringProperty = value2 } }
        };

        var model = new Model.Model();
        var repository = new JsonRepository(model);

        repository.Save(collection);

        Assert.That(model.Shards.Count, Is.EqualTo(1));
        Assert.That(model.Shards.Single().Name, Is.EqualTo(FakeModelShardInfo.FirstCollectionInfo.ShardName));
        Assert.That(model.Shards.Single().Collections.Count, Is.EqualTo(1));
        var jsonCollection = model.Shards.Single().Collections.OfType<Collection<FirstEntityProperties>>().Single();
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

        var model = new Model.Model();
        var repository = new JsonRepository(model);
        var relation = new Relation<FirstEntity, SecondEntity>(FakeModelShardInfo.OneToOneRelationInfo, new OneToOne<FirstEntity, SecondEntity>(), new OneToOne<SecondEntity, FirstEntity>())
        {
            { new FirstEntity(entity1Id), new SecondEntity(entity2Id) },
            { new FirstEntity(entity3Id), new SecondEntity(entity4Id) }
        };

        repository.Save(relation);

        Assert.That(model.Shards.Count, Is.EqualTo(1));
        Assert.That(model.Shards.Single().Name, Is.EqualTo(FakeModelShardInfo.OneToOneRelationInfo.ShardName));
        Assert.That(model.Shards.Single().Relations.Count, Is.EqualTo(1));
        var jsonRelation = model.Shards.Single().Relations.Single();
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

        var model = new Model.Model();
        var repository = new JsonRepository(model);

        repository.Update(
            new CollectionChangeSet<FirstEntity, FirstEntityProperties>(FakeModelShardInfo.FirstCollectionInfo)
            {
                { CollectionAction.Add, new FirstEntity(entity1Id), null, new() { NullableStringProperty = value1 } },
                { CollectionAction.Add, new FirstEntity(entity2Id), null, new() { NullableStringProperty = value2 } }
            });

        Assert.That(model.Shards.Count, Is.EqualTo(1));
        Assert.That(model.Shards.Single().Name, Is.EqualTo(FakeModelShardInfo.FirstCollectionInfo.ShardName));
        Assert.That(model.Shards.Single().Collections.Count, Is.EqualTo(1));
        var collection = model.Shards.Single().Collections.OfType<Collection<FirstEntityProperties>>().Single();
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

        var model = new Model.Model();
        var repository = new JsonRepository(model);

        repository.Update(
            new RelationChangeSet<FirstEntity, SecondEntity>(FakeModelShardInfo.OneToOneRelationInfo)
            {
                { RelationAction.Linked, new FirstEntity(entity1Id), new SecondEntity(entity2Id) },
                { RelationAction.Linked, new FirstEntity(entity3Id), new SecondEntity(entity4Id) }
            });

        Assert.That(model.Shards.Count, Is.EqualTo(1));
        Assert.That(model.Shards.Single().Name, Is.EqualTo(FakeModelShardInfo.OneToOneRelationInfo.ShardName));
        Assert.That(model.Shards.Single().Relations.Count, Is.EqualTo(1));
        var relation = model.Shards.Single().Relations.Single();
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
        var model = new Model.Model();
        var repository = new JsonRepository(model);

        Assert.Throws<InvalidOperationException>(() => repository.Update(
                new CollectionChangeSet<FirstEntity, FirstEntityProperties>(FakeModelShardInfo.FirstCollectionInfo)
                {
                    { CollectionAction.Modify, new FirstEntity(Guid.Empty), new(), new() }
                }));
    }

    [Test]
    public void UpdateWithoutCollectionShouldThrowTest()
    {
        var model = new Model.Model()
        {
            Shards = [new(FakeModelShardInfo.FirstCollectionInfo.ShardName)]
        };
        var repository = new JsonRepository(model);

        Assert.Throws<InvalidOperationException>(() => repository.Update(
            new CollectionChangeSet<FirstEntity, FirstEntityProperties>(FakeModelShardInfo.FirstCollectionInfo)
            {
                { CollectionAction.Modify, new FirstEntity(Guid.Empty), new(), new() }
            }));
    }

    [Test]
    public void UpdateWithoutItemsShouldThrowTest()
    {
        var model = new Model.Model()
        {
            Shards =
            [
                new(FakeModelShardInfo.FirstCollectionInfo.ShardName)
                {
                    Collections = new[]
                    {
                        new Collection<FirstEntityProperties>(FakeModelShardInfo.FirstCollectionInfo.Name)
                    }
                }
            ]
        };
        var repository = new JsonRepository(model);

        Assert.Throws<InvalidOperationException>(() => repository.Update(
            new CollectionChangeSet<FirstEntity, FirstEntityProperties>(FakeModelShardInfo.FirstCollectionInfo)
            {
                { CollectionAction.Modify, new FirstEntity(Guid.Empty), new(), new() }
            }));
    }

    [Test]
    public void UpdateForCollectionTest()
    {
        var entityId = Guid.NewGuid();
        var value = "value";

        var model = new Model.Model()
        {
            Shards =
            [
                new(FakeModelShardInfo.FirstCollectionInfo.ShardName)
                {
                    Collections = new[]
                    {
                        new Collection<FirstEntityProperties>(FakeModelShardInfo.FirstCollectionInfo.Name)
                        {
                            Items =
                            [
                                new Item<FirstEntityProperties>(entityId, new())
                            ]
                        }
                    }
                }
            ]
        };
        var repository = new JsonRepository(model);

        repository.Update(
            new CollectionChangeSet<FirstEntity, FirstEntityProperties>(FakeModelShardInfo.FirstCollectionInfo)
            {
                { CollectionAction.Modify, new FirstEntity(entityId), new(), new() { NullableStringProperty = value } }
            });

        var item = model.Shards
            .Single()
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

        var model = new Model.Model()
        {
            Shards =
            [
                new(FakeModelShardInfo.FirstCollectionInfo.ShardName)
                {
                    Collections =
                    [
                        new Collection<FirstEntityProperties>(FakeModelShardInfo.FirstCollectionInfo.Name)
                        {
                            Items = [new(entityId, new())]
                        }
                    ]
                }
            ]
        };
        var repository = new JsonRepository(model);

        repository.Update(
            new CollectionChangeSet<FirstEntity, FirstEntityProperties>(FakeModelShardInfo.FirstCollectionInfo)
            {
                { CollectionAction.Remove, new FirstEntity(entityId), null, null }
            });

        Assert.That(
            model.Shards.Single().Collections.OfType<Collection<FirstEntityProperties>>().SingleOrDefault(),
            Is.Null);
    }

    [Test]
    public void DeleteWithoutShardShouldThrowTest()
    {
        var entityId = Guid.NewGuid();

        var model = new Model.Model();
        var repository = new JsonRepository(model);

        Assert.Throws<InvalidOperationException>(() => repository.Update(
            new CollectionChangeSet<FirstEntity, FirstEntityProperties>(FakeModelShardInfo.FirstCollectionInfo)
            {
                 { CollectionAction.Remove, new FirstEntity(entityId), null, null }
            }));
    }

    [Test]
    public void DeleteWithoutCollectionShouldThrowTest()
    {
        var entityId = Guid.NewGuid();

        var model = new Model.Model()
        {
            Shards = [new(FakeModelShardInfo.FirstCollectionInfo.ShardName)]
        };
        var repository = new JsonRepository(model);

        Assert.Throws<InvalidOperationException>(() => repository.Update(
            new CollectionChangeSet<FirstEntity, FirstEntityProperties>(FakeModelShardInfo.FirstCollectionInfo)
            {
                { CollectionAction.Remove, new FirstEntity(entityId), null, null }
            }));
    }

    [Test]
    public void DeleteWithoutItemShouldThrowTest()
    {
        var entityId = Guid.NewGuid();

        var model = new Model.Model()
        {
            Shards =
            [
                new(FakeModelShardInfo.FirstCollectionInfo.ShardName)
                {
                    Collections = [new Collection<FirstEntityProperties>(FakeModelShardInfo.FirstCollectionInfo.Name)]
                }
            ]
        };
        var repository = new JsonRepository(model);

        Assert.Throws<InvalidOperationException>(() => repository.Update(
            new CollectionChangeSet<FirstEntity, FirstEntityProperties>(FakeModelShardInfo.FirstCollectionInfo)
            {
                { CollectionAction.Remove, new FirstEntity(entityId), null, null }
            }));
    }

    [Test]
    public void DeleteForRelationTest()
    {
        var parentId = Guid.NewGuid();
        var childId = Guid.NewGuid();

        var model = new Model.Model()
        {
            Shards =
            [
                new(FakeModelShardInfo.OneToOneRelationInfo.ShardName)
                {
                    Relations =
                    [
                        new Relation(FakeModelShardInfo.OneToOneRelationInfo.Name)
                        {
                            Pairs = [new Pair(parentId, childId)]
                        }
                    ]
                }
            ]
        };
        var repository = new JsonRepository(model);

        repository.Update(
            new RelationChangeSet<FirstEntity, SecondEntity>(FakeModelShardInfo.OneToOneRelationInfo)
            {
                { RelationAction.Unlinked, new FirstEntity(parentId), new SecondEntity(childId) }
            });

        Assert.That(model.Shards.Single().Relations.SingleOrDefault(), Is.Null);
    }

    [Test]
    public void DeleteRelationWithoutShardShouldThrowTest()
    {
        var parentId = Guid.NewGuid();
        var childId = Guid.NewGuid();

        var model = new Model.Model();
        var repository = new JsonRepository(model);

        Assert.Throws<InvalidOperationException>(() => repository.Update(
            new RelationChangeSet<FirstEntity, SecondEntity>(FakeModelShardInfo.OneToOneRelationInfo)
            {
                { RelationAction.Unlinked, new FirstEntity(parentId), new SecondEntity(childId) }
            }));
    }

    [Test]
    public void DeleteWithoutRelationShouldThrowTest()
    {
        var parentId = Guid.NewGuid();
        var childId = Guid.NewGuid();

        var model = new Model.Model()
        {
            Shards = [new(FakeModelShardInfo.OneToOneRelationInfo.ShardName)]
        };
        var repository = new JsonRepository(model);

        Assert.Throws<InvalidOperationException>(() => repository.Update(
            new RelationChangeSet<FirstEntity, SecondEntity>(FakeModelShardInfo.OneToOneRelationInfo)
            {
                { RelationAction.Unlinked, new FirstEntity(parentId), new SecondEntity(childId) }
            }));
    }

    [Test]
    public void DeleteWithoutPairShouldThrowTest()
    {
        var parentId = Guid.NewGuid();
        var childId = Guid.NewGuid();

        var model = new Model.Model()
        {
            Shards =
            [
                new(FakeModelShardInfo.OneToOneRelationInfo.ShardName)
                {
                    Relations = [new(FakeModelShardInfo.OneToOneRelationInfo.Name)]
                }
            ]
        };
        var repository = new JsonRepository(model);

        Assert.Throws<InvalidOperationException>(() => repository.Update(
            new RelationChangeSet<FirstEntity, SecondEntity>(FakeModelShardInfo.OneToOneRelationInfo)
            {
                { RelationAction.Unlinked, new FirstEntity(parentId), new SecondEntity(childId) }
            }));
    }

    [Test]
    public void SaveHistoryCollectionChangeSetTest()
    {
        var entityId = Guid.NewGuid();
        var model = new Model.Model();
        var repository = new JsonRepository(model);

        var collectionChangeSet = new CollectionChangeSet<FirstEntity, FirstEntityProperties>(FakeModelShardInfo.FirstCollectionInfo)
        {
            { CollectionAction.Add, new(entityId), null, new() }
        };

        repository.Save(0, collectionChangeSet);

        Assert.That(model.Shards.Count, Is.EqualTo(0));
        Assert.That(model.ChangesHistory.Count, Is.EqualTo(1));
        var frame = model.ChangesHistory.Single().Frames.Single();
        Assert.That(frame.Name, Is.EqualTo(FakeModelShardInfo.FirstCollectionInfo.ShardName));
        var collectionChange = frame.CollectionChanges
            .OfType<Model.History.CollectionChangeSet<FirstEntity, FirstEntityProperties>>()
            .Single();
        Assert.That(collectionChange.Name, Is.EqualTo(FakeModelShardInfo.FirstCollectionInfo.Name));
        Assert.That(collectionChange.Changes.Count, Is.EqualTo(1));
        Assert.That(collectionChange.Changes.Single().Action, Is.EqualTo(CollectionAction.Add));
        Assert.That(collectionChange.Changes.Single().Entity.Id, Is.EqualTo(entityId));
        Assert.That(collectionChange.Changes.Single().OldProperties, Is.Null);
        Assert.That(collectionChange.Changes.Single().NewProperties, Is.Not.Null);
    }

    [Test]
    public void SaveHistoryRelationChangeSetTest()
    {
        var parentId = Guid.NewGuid();
        var childId = Guid.NewGuid();
        var model = new Model.Model();
        var repository = new JsonRepository(model);

        var relationChangeSet = new RelationChangeSet<FirstEntity, SecondEntity>(FakeModelShardInfo.OneToOneRelationInfo)
        {
            { RelationAction.Linked, new(parentId), new(childId) }
        };

        repository.Save(0, relationChangeSet);

        Assert.That(model.Shards.Count, Is.EqualTo(0));
        Assert.That(model.ChangesHistory.Count, Is.EqualTo(1));
        var frame = model.ChangesHistory.Single().Frames.Single();
        Assert.That(frame.Name, Is.EqualTo(FakeModelShardInfo.OneToOneRelationInfo.ShardName));
        var collectionChange = frame.RelationChanges
            .OfType<Model.History.RelationChangeSet<FirstEntity, SecondEntity>>()
            .Single();
        Assert.That(collectionChange.Name, Is.EqualTo(FakeModelShardInfo.OneToOneRelationInfo.Name));
        Assert.That(collectionChange.Changes.Count, Is.EqualTo(1));
        Assert.That(collectionChange.Changes.Single().Action, Is.EqualTo(RelationAction.Linked));
        Assert.That(collectionChange.Changes.Single().Parent.Id, Is.EqualTo(parentId));
        Assert.That(collectionChange.Changes.Single().Child.Id, Is.EqualTo(childId));
    }

    [Test]
    public void SaveHistoryCollectionChangeSetWithoutChangesTest()
    {
        var model = new Model.Model();
        var repository = new JsonRepository(model);

        var collectionChangeSet = new CollectionChangeSet<FirstEntity, FirstEntityProperties>(FakeModelShardInfo.FirstCollectionInfo);

        repository.Save(0, collectionChangeSet);

        Assert.That(model.Shards.Count, Is.EqualTo(0));
        Assert.That(model.ChangesHistory.Count, Is.EqualTo(0));
    }

    [Test]
    public void SaveHistoryRelationChangeSetWithoutChangesTest()
    {
        var model = new Model.Model();
        var repository = new JsonRepository(model);

        var relationChangeSet = new RelationChangeSet<FirstEntity, SecondEntity>(FakeModelShardInfo.OneToOneRelationInfo);

        repository.Save(0, relationChangeSet);

        Assert.That(model.Shards.Count, Is.EqualTo(0));
        Assert.That(model.ChangesHistory.Count, Is.EqualTo(0));
    }

    [Test]
    public void SelectCollectionWithoutShardShouldReturnEmptyResultTest()
    {
        var entityId = Guid.NewGuid();
        var props = new FirstEntityProperties() { NullableStringProperty = "value" };

        var model = new Model.Model();
        var repository = new JsonRepository(model);
        var collection = A.Fake<IMutableCollection<FirstEntity, FirstEntityProperties>>();

        repository.Load(collection);

        A.CallTo(() => collection.Add(A<Guid>.Ignored, A<Func<FirstEntityProperties, FirstEntityProperties>>.Ignored))
            .MustNotHaveHappened();
    }

    [Test]
    public void SelectCollectionWithoutCollectionShouldReturnEmptyResultTest()
    {
        var entityId = Guid.NewGuid();
        var props = new FirstEntityProperties() { NullableStringProperty = "value" };

        var model = new Model.Model()
        {
            Shards = [new(FakeModelShardInfo.FirstCollectionInfo.ShardName)]
        };
        var repository = new JsonRepository(model);
        var collection = A.Fake<IMutableCollection<FirstEntity, FirstEntityProperties>>();

        repository.Load(collection);

        A.CallTo(() => collection.Add(A<Guid>.Ignored, A<Func<FirstEntityProperties, FirstEntityProperties>>.Ignored))
            .MustNotHaveHappened();
    }

    [Test]
    public void SelectCollectionWithoutItemsShouldReturnEmptyResultTest()
    {
        var entityId = Guid.NewGuid();
        var props = new FirstEntityProperties() { NullableStringProperty = "value" };

        var model = new Model.Model()
        {
            Shards =
            [
                new(FakeModelShardInfo.FirstCollectionInfo.ShardName)
                {
                    Collections =
                    [
                        new Collection<FirstEntityProperties>(FakeModelShardInfo.FirstCollectionInfo.Name)
                    ]
                }
            ]
        };
        var repository = new JsonRepository(model);
        var collection = A.Fake<IMutableCollection<FirstEntity, FirstEntityProperties>>();

        repository.Load(collection);

        A.CallTo(() => collection.Add(A<Guid>.Ignored, A<Func<FirstEntityProperties, FirstEntityProperties>>.Ignored))
            .MustNotHaveHappened();
    }

    [Test]
    public void SelectForCollectionTest()
    {
        var entityId = Guid.NewGuid();
        var props = new FirstEntityProperties() { NullableStringProperty = "value" };

        var model = new Model.Model()
        {
            Shards =
            [
                new(FakeModelShardInfo.FirstCollectionInfo.ShardName)
                {
                    Collections =
                    [
                        new Collection<FirstEntityProperties>(FakeModelShardInfo.FirstCollectionInfo.Name)
                        {
                            Items = [new(entityId, props)]
                        }
                    ]
                }
            ]
        };
        var repository = new JsonRepository(model);
        var collection = A.Fake<IMutableCollection<FirstEntity, FirstEntityProperties>>();

        A.CallTo(() => collection.Info).Returns(FakeModelShardInfo.FirstCollectionInfo);

        repository.Load(collection);

        A.CallTo(() => collection.Add(A<Guid>.Ignored, A<Func<FirstEntityProperties, FirstEntityProperties>>.Ignored))
            .MustHaveHappenedOnceExactly();
    }

    [Test]
    public void SelectRelationWithoutShardShouldReturnEmptyResultTest()
    {
        var parentId = Guid.NewGuid();
        var childId = Guid.NewGuid();

        var model = new Model.Model();
        var repository = new JsonRepository(model);
        var relation = A.Fake<IMutableRelation<FirstEntity, SecondEntity>>();
        var parentCollection = new[] { new FirstEntity(parentId) };
        var childCollection = new[] { new SecondEntity(childId) };

        repository.Load(relation, parentCollection, childCollection);

        A.CallTo(() => relation.Add(A<FirstEntity>.Ignored, A<SecondEntity>.Ignored)).MustNotHaveHappened();
    }

    [Test]
    public void SelectRelationWithoutRelationShouldReturnEmptyResultTest()
    {
        var parentId = Guid.NewGuid();
        var childId = Guid.NewGuid();

        var model = new Model.Model()
        {
            Shards = [new(FakeModelShardInfo.OneToOneRelationInfo.ShardName)]
        };
        var repository = new JsonRepository(model);
        var relation = A.Fake<IMutableRelation<FirstEntity, SecondEntity>>();
        var parentCollection = new[] { new FirstEntity(parentId) };
        var childCollection = new[] { new SecondEntity(childId) };

        repository.Load(relation, parentCollection, childCollection);

        A.CallTo(() => relation.Add(A<FirstEntity>.Ignored, A<SecondEntity>.Ignored)).MustNotHaveHappened();
    }

    [Test]
    public void SelectRelationWithoutPairsShouldReturnEmptyResultTest()
    {
        var parentId = Guid.NewGuid();
        var childId = Guid.NewGuid();

        var model = new Model.Model()
        {
            Shards = [
                new(FakeModelShardInfo.OneToOneRelationInfo.ShardName)
                {
                    Relations = [new(FakeModelShardInfo.OneToOneRelationInfo.Name)]
                }
            ]
        };
        var repository = new JsonRepository(model);
        var relation = A.Fake<IMutableRelation<FirstEntity, SecondEntity>>();
        var parentCollection = new[] { new FirstEntity(parentId) };
        var childCollection = new[] { new SecondEntity(childId) };

        repository.Load(relation, parentCollection, childCollection);

        A.CallTo(() => relation.Add(A<FirstEntity>.Ignored, A<SecondEntity>.Ignored)).MustNotHaveHappened();
    }

    [Test]
    public void SelectForRelationTest()
    {
        var parentId = Guid.NewGuid();
        var childId = Guid.NewGuid();

        var model = new Model.Model()
        {
            Shards =
            [
                new(FakeModelShardInfo.OneToOneRelationInfo.ShardName)
                {
                    Relations =
                    [
                        new(FakeModelShardInfo.OneToOneRelationInfo.Name)
                        {
                            Pairs = [new(parentId, childId)]
                        }
                    ]
                }
            ]
        };
        var repository = new JsonRepository(model);
        var relation = A.Fake<IMutableRelation<FirstEntity, SecondEntity>>();
        var parentCollection = new[] { new FirstEntity(parentId) };
        var childCollection = new[] { new SecondEntity(childId) };

        A.CallTo(() => relation.Info).Returns(FakeModelShardInfo.OneToOneRelationInfo);

        repository.Load(relation, parentCollection, childCollection);

        A.CallTo(() => relation.Add(A<FirstEntity>.Ignored, A<SecondEntity>.Ignored)).MustHaveHappenedOnceExactly();
    }

    [Test]
    public void LoadChangesHistoryForCollectionTest()
    {
        var entityId = Guid.NewGuid();
        var props = new FirstEntityProperties() { NullableStringProperty = "value" };

        var changes = new Model.History.ModelChanges(0);
        changes
            .Create(FakeModelShardInfo.FirstCollectionInfo.ShardName)
            .CreateCollectionChangeSet<FirstEntity, FirstEntityProperties>(FakeModelShardInfo.FirstCollectionInfo)
            .Create(new(entityId), props);
        var model = new Model.Model()
        {
            ChangesHistory = [changes]
        };
        var repository = new JsonRepository(model);
        var collectionChangeSet = A.Fake<ICollectionChangeSet<FirstEntity, FirstEntityProperties>>();

        A.CallTo(() => collectionChangeSet.Info).Returns(FakeModelShardInfo.FirstCollectionInfo);

        repository.Load(0, collectionChangeSet);

        A.CallTo(() => collectionChangeSet.Add(A<CollectionAction>.Ignored, A<FirstEntity>.Ignored, A<FirstEntityProperties>.Ignored, A<FirstEntityProperties>.Ignored))
            .MustHaveHappenedOnceExactly();
    }

    [Test]
    public void LoadChangesHistoryForRelationTest()
    {
        var parentId = Guid.NewGuid();
        var childId = Guid.NewGuid();

        var change = new Model.History.ModelChanges(0);
        change
            .Create(FakeModelShardInfo.OneToOneRelationInfo.ShardName)
            .CreateRelationChangeSet<FirstEntity, SecondEntity>(FakeModelShardInfo.OneToOneRelationInfo)
            .Create(new(parentId), new(childId));
        var model = new Model.Model()
        {
            ChangesHistory = [change]
        };
        var repository = new JsonRepository(model);
        var relationChangeSet = A.Fake<IRelationChangeSet<FirstEntity, SecondEntity>>();

        A.CallTo(() => relationChangeSet.Info).Returns(FakeModelShardInfo.OneToOneRelationInfo);

        repository.Load(0, relationChangeSet);

        A.CallTo(() => relationChangeSet.Add(A<RelationAction>.Ignored, A<FirstEntity>.Ignored, A<SecondEntity>.Ignored))
            .MustHaveHappenedOnceExactly();
    }

    [Test]
    public void LoadHistoryTest()
    {
        var parentId = Guid.NewGuid();
        var childId = Guid.NewGuid();

        var change = new Model.History.ModelChanges(0);
        change
            .Create(FakeModelShardInfo.OneToOneRelationInfo.ShardName)
            .CreateRelationChangeSet<FirstEntity, SecondEntity>(FakeModelShardInfo.OneToOneRelationInfo)
            .Create(new(parentId), new(childId));
        var model = new Model.Model()
        {
            ChangesHistory = [change]
        };
        var repository = new JsonRepository(model);
        var frame = A.Fake<IChangesFrame>(c => c.Implements<IChangesFrameEx>());
        var modelShard = A.Fake<IModelShard>(c => c.Implements<IFrameFactory>());

        A.CallTo(() => ((IFrameFactory)modelShard).Create()).Returns(frame);
        A.CallTo(() => frame.HasChanges()).Returns(true);

        var changes = repository.RestoreHistory([modelShard]).ToList();

        Assert.That(changes.Count, Is.EqualTo(1));
        Assert.That(changes.Single().HasChanges(), Is.True);
        Assert.That(changes.Single().Single().HasChanges(), Is.True);
    }
}
