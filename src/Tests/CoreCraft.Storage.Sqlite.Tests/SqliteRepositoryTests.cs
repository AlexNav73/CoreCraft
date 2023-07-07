using CoreCraft.ChangesTracking;

namespace CoreCraft.Storage.Sqlite.Tests;

public class SqliteRepositoryTests
{
    [Test]
    public void SetDatabaseVersionTest()
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

        repository.Insert(
            FakeModelShardStorage.FirstCollectionInfo,
            new List<KeyValuePair<FirstEntity, FirstEntityProperties>>()
            {
                new KeyValuePair<FirstEntity, FirstEntityProperties>(new(id), new FirstEntityProperties() with { NonNullableStringProperty = "first entity" })
            });

        Assert.That(query, Is.EqualTo("INSERT INTO [Fake.FirstCollection] ([Id], [NonNullableStringProperty], [NullableStringProperty], [NullableStringWithDefaultValueProperty]) VALUES (f4a0e880-b549-4e9d-a58c-716eab14e9f1, first entity, , );"));
    }

    [Test]
    public void InsertCollectionTest()
    {
        using var repository = new SqliteRepository(":memory:");

        var value = "first entity";
        var id = Guid.NewGuid();

        repository.Insert(
            FakeModelShardStorage.FirstCollectionInfo,
            new List<KeyValuePair<FirstEntity, FirstEntityProperties>>()
            {
                new KeyValuePair<FirstEntity, FirstEntityProperties>(new(id), new FirstEntityProperties() with { NonNullableStringProperty = value })
            });

        var collection = new Collection<FirstEntity, FirstEntityProperties>("", id => new(id), () => new());
        repository.Select(FakeModelShardStorage.FirstCollectionInfo, collection);

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
        var parentCollection = new Collection<FirstEntity, FirstEntityProperties>("", id => new(id), () => new());
        var childCollection = new Collection<SecondEntity, SecondEntityProperties>("", id => new(id), () => new());
        parentCollection.Add(entity1, new());
        childCollection.Add(entity2, new());

        repository.Insert(
            FakeModelShardStorage.OneToOneRelationInfo,
            new List<KeyValuePair<FirstEntity, SecondEntity>>()
            {
                new KeyValuePair<FirstEntity, SecondEntity>(entity1, entity2)
            });

        var relation = new Relation<FirstEntity, SecondEntity>("", new OneToOne<FirstEntity, SecondEntity>(), new OneToOne<SecondEntity, FirstEntity>());
        repository.Select(FakeModelShardStorage.OneToOneRelationInfo, relation, parentCollection, childCollection);

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

        repository.Insert(
            FakeModelShardStorage.FirstCollectionInfo,
            new List<KeyValuePair<FirstEntity, FirstEntityProperties>>()
            {
                new KeyValuePair<FirstEntity, FirstEntityProperties>(new(id), new FirstEntityProperties() with { NonNullableStringProperty = value })
            });
        repository.Update(
            FakeModelShardStorage.FirstCollectionInfo,
            new List<ICollectionChange<FirstEntity, FirstEntityProperties>>()
            {
                new CollectionChange<FirstEntity, FirstEntityProperties>(CollectionAction.Modify, new(id), new(), new FirstEntityProperties() with { NonNullableStringProperty = value2 })
            });

        var collection = new Collection<FirstEntity, FirstEntityProperties>("", id => new(id), () => new());
        repository.Select(FakeModelShardStorage.FirstCollectionInfo, collection);

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

        repository.Insert(
            FakeModelShardStorage.FirstCollectionInfo,
            new List<KeyValuePair<FirstEntity, FirstEntityProperties>>()
            {
                new KeyValuePair<FirstEntity, FirstEntityProperties>(new(id), new FirstEntityProperties() with { NonNullableStringProperty = value })
            });
        repository.Delete(FakeModelShardStorage.FirstCollectionInfo, new[] { new FirstEntity(id) });

        var collection = new Collection<FirstEntity, FirstEntityProperties>("", id => new(id), () => new());
        repository.Select(FakeModelShardStorage.FirstCollectionInfo, collection);

        Assert.That(collection.Count, Is.EqualTo(0));
    }

    [Test]
    public void DeleteRelationTest()
    {
        using var repository = new SqliteRepository(":memory:");

        var entity1 = new FirstEntity(Guid.NewGuid());
        var entity2 = new SecondEntity(Guid.NewGuid());
        var parentCollection = new Collection<FirstEntity, FirstEntityProperties>("", id => new(id), () => new());
        var childCollection = new Collection<SecondEntity, SecondEntityProperties>("", id => new(id), () => new());
        parentCollection.Add(entity1, new());
        childCollection.Add(entity2, new());

        repository.Insert(
            FakeModelShardStorage.OneToOneRelationInfo,
            new List<KeyValuePair<FirstEntity, SecondEntity>>()
            {
                new KeyValuePair<FirstEntity, SecondEntity>(entity1, entity2)
            });
        repository.Delete(
            FakeModelShardStorage.OneToOneRelationInfo,
            new List<KeyValuePair<FirstEntity, SecondEntity>>()
            {
                new KeyValuePair<FirstEntity, SecondEntity>(entity1, entity2)
            });

        var relation = new Relation<FirstEntity, SecondEntity>("", new OneToOne<FirstEntity, SecondEntity>(), new OneToOne<SecondEntity, FirstEntity>());
        repository.Select(FakeModelShardStorage.OneToOneRelationInfo, relation, parentCollection, childCollection);

        Assert.That(relation.Count, Is.EqualTo(0));
    }

    [Test]
    public void SelectFromNotExistingCollectionTest()
    {
        using var repository = new SqliteRepository(":memory:");

        var collection = new Collection<FirstEntity, FirstEntityProperties>("", id => new(id), () => new());

        repository.Select(FakeModelShardStorage.FirstCollectionInfo, collection);

        Assert.That(collection.Count, Is.EqualTo(0));
    }

    [Test]
    public void SelectFromNotExistingRelationTest()
    {
        using var repository = new SqliteRepository(":memory:");

        var parentCollection = new Collection<FirstEntity, FirstEntityProperties>("", id => new(id), () => new());
        var childCollection = new Collection<SecondEntity, SecondEntityProperties>("", id => new(id), () => new());
        var relation = new Relation<FirstEntity, SecondEntity>("", new OneToOne<FirstEntity, SecondEntity>(), new OneToOne<SecondEntity, FirstEntity>());

        repository.Select(FakeModelShardStorage.OneToOneRelationInfo, relation, parentCollection, childCollection);

        Assert.That(relation.Count, Is.EqualTo(0));
    }
}
