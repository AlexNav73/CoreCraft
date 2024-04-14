using CoreCraft.ChangesTracking;
using CoreCraft.Exceptions;

namespace CoreCraft.Storage.Sqlite.Tests;

public class SqliteRepositoryTests
{
    [Test]
    public void SetDatabaseVersionInMemoryTest()
    {
        using var repository = new SqliteRepository(":memory:");

        repository.SetDatabaseVersion(1);
        var version = repository.GetDatabaseVersion();

        Assert.That(version, Is.EqualTo(1));
    }

    [Test]
    public void LoggingTest()
    {
        string? query = null;
        using var repository = new SqliteRepository(":memory:", sql => query = sql);

        var id = Guid.Parse("f4a0e880-b549-4e9d-a58c-716eab14e9f1");

        repository.Update(
            new CollectionChangeSet<FirstEntity, FirstEntityProperties>(FakeModelShardInfo.FirstCollectionInfo)
            {
                { CollectionAction.Add, new(id), null, new FirstEntityProperties() { NonNullableStringProperty = "first entity" } }
            });

        Assert.That(query, Is.EqualTo("INSERT INTO [Fake.FirstCollection] ([Id], [NonNullableStringProperty], [NullableStringProperty], [NullableStringWithDefaultValueProperty]) VALUES (f4a0e880-b549-4e9d-a58c-716eab14e9f1, first entity, NULL, NULL);"));
    }

    [Test]
    public void SaveCollectionWithItemsTest()
    {
        var value = "first entity";
        var id = Guid.NewGuid();

        var collection = new Collection<FirstEntity, FirstEntityProperties>(FakeModelShardInfo.FirstCollectionInfo, id => new FirstEntity(id), () => new())
        {
            { new FirstEntity(id), new() { NullableStringProperty = value } }
        };

        using var repository = new SqliteRepository(":memory:");

        repository.Save(collection);

        var loadedCollection = new Collection<FirstEntity, FirstEntityProperties>(FakeModelShardInfo.FirstCollectionInfo, id => new(id), () => new());
        repository.Load(loadedCollection);

        Assert.That(loadedCollection.Count, Is.EqualTo(1));
        Assert.That(loadedCollection.Single().Id, Is.EqualTo(id));
        Assert.That(loadedCollection.Get(collection.Single()).NullableStringProperty, Is.EqualTo(value));
    }

    [Test]
    public void SaveRelationWithItemsTest()
    {
        using var repository = new SqliteRepository(":memory:");

        var entity1 = new FirstEntity(Guid.NewGuid());
        var entity2 = new SecondEntity(Guid.NewGuid());
        var parentCollection = new Collection<FirstEntity, FirstEntityProperties>(FakeModelShardInfo.FirstCollectionInfo, id => new(id), () => new());
        var childCollection = new Collection<SecondEntity, SecondEntityProperties>(FakeModelShardInfo.SecondCollectionInfo, id => new(id), () => new());
        parentCollection.Add(entity1, new());
        childCollection.Add(entity2, new());

        var relation = new Relation<FirstEntity, SecondEntity>(FakeModelShardInfo.OneToOneRelationInfo, new OneToOne<FirstEntity, SecondEntity>(), new OneToOne<SecondEntity, FirstEntity>())
        {
            { entity1, entity2 }
        };

        repository.Save(relation);

        var loadedRelation = new Relation<FirstEntity, SecondEntity>(FakeModelShardInfo.OneToOneRelationInfo, new OneToOne<FirstEntity, SecondEntity>(), new OneToOne<SecondEntity, FirstEntity>());
        repository.Load(loadedRelation, parentCollection, childCollection);

        Assert.That(loadedRelation.Count, Is.EqualTo(1));
        Assert.That(loadedRelation.Children(entity1), Is.EquivalentTo(new[] { entity2 }));
        Assert.That(loadedRelation.Parents(entity2), Is.EquivalentTo(new[] { entity1 }));
    }

