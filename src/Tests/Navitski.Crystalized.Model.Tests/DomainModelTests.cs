using Navitski.Crystalized.Model.Engine;
using Navitski.Crystalized.Model.Engine.ChangesTracking;
using Navitski.Crystalized.Model.Engine.Commands;
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
    public void SubscribeSameDelegateTwiceTest()
    {
        var storage = A.Fake<IStorage>();
        var model = new TestDomainModel(new[] { new FakeModelShard() }, storage);
        Action<Change<IModelChanges>> handler = args => { };
        model.Subscribe(handler);

        Assert.Throws<SubscriptionAlreadyExistsException>(() => model.Subscribe(handler));
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
    public void RunThrowsWhenCommandExecutionFailsTest()
    {
        var storage = A.Fake<IStorage>();
        var model = new TestDomainModel(new[] { new FakeModelShard() }, storage);

        Assert.ThrowsAsync<CommandInvocationException>(() => model.Run<IMutableFakeModelShard>((shard, _) => throw new Exception("BOOM!")));
    }

    [Test]
    public void RunCommandThrowsWhenCommandExecutionFailsTest()
    {
        var storage = A.Fake<IStorage>();
        var model = new TestDomainModel(new[] { new FakeModelShard() }, storage);
        var command = A.Fake<ICommand>();

        A.CallTo(() => command.Execute(A<IModel>.Ignored, A<CancellationToken>.Ignored))
            .Throws<Exception>();

        Assert.ThrowsAsync<CommandInvocationException>(() => model.Run(command));
    }

    [Test]
    public void SaveChangesThrowsExceptionTest()
    {
        var storage = A.Fake<IStorage>();
        A.CallTo(() => storage.Update(A<string>.Ignored, A<IModel>.Ignored, A<IModelChanges>.Ignored))
            .Throws<InvalidOperationException>();
        var model = new TestDomainModel(new[] { new FakeModelShard() }, storage);
        
        Assert.ThrowsAsync<ModelSaveException>(() => model.Save("", new[] { A.Fake<IModelChanges>() }));
    }

    [Test]
    public void SaveChangesWithEmptyChangesCollectionShouldNotTriggerUpdateOnStorageTest()
    {
        var storage = A.Fake<IStorage>();
        var model = new TestDomainModel(new[] { new FakeModelShard() }, storage);

        var task = model.Save("", Array.Empty<IModelChanges>());

        A.CallTo(() => storage.Update(A<string>.Ignored, A<IModel>.Ignored, A<IModelChanges>.Ignored))
            .MustNotHaveHappened();
        Assert.That(task, Is.Not.Null);
        Assert.That(task.IsCompleted, Is.True);
    }

    [Test]
    public void SaveModelDoMergingOfModelChangesIntoOneChangeTest()
    {
        var modelChanges1 = A.Fake<IModelChanges>(c => c.Implements<IWritableModelChanges>());
        var modelChanges2 = A.Fake<IModelChanges>(c => c.Implements<IWritableModelChanges>());

        var storage = A.Fake<IStorage>();
        var model = new TestDomainModel(new[] { new FakeModelShard() }, storage);

        var task = model.Save("", new[] { modelChanges1, modelChanges2 });

        A.CallTo(() => ((IWritableModelChanges)modelChanges1).Merge(modelChanges2))
            .MustHaveHappenedOnceExactly();
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

    [Test]
    public void LoadThrowsWhenLoadExecutionFailsTest()
    {
        var storage = A.Fake<IStorage>();
        var model = new TestDomainModel(new[] { new FakeModelShard() }, storage);
        var notificationSent = false;

        model.Subscribe(x => notificationSent = true);

        A.CallTo(() => storage.Load(A<string>.Ignored, A<IModel>.Ignored))
            .Throws<Exception>();

        Assert.ThrowsAsync<ModelLoadingException>(() => model.Load("fake"));
        Assert.That(notificationSent, Is.False);
    }

    [Test]
    public void ApplyThrowsWhenApplyExecutionFailsTest()
    {
        var storage = A.Fake<IStorage>();
        var model = new TestDomainModel(new[] { new FakeModelShard() }, storage);
        var modelChanges = A.Fake<IWritableModelChanges>();
        var notificationSent = false;

        A.CallTo(() => modelChanges.HasChanges()).Returns(true);
        A.CallTo(() => modelChanges.Apply(A<IModel>.Ignored)).Throws<Exception>();

        model.Subscribe(x => notificationSent = true);

        Assert.ThrowsAsync<ApplyModelChangesException>(() => model.Apply(modelChanges));
        Assert.That(notificationSent, Is.False);
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

        public async Task Load(string path)
        {
            await Load(_storage, path);
        }

        public async Task Apply(IWritableModelChanges changes)
        {
            await Apply(changes, CancellationToken.None);
        }

        protected override void OnModelChanged(Change<IModelChanges> change)
        {
            _onModelChanged?.Invoke(change);
        }
    }
}
