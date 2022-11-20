using Navitski.Crystalized.Model.Engine;
using Navitski.Crystalized.Model.Engine.ChangesTracking;
using Navitski.Crystalized.Model.Engine.Core;
using Navitski.Crystalized.Model.Engine.Exceptions;
using Navitski.Crystalized.Model.Engine.Persistence;
using Navitski.Crystalized.Model.Engine.Scheduling;
using Navitski.Crystalized.Model.Engine.Subscription;

namespace Navitski.Crystalized.Model.Tests;

public class DomainModelTests
{
    [Test]
    public void SubscribeWhenCurrentChangesAreNullTest()
    {
        var storage = A.Fake<IStorage>();
        var model = new TestDomainModel(Array.Empty<IModelShard>(), storage);
        Action<Change<IModelChanges>> handler = args => { };

        var subscription = model.Subscribe(handler);

        Assert.That(subscription, Is.Not.Null);
    }

    [Test]
    public void SubscribeToModelShardWhenCurrentChangesAreNullTest()
    {
        var storage = A.Fake<IStorage>();
        var model = new TestDomainModel(Array.Empty<IModelShard>(), storage);
        Action<Change<IFakeChangesFrame>> handler = args => { };

        var subscription = model.SubscribeTo<IFakeChangesFrame>(x => x.By(handler));

        Assert.That(subscription, Is.Not.Null);
    }

    [Test]
    public async Task SubscribeWhenHandlingChangesTest()
    {
        var storage = A.Fake<IStorage>();
        var model = new TestDomainModel(new[] { new FakeModelShard() }, storage);
        model.Subscribe(args =>
        {
            var subscriptionCalledImmidiately = false;
            Action<Change<IModelChanges>> handler = args => subscriptionCalledImmidiately = true;

            var subscription = model.Subscribe(handler);

            Assert.That(subscription, Is.Not.Null);
            Assert.That(subscriptionCalledImmidiately, Is.True);
        });

        await model.Run<IMutableFakeModelShard>((shard, _) => shard.FirstCollection.Add(new()));
    }

    [Test]
    public async Task SubscribeToModelShardWhenHandlingChangesTest()
    {
        var storage = A.Fake<IStorage>();
        var model = new TestDomainModel(new[] { new FakeModelShard() }, storage);
        model.SubscribeTo<IFakeChangesFrame>(x => x.By(args =>
        {
            var subscriptionCalledImmidiately = false;
            Action<Change<IFakeChangesFrame>> handler = args => subscriptionCalledImmidiately = true;

            var subscription = model.SubscribeTo<IFakeChangesFrame>(x => x.By(handler));

            Assert.That(subscription, Is.Not.Null);
            Assert.That(subscriptionCalledImmidiately, Is.True);
        }));

        await model.Run<IMutableFakeModelShard>((shard, _) => shard.FirstCollection.Add(new()));
    }

    [Test]
    public async Task ReceiveModelChangesAfterCommandExecutionTest()
    {
        var storage = A.Fake<IStorage>();
        var changesReceived = false;
        var model = new TestDomainModel(
            new[] { new FakeModelShard() },
            storage,
            m => changesReceived = true);

        await model.Run<IMutableFakeModelShard>((shard, _) => shard.FirstCollection.Add(new()));

        Assert.That(changesReceived, Is.True);
    }

    [Test]
    public void SubscribeSameDelegateTwiceTest()
    {
        var storage = A.Fake<IStorage>();
        var model = new TestDomainModel(new[] { new FakeModelShard() }, storage);
        Action<Change<IModelChanges>> handler = args => { };
        model.Subscribe(handler);

        Assert.Throws<SubscriptionAlreadyExistsException>(() => model.Subscribe(handler));
    }

    [Test]
    public void SaveChangesThrowsExceptionTest()
    {
        var storage = A.Fake<IStorage>();
        A.CallTo(() => storage.Update(A<string>.Ignored, A<IModel>.Ignored, A<IReadOnlyList<IModelChanges>>.Ignored))
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

    private class TestDomainModel : DomainModel
    {
        private readonly IStorage _storage;
        private readonly Action<Change<IModelChanges>>? _onModelChanged;

        public TestDomainModel(
            IEnumerable<IModelShard> shards,
            IStorage storage,
            Action<Change<IModelChanges>>? onModelChanged = null)
            : base(shards, new SyncScheduler())
        {
            _storage = storage;
            _onModelChanged = onModelChanged;
        }

        public async Task Save(string path, IReadOnlyList<IModelChanges> changes)
        {
            await Save(_storage, path, changes);
        }

        public async Task Save(string path)
        {
            await Save(_storage, path);
        }

        protected override void OnModelChanged(Change<IModelChanges> change)
        {
            _onModelChanged?.Invoke(change);
        }
    }
}
