namespace Navitski.Crystalized.Model.Storage.Sqlite.Tests;

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
    public void InsertCollectionTest()
    {
        using var repository = new SqliteRepository(":memory:");

        var table = "test";
        var value = "first entity";
        var id = Guid.NewGuid();

        repository.Insert(
            table,
            new List<KeyValuePair<FirstEntity, FirstEntityProperties>>()
            {
                new KeyValuePair<FirstEntity, FirstEntityProperties>(new(id), new FirstEntityProperties() with { NonNullableStringProperty = value })
            },
            FakeModelShardStorage.FirstCollectionScheme);

        var collection = new Collection<FirstEntity, FirstEntityProperties>(id => new(id), () => new());
        repository.Select(table, collection, FakeModelShardStorage.FirstCollectionScheme);

        Assert.That(collection.Count, Is.EqualTo(1));
        Assert.That(collection.Single().Id, Is.EqualTo(id));
        Assert.That(collection.Get(collection.Single()).NonNullableStringProperty, Is.EqualTo(value));
    }

    [Test]
    public void InsertRelationTest()
    {
        using var repository = new SqliteRepository(":memory:");

        var table = "test";
        var entity1 = new FirstEntity(Guid.NewGuid());
        var entity2 = new SecondEntity(Guid.NewGuid());
        var parentCollection = new Collection<FirstEntity, FirstEntityProperties>(id => new(id), () => new());
        var childCollection = new Collection<SecondEntity, SecondEntityProperties>(id => new(id), () => new());
        parentCollection.Add(entity1, new());
        childCollection.Add(entity2, new());

        repository.Insert(
            table,
            new List<KeyValuePair<FirstEntity, SecondEntity>>()
            {
                new KeyValuePair<FirstEntity, SecondEntity>(entity1, entity2)
            });

        var relation = new Relation<FirstEntity, SecondEntity>(new OneToOne<FirstEntity, SecondEntity>(), new OneToOne<SecondEntity, FirstEntity>());
        repository.Select(table, relation, parentCollection, childCollection);

        Assert.That(relation.Count, Is.EqualTo(1));
        Assert.That(relation.Children(entity1), Is.EquivalentTo(new[] { entity2 }));
        Assert.That(relation.Parents(entity2), Is.EquivalentTo(new[] { entity1 }));
    }

    [Test]
    public void UpdateCollectionTest()
    {
        using var repository = new SqliteRepository(":memory:");

        var table = "test";
        var value = "first entity";
        var value2 = "first entity 2";
        var id = Guid.NewGuid();

        repository.Insert(
            table,
            new List<KeyValuePair<FirstEntity, FirstEntityProperties>>()
            {
                new KeyValuePair<FirstEntity, FirstEntityProperties>(new(id), new FirstEntityProperties() with { NonNullableStringProperty = value })
            },
            FakeModelShardStorage.FirstCollectionScheme);
        repository.Update(
            table,
            new List<KeyValuePair<FirstEntity, FirstEntityProperties>>()
            {
                new KeyValuePair<FirstEntity, FirstEntityProperties>(new(id), new FirstEntityProperties() with { NonNullableStringProperty = value2 })
            },
            FakeModelShardStorage.FirstCollectionScheme);

        var collection = new Collection<FirstEntity, FirstEntityProperties>(id => new(id), () => new());
        repository.Select(table, collection, FakeModelShardStorage.FirstCollectionScheme);

        Assert.That(collection.Count, Is.EqualTo(1));
        Assert.That(collection.Single().Id, Is.EqualTo(id));
        Assert.That(collection.Get(collection.Single()).NonNullableStringProperty, Is.EqualTo(value2));
    }

    [Test]
    public void DeleteCollectionTest()
    {
        using var repository = new SqliteRepository(":memory:");

        var table = "test";
        var value = "first entity";
        var id = Guid.NewGuid();

        repository.Insert(
            table,
            new List<KeyValuePair<FirstEntity, FirstEntityProperties>>()
            {
                new KeyValuePair<FirstEntity, FirstEntityProperties>(new(id), new FirstEntityProperties() with { NonNullableStringProperty = value })
            },
            FakeModelShardStorage.FirstCollectionScheme);
        repository.Delete(table, new[] { new FirstEntity(id) });

        var collection = new Collection<FirstEntity, FirstEntityProperties>(id => new(id), () => new());
        repository.Select(table, collection, FakeModelShardStorage.FirstCollectionScheme);

        Assert.That(collection.Count, Is.EqualTo(0));
    }

    [Test]
    public void DeleteRelationTest()
    {
        using var repository = new SqliteRepository(":memory:");

        var table = "test";
        var entity1 = new FirstEntity(Guid.NewGuid());
        var entity2 = new SecondEntity(Guid.NewGuid());
        var parentCollection = new Collection<FirstEntity, FirstEntityProperties>(id => new(id), () => new());
        var childCollection = new Collection<SecondEntity, SecondEntityProperties>(id => new(id), () => new());
        parentCollection.Add(entity1, new());
        childCollection.Add(entity2, new());

        repository.Insert(
            table,
            new List<KeyValuePair<FirstEntity, SecondEntity>>()
            {
                new KeyValuePair<FirstEntity, SecondEntity>(entity1, entity2)
            });
        repository.Delete(table, new List<KeyValuePair<FirstEntity, SecondEntity>>()
            {
                new KeyValuePair<FirstEntity, SecondEntity>(entity1, entity2)
            });

        var relation = new Relation<FirstEntity, SecondEntity>(new OneToOne<FirstEntity, SecondEntity>(), new OneToOne<SecondEntity, FirstEntity>());
        repository.Select(table, relation, parentCollection, childCollection);

        Assert.That(relation.Count, Is.EqualTo(0));
    }
}