    [Test]
    public void SaveHistoryCollectionWithItemsTest()
    {
        using var repository = new SqliteRepository(":memory:");

        var entity = new FirstEntity(Guid.NewGuid());
        var properties = new FirstEntityProperties();
        var frame = new FakeChangesFrame();

        frame.FirstCollection.Add(CollectionAction.Add, entity, null, properties);

        repository.ExecuteNonQuery(QueryBuilder.History.CreateCollectionTable);

        repository.Save(42, frame.FirstCollection);

        var loadedFrame = new FakeChangesFrame();
        repository.Load(42, loadedFrame.FirstCollection);

        Assert.That(loadedFrame.FirstCollection.Count(), Is.EqualTo(1));
        Assert.That(loadedFrame.FirstCollection.Single().Action, Is.EqualTo(CollectionAction.Add));
        Assert.That(loadedFrame.FirstCollection.Single().Entity, Is.EqualTo(entity));
        Assert.That(loadedFrame.FirstCollection.Single().OldData, Is.Null);
        Assert.That(loadedFrame.FirstCollection.Single().NewData, Is.EqualTo(properties));
    }

    [Test]
    public void SaveHistoryRelationWithItemsTest()
    {
        using var repository = new SqliteRepository(":memory:");

        var parent = new FirstEntity(Guid.NewGuid());
        var child = new SecondEntity(Guid.NewGuid());
        var frame = new FakeChangesFrame();

        frame.OneToOneRelation.Add(RelationAction.Linked, parent, child);

        repository.ExecuteNonQuery(QueryBuilder.History.CreateRelationTable);

        repository.Save(42, frame.OneToOneRelation);

        var loadedFrame = new FakeChangesFrame();
        repository.Load(42, loadedFrame.OneToOneRelation);

        Assert.That(loadedFrame.OneToOneRelation.Count(), Is.EqualTo(1));
        Assert.That(loadedFrame.OneToOneRelation.Single().Action, Is.EqualTo(RelationAction.Linked));
        Assert.That(loadedFrame.OneToOneRelation.Single().Parent, Is.EqualTo(parent));
        Assert.That(loadedFrame.OneToOneRelation.Single().Child, Is.EqualTo(child));
    }

    [Test]
    public void SaveHistoryCollectionWithoutItemsTest()
    {
        using var repository = new SqliteRepository(":memory:");

        var entity = new FirstEntity(Guid.NewGuid());
        var properties = new FirstEntityProperties();
        var frame = new FakeChangesFrame();

        repository.ExecuteNonQuery(QueryBuilder.History.CreateCollectionTable);

        repository.Save(42, frame.FirstCollection);

        var loadedFrame = new FakeChangesFrame();
        repository.Load(42, loadedFrame.FirstCollection);

        Assert.That(loadedFrame.FirstCollection.Count(), Is.EqualTo(0));
    }

    [Test]
    public void SaveHistoryRelationWithoutItemsTest()
    {
        using var repository = new SqliteRepository(":memory:");

        var parent = new FirstEntity(Guid.NewGuid());
        var child = new SecondEntity(Guid.NewGuid());
        var frame = new FakeChangesFrame();

        repository.ExecuteNonQuery(QueryBuilder.History.CreateRelationTable);

        repository.Save(42, frame.OneToOneRelation);

        var loadedFrame = new FakeChangesFrame();
        repository.Load(42, loadedFrame.OneToOneRelation);

        Assert.That(loadedFrame.OneToOneRelation.Count(), Is.EqualTo(0));
    }

    [Test]
    public void InsertCollectionTest()
    {
        using var repository = new SqliteRepository(":memory:");

        var value = "first entity";
        var id = Guid.NewGuid();

        repository.Update(
            new CollectionChangeSet<FirstEntity, FirstEntityProperties>(FakeModelShardInfo.FirstCollectionInfo)
            {
                { CollectionAction.Add, new(id), null, new FirstEntityProperties() { NonNullableStringProperty = value } }
            });

        var collection = new Collection<FirstEntity, FirstEntityProperties>(
            FakeModelShardInfo.FirstCollectionInfo, id => new(id), () => new());
        repository.Load(collection);

        Assert.That(collection.Count, Is.EqualTo(1));
        Assert.That(collection.Single().Id, Is.EqualTo(id));
        Assert.That(collection.Get(collection.Single()).NonNullableStringProperty, Is.EqualTo(value));
    }

