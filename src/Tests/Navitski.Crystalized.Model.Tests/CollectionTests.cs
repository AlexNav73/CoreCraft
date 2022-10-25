using Navitski.Crystalized.Model.Engine.ChangesTracking;
using Navitski.Crystalized.Model.Engine.Core;
using Navitski.Crystalized.Model.Engine.Exceptions;

namespace Navitski.Crystalized.Model.Tests;

public class CollectionTests
{
    private IMutableCollection<FirstEntity, FirstEntityProperties>? _collection;

    [SetUp]
    public void Setup()
    {
        _collection = new Collection<FirstEntity, FirstEntityProperties>(id => new FirstEntity(id), () => new FirstEntityProperties());
    }

    [Test]
    public void CreateEntityTest()
    {
        Assert.That(_collection!.Count, Is.EqualTo(0));

        var firstEntityId = Guid.NewGuid();
        var entity = _collection.Add(firstEntityId, p => p);

        Assert.That(_collection.Count, Is.EqualTo(1));
        Assert.That(_collection.Single(), Is.EqualTo(entity));
        Assert.That(entity.Id, Is.EqualTo(firstEntityId));
    }

    [Test]
    public void AddExistingEntityToCollectionTest()
    {
        Assert.That(_collection!.Count, Is.EqualTo(0));

        var entity = _collection.Add(new());

        Assert.That(_collection.Count, Is.EqualTo(1));
        Assert.That(_collection.Single(), Is.EqualTo(entity));
        Assert.Throws<DuplicateKeyException>(() =>
        {
            _collection.Add(entity.Id, p => p);
        });
    }

    [Test]
    public void RemoveEntitiyFromCollectionTest()
    {
        var entity = _collection!.Add(new());

        Assert.That(_collection.Count, Is.EqualTo(1));

        _collection.Remove(entity);

        Assert.That(_collection.Count, Is.EqualTo(0));
    }

    [Test]
    public void RemoveNotExistingEntityTest()
    {
        _collection!.Add(new());

        Assert.That(_collection.Count, Is.EqualTo(1));
        Assert.Throws<KeyNotFoundException>(() => _collection.Remove(new FirstEntity()));
    }

    [Test]
    public void RemoveFromEmptyCollectionTest()
    {
        Assert.That(_collection!.Count, Is.EqualTo(0));
        Assert.Throws<KeyNotFoundException>(() => _collection.Remove(new FirstEntity()));
    }

    [Test]
    public void GetPropertiesByEntityTest()
    {
        var value = "test";
        var entity = _collection!.Add(new() { NonNullableStringProperty = value });

        Assert.That(_collection.Count, Is.EqualTo(1));

        var properties = _collection.Get(entity);

        Assert.That(properties, Is.Not.Null);
        Assert.That(properties.NonNullableStringProperty, Is.EqualTo(value));
    }

    [Test]
    public void GetWithInvalidEntityTest()
    {
        var entity = _collection!.Add(new());

        Assert.That(_collection.Count, Is.EqualTo(1));

        var properties = _collection.Get(entity);

        Assert.That(properties, Is.Not.Null);
        Assert.Throws<KeyNotFoundException>(() => _collection.Get(new FirstEntity()));
    }

    [Test]
    public void GetFromEmptyCollectionTest()
    {
        Assert.That(_collection!.Count, Is.EqualTo(0));
        Assert.Throws<KeyNotFoundException>(() => _collection.Get(new FirstEntity()));
    }

    [Test]
    public void ContainsByEntityTest()
    {
        var entity = _collection!.Add(new());

        Assert.That(_collection!.Contains(entity), Is.True);
    }

    [Test]
    public void ContainsByInvalidEntityTest()
    {
        var entity = new FirstEntity();

        Assert.That(_collection!.Contains(entity), Is.False);
    }

    [Test]
    public void ModifyPropertiesTest()
    {
        var value = "test";
        var newValue = "new value";

        var entity = _collection!.Add(new() { NonNullableStringProperty = value });

        var properties = _collection.Get(entity);
        Assert.That(properties.NonNullableStringProperty, Is.EqualTo(value));

        _collection.Modify(entity, p => p with { NonNullableStringProperty = newValue });
        properties = _collection.Get(entity);

        Assert.That(properties, Is.Not.Null);
        Assert.That(properties.NonNullableStringProperty, Is.EqualTo(newValue));
    }

    [Test]
    public void ModifyNotExistingEntityTest()
    {
        Assert.That(_collection!.Count, Is.EqualTo(0));
        Assert.Throws<KeyNotFoundException>(() =>
            _collection.Modify(new FirstEntity(), p => p with { NullableStringProperty = "test" }));
    }

    [Test]
    public void CopyCollectionTest()
    {
        var value = "test";
        var entity = _collection!.Add(new() { NonNullableStringProperty = value });

        var copy = _collection.Copy();
        var copiedEntity = _collection.Single();

        Assert.That(ReferenceEquals(_collection, copy), Is.False);
        Assert.That(_collection.Count, Is.EqualTo(copy.Count));
        Assert.That(entity, Is.EqualTo(copiedEntity));

        var props = _collection.Get(entity);
        var copiedProps = copy.Get(copiedEntity);

        Assert.That(props, Is.EqualTo(copiedProps));
    }

    [Test]
    public void PairsCollectionTest()
    {
        var property = new FirstEntityProperties();
        var entity = _collection!.Add(property);

        var pairs = _collection.Pairs().ToArray();

        Assert.That(pairs.Length, Is.EqualTo(1));
        Assert.That(pairs[0].entity, Is.EqualTo(entity));
        Assert.That(pairs[0].properties, Is.EqualTo(property));
    }
}
