using Navitski.Crystalized.Model.Engine;
using Navitski.Crystalized.Model.Engine.ChangesTracking;
using Navitski.Crystalized.Model.Engine.Commands;
using Navitski.Crystalized.Model.Engine.Core;
using Navitski.Crystalized.Model.Engine.Exceptions;
using Navitski.Crystalized.Model.Engine.Persistence;
using Navitski.Crystalized.Model.Engine.Scheduling;
using Navitski.Crystalized.Model.Tests.Infrastructure.Commands;

namespace Navitski.Crystalized.Model.Tests;

public class DomainModelTests
{
    [Test]
    public void SubscribeWhenCurrentChangesAreNullTest()
    {
        var storage = A.Fake<IStorage>();
        var model = new TestDomainModel(Array.Empty<IModelShard>(), storage);
        Action<ModelChangedEventArgs> handler = args => { };

        var subscription = model.Subscribe(handler);

        Assert.That(subscription, Is.Not.Null);
    }

    [Test]
    public void SubscribeWhenHandlingChangesTest()
    {
        var storage = A.Fake<IStorage>();
        var model = new TestDomainModel(new[] { new FakeModelShard() }, storage);
        model.Subscribe(args =>
        {
            var subscriptionCalledImmidiately = false;
            Action<ModelChangedEventArgs> handler = args => subscriptionCalledImmidiately = true;

            var subscription = model.Subscribe(handler);

            Assert.That(subscription, Is.Not.Null);
            Assert.That(subscriptionCalledImmidiately, Is.True);
        });
        var command = CreateCommand(model);

        command.Execute();
    }

    [Test]
    public void SubscribeSameDelegateTwiceTest()
    {
        var storage = A.Fake<IStorage>();
        var model = new TestDomainModel(new[] { new FakeModelShard() }, storage);
        Action<ModelChangedEventArgs> handler = args => { };
        model.Subscribe(handler);

        Assert.Throws<SubscriptionAlreadyExistsException>(() => model.Subscribe(handler));
    }

    [Test]
    public void SaveChangesThrowsExceptionTest()
    {
        var storage = A.Fake<IStorage>();
        A.CallTo(() => storage.Migrate(A<string>.Ignored, A<IModel>.Ignored, A<IEnumerable<IModelChanges>>.Ignored))
            .Throws<InvalidOperationException>();
        var model = new TestDomainModel(new[] { new FakeModelShard() }, storage);
        
        Assert.ThrowsAsync<ModelSaveException>(() => model.Save("", new[] { A.Fake<IModelChanges>() }));
    }

    [Test]
    public void SaveAllThrowsExceptionTest()
    {
        var storage = A.Fake<IStorage>();
        A.CallTo(() => storage.Save(A<string>.Ignored, A<IModel>.Ignored))
            .Throws<InvalidOperationException>();
        var model = new TestDomainModel(new[] { new FakeModelShard() }, storage);

        Assert.ThrowsAsync<ModelSaveException>(() => model.Save(""));
    }

    private ModelCommand<TestDomainModel> CreateCommand(TestDomainModel model)
    {
        return new DelegateCommand<TestDomainModel>(model, m =>
        {
            var shard = m.Shard<IMutableFakeModelShard>();
            shard.FirstCollection.Add(new());
        });
    }

    private class TestDomainModel : DomainModel
    {
        private readonly IStorage _storage;

        public TestDomainModel(IEnumerable<IModelShard> shards, IStorage storage)
            : base(shards, new SyncScheduler())
        {
            _storage = storage;
        }

        public async Task Save(string path, IEnumerable<IModelChanges> changes)
        {
            await Save(_storage, path, changes);
        }

        public async Task Save(string path)
        {
            await Save(_storage, path);
        }
    }
}