    [Test]
    public void InsertRelationTest()
    {
        using var repository = new SqliteRepository(":memory:");

        var entity1 = new FirstEntity(Guid.NewGuid());
        var entity2 = new SecondEntity(Guid.NewGuid());
        var parentCollection = new Collection<FirstEntity, FirstEntityProperties>(FakeModelShardInfo.FirstCollectionInfo, id => new(id), () => new());
        var childCollection = new Collection<SecondEntity, SecondEntityProperties>(FakeModelShardInfo.SecondCollectionInfo, id => new(id), () => new());
        parentCollection.Add(entity1, new());
        childCollection.Add(entity2, new());

        repository.Update(
            new RelationChangeSet<FirstEntity, SecondEntity>(FakeModelShardInfo.OneToOneRelationInfo)
            {
                { RelationAction.Linked, entity1, entity2 }
            });

        var relation = new Relation<FirstEntity, SecondEntity>(FakeModelShardInfo.OneToOneRelationInfo, new OneToOne<FirstEntity, SecondEntity>(), new OneToOne<SecondEntity, FirstEntity>());
        repository.Load(relation, parentCollection, childCollection);

        Assert.That(relation.Count, Is.EqualTo(1));
        Assert.That(relation.Children(entity1), Is.EquivalentTo(new[] { entity2 }));
        Assert.That(relation.Parents(entity2), Is.EquivalentTo(new[] { entity1 }));
    }

    [Test]
    public void UpdateCollectionTest()
    {
        using var repository = new SqliteRepository(":memory:");

        var value = "first entity";
        var value2 = "first entity 2";
        var id = Guid.NewGuid();

        repository.Update(
            new CollectionChangeSet<FirstEntity, FirstEntityProperties>(FakeModelShardInfo.FirstCollectionInfo)
            {
                { CollectionAction.Add, new(id), null, new FirstEntityProperties() { NonNullableStringProperty = value } }
            });

        repository.Update(
            new CollectionChangeSet<FirstEntity, FirstEntityProperties>(FakeModelShardInfo.FirstCollectionInfo)
            {
                { CollectionAction.Modify, new(id), new(), new FirstEntityProperties() { NonNullableStringProperty = value2 } }
            });

        var collection = new Collection<FirstEntity, FirstEntityProperties>(FakeModelShardInfo.FirstCollectionInfo, id => new(id), () => new());
        repository.Load(collection);

        Assert.That(collection.Count, Is.EqualTo(1));
        Assert.That(collection.Single().Id, Is.EqualTo(id));
        Assert.That(collection.Get(collection.Single()).NonNullableStringProperty, Is.EqualTo(value2));
    }

    [Test]
    public void DeleteCollectionTest()
    {
        using var repository = new SqliteRepository(":memory:");

        var value = "first entity";
        var id = Guid.NewGuid();

        repository.Update(
            new CollectionChangeSet<FirstEntity, FirstEntityProperties>(FakeModelShardInfo.FirstCollectionInfo)
            {
                { CollectionAction.Add, new FirstEntity(id), null, new FirstEntityProperties() { NonNullableStringProperty = value } }
            });

        repository.Update(
            new CollectionChangeSet<FirstEntity, FirstEntityProperties>(FakeModelShardInfo.FirstCollectionInfo)
            {
                { CollectionAction.Remove, new FirstEntity(id), new FirstEntityProperties() { NonNullableStringProperty = value }, null }
            });

        var collection = new Collection<FirstEntity, FirstEntityProperties>(FakeModelShardInfo.FirstCollectionInfo, id => new(id), () => new());
        repository.Load(collection);

        Assert.That(collection.Count, Is.EqualTo(0));
    }

    [Test]
    public void DeleteRelationTest()
    {
        using var repository = new SqliteRepository(":memory:");

        var entity1 = new FirstEntity(Guid.NewGuid());
        var entity2 = new SecondEntity(Guid.NewGuid());
        var parentCollection = new Collection<FirstEntity, FirstEntityProperties>(FakeModelShardInfo.FirstCollectionInfo, id => new(id), () => new());
        var childCollection = new Collection<SecondEntity, SecondEntityProperties>(FakeModelShardInfo.SecondCollectionInfo, id => new(id), () => new());
        parentCollection.Add(entity1, new());
        childCollection.Add(entity2, new());

        repository.Update(
            new RelationChangeSet<FirstEntity, SecondEntity>(FakeModelShardInfo.OneToOneRelationInfo)
            {
                { RelationAction.Linked, entity1, entity2 }
            });

        repository.Update(
            new RelationChangeSet<FirstEntity, SecondEntity>(FakeModelShardInfo.OneToOneRelationInfo)
            {
                { RelationAction.Unlinked, entity1, entity2 }
            });

        var relation = new Relation<FirstEntity, SecondEntity>(FakeModelShardInfo.OneToOneRelationInfo, new OneToOne<FirstEntity, SecondEntity>(), new OneToOne<SecondEntity, FirstEntity>());
        repository.Load(relation, parentCollection, childCollection);

        Assert.That(relation.Count, Is.EqualTo(0));
    }

