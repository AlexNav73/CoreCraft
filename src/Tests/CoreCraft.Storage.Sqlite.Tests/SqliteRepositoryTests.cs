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

        repository.Save(
            FakeModelShardInfo.FirstCollectionInfo,
            new CollectionChangeSet<FirstEntity, FirstEntityProperties>("")
            {
                { CollectionAction.Add, new(id), null, new FirstEntityProperties() { NonNullableStringProperty = "first entity" } }
            });

        Assert.That(query, Is.EqualTo("INSERT INTO [Fake.FirstCollection] ([Id], [NonNullableStringProperty], [NullableStringProperty], [NullableStringWithDefaultValueProperty]) VALUES (f4a0e880-b549-4e9d-a58c-716eab14e9f1, first entity, , );"));
    }

    [Test]
    public void InsertCollectionTest()
    {
        using var repository = new SqliteRepository(":memory:");

        var value = "first entity";
        var id = Guid.NewGuid();

        repository.Save(
            FakeModelShardInfo.FirstCollectionInfo,
            new CollectionChangeSet<FirstEntity, FirstEntityProperties>("")
            {
                { CollectionAction.Add, new(id), null, new FirstEntityProperties() { NonNullableStringProperty = value } }
            });

        var collection = new Collection<FirstEntity, FirstEntityProperties>("", id => new(id), () => new());
        repository.Load(FakeModelShardInfo.FirstCollectionInfo, collection);

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

        repository.Save(
            FakeModelShardInfo.OneToOneRelationInfo,
            new RelationChangeSet<FirstEntity, SecondEntity>("")
            {
                { RelationAction.Linked, entity1, entity2 }
            });

        var relation = new Relation<FirstEntity, SecondEntity>("", new OneToOne<FirstEntity, SecondEntity>(), new OneToOne<SecondEntity, FirstEntity>());
        repository.Load(FakeModelShardInfo.OneToOneRelationInfo, relation, parentCollection, childCollection);

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

        repository.Save(
            FakeModelShardInfo.FirstCollectionInfo,
            new CollectionChangeSet<FirstEntity, FirstEntityProperties>("")
            {
                { CollectionAction.Add, new(id), null, new FirstEntityProperties() { NonNullableStringProperty = value } },
                { CollectionAction.Modify, new(id), new(), new FirstEntityProperties() { NonNullableStringProperty = value2 } }
            });

        var collection = new Collection<FirstEntity, FirstEntityProperties>("", id => new(id), () => new());
        repository.Load(FakeModelShardInfo.FirstCollectionInfo, collection);

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

        repository.Save(
            FakeModelShardInfo.FirstCollectionInfo,
            new CollectionChangeSet<FirstEntity, FirstEntityProperties>("")
            {
                { CollectionAction.Add, new FirstEntity(id), null, new FirstEntityProperties() { NonNullableStringProperty = value } },
                { CollectionAction.Remove, new FirstEntity(id), new FirstEntityProperties() { NonNullableStringProperty = value }, null }
            });

        var collection = new Collection<FirstEntity, FirstEntityProperties>("", id => new(id), () => new());
        repository.Load(FakeModelShardInfo.FirstCollectionInfo, collection);

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

        repository.Save(
            FakeModelShardInfo.OneToOneRelationInfo,
            new RelationChangeSet<FirstEntity, SecondEntity>("")
            {
                { RelationAction.Linked, entity1, entity2 },
                { RelationAction.Unlinked, entity1, entity2 }
            });

        var relation = new Relation<FirstEntity, SecondEntity>("", new OneToOne<FirstEntity, SecondEntity>(), new OneToOne<SecondEntity, FirstEntity>());
        repository.Load(FakeModelShardInfo.OneToOneRelationInfo, relation, parentCollection, childCollection);

        Assert.That(relation.Count, Is.EqualTo(0));
    }

    [Test]
    public void SelectFromNotExistingCollectionTest()
    {
        using var repository = new SqliteRepository(":memory:");

        var collection = new Collection<FirstEntity, FirstEntityProperties>("", id => new(id), () => new());

        repository.Load(FakeModelShardInfo.FirstCollectionInfo, collection);

        Assert.That(collection.Count, Is.EqualTo(0));
    }

    [Test]
    public void SelectFromNotExistingRelationTest()
    {
        using var repository = new SqliteRepository(":memory:");

        var parentCollection = new Collection<FirstEntity, FirstEntityProperties>("", id => new(id), () => new());
        var childCollection = new Collection<SecondEntity, SecondEntityProperties>("", id => new(id), () => new());
        var relation = new Relation<FirstEntity, SecondEntity>("", new OneToOne<FirstEntity, SecondEntity>(), new OneToOne<SecondEntity, FirstEntity>());

        repository.Load(FakeModelShardInfo.OneToOneRelationInfo, relation, parentCollection, childCollection);

        Assert.That(relation.Count, Is.EqualTo(0));
    }

    [Test]
    public void LoadCollectionDataToNonEmptyModelTest()
    {
        using var repository = new SqliteRepository(":memory:");

        var collection = new Collection<FirstEntity, FirstEntityProperties>("", id => new(id), () => new())
        {
            new()
        };

        Assert.Throws<NonEmptyModelException>(() => repository.Load(FakeModelShardInfo.FirstCollectionInfo, collection));
    }



    [Test]
    public void LoadRelationDataToNonEmptyModelTest()
    {
        using var repository = new SqliteRepository(":memory:");

        var parentCollection = new Collection<FirstEntity, FirstEntityProperties>("", id => new(id), () => new());
        var childCollection = new Collection<SecondEntity, SecondEntityProperties>("", id => new(id), () => new());
        var relation = new Relation<FirstEntity, SecondEntity>("", new OneToOne<FirstEntity, SecondEntity>(), new OneToOne<SecondEntity, FirstEntity>())
        {
            { new(), new() }
        };

        Assert.Throws<NonEmptyModelException>(() => repository.Load(FakeModelShardInfo.OneToOneRelationInfo, relation, parentCollection, childCollection));
    }
}