    [Test]
    public void SelectFromNotExistingCollectionTest()
    {
        using var repository = new SqliteRepository(":memory:");

        var collection = new Collection<FirstEntity, FirstEntityProperties>(FakeModelShardInfo.FirstCollectionInfo, id => new(id), () => new());

        repository.Load(collection);

        Assert.That(collection.Count, Is.EqualTo(0));
    }

    [Test]
    public void SelectFromNotExistingRelationTest()
    {
        using var repository = new SqliteRepository(":memory:");

        var parentCollection = new Collection<FirstEntity, FirstEntityProperties>(FakeModelShardInfo.FirstCollectionInfo, id => new(id), () => new());
        var childCollection = new Collection<SecondEntity, SecondEntityProperties>(FakeModelShardInfo.SecondCollectionInfo, id => new(id), () => new());
        var relation = new Relation<FirstEntity, SecondEntity>(FakeModelShardInfo.OneToOneRelationInfo, new OneToOne<FirstEntity, SecondEntity>(), new OneToOne<SecondEntity, FirstEntity>());

        repository.Load(relation, parentCollection, childCollection);

        Assert.That(relation.Count, Is.EqualTo(0));
    }

    [Test]
    public void LoadCollectionDataToNonEmptyModelTest()
    {
        using var repository = new SqliteRepository(":memory:");

        var collection = new Collection<FirstEntity, FirstEntityProperties>(FakeModelShardInfo.FirstCollectionInfo, id => new(id), () => new())
        {
            new()
        };

        Assert.Throws<NonEmptyModelException>(() => collection.Load(repository));
    }

    [Test]
    public void LoadRelationDataToNonEmptyModelTest()
    {
        using var repository = new SqliteRepository(":memory:");

        var parentCollection = new Collection<FirstEntity, FirstEntityProperties>(FakeModelShardInfo.FirstCollectionInfo, id => new(id), () => new());
        var childCollection = new Collection<SecondEntity, SecondEntityProperties>(FakeModelShardInfo.SecondCollectionInfo, id => new(id), () => new());
        var relation = new Relation<FirstEntity, SecondEntity>(FakeModelShardInfo.OneToOneRelationInfo, new OneToOne<FirstEntity, SecondEntity>(), new OneToOne<SecondEntity, FirstEntity>())
        {
            { new(), new() }
        };

        Assert.Throws<NonEmptyModelException>(() => relation.Load(repository, parentCollection, childCollection));
    }

    [Test]
    public void RestoreHistoryTest()
    {
        using var repository = new SqliteRepository(":memory:");

        var entity = new FirstEntity(Guid.NewGuid());
        var properties = new FirstEntityProperties();
        var frame = new FakeChangesFrame();

        frame.FirstCollection.Add(CollectionAction.Add, entity, null, properties);

        repository.ExecuteNonQuery(QueryBuilder.History.CreateCollectionTable);
        repository.ExecuteNonQuery(QueryBuilder.History.CreateRelationTable);

        repository.Save(42, frame.FirstCollection);

        var changes = repository.RestoreHistory([new FakeModelShard()]).ToList();

        Assert.That(changes.Count, Is.EqualTo(1));
        var loadedFrame = changes.Single().OfType<FakeChangesFrame>().Single();
        Assert.That(loadedFrame.FirstCollection.Count(), Is.EqualTo(1));
        Assert.That(loadedFrame.FirstCollection.Single().Action, Is.EqualTo(CollectionAction.Add));
        Assert.That(loadedFrame.FirstCollection.Single().Entity, Is.EqualTo(entity));
        Assert.That(loadedFrame.FirstCollection.Single().OldData, Is.Null);
        Assert.That(loadedFrame.FirstCollection.Single().NewData, Is.EqualTo(properties));
    }

    [Test]
    public void ExistsForNonExistingTableShouldReturnFalseTest()
    {
        using var repository = new SqliteRepository(":memory:");

        Assert.That(repository.Exists(new CollectionInfo("non", "existing")), Is.False);
    }
}
