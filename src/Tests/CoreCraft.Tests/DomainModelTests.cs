using CoreCraft;
using CoreCraft.ChangesTracking;
using CoreCraft.Commands;
using CoreCraft.Core;
using CoreCraft.Exceptions;
using CoreCraft.Persistence;
using CoreCraft.Scheduling;
using CoreCraft.Subscription;
using CoreCraft.Subscription.Builders;

namespace CoreCraft.Tests;

public class DomainModelTests
{
    [Test]
    public void CreatingDomainModelWithDefaultSchedulerDoesNotThrowsExceptionsTest()
    {
        Assert.DoesNotThrow(() => new DomainModel(Array.Empty<IModelShard>()));
    }

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

        var subscription = model.For<IFakeChangesFrame>().Subscribe(handler);

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
        model.For<IFakeChangesFrame>()
            .Subscribe(args =>
            {
                var subscriptionCalledImmidiately = false;
                Action<Change<IFakeChangesFrame>> handler = args => subscriptionCalledImmidiately = true;

                var subscription = model.For<IFakeChangesFrame>().Subscribe(handler);

                Assert.That(subscription, Is.Not.Null);
                Assert.That(subscriptionCalledImmidiately, Is.True);
            });

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
    public async Task ModelShouldNotBeUpdatedAndNotificationsSentIfNothingChanged()
    {
        var storage = A.Fake<IStorage>();
        var changesReceived = false;
        var model = new TestDomainModel(
            new[] { new FakeModelShard() },
            storage,
            m => changesReceived = true);

        model.Subscribe(c => changesReceived = true);

        var before = model.Shard<IFakeModelShard>();
        await model.Run<IMutableFakeModelShard>((shard, _) => { });
        var after = model.Shard<IFakeModelShard>();

        Assert.That(changesReceived, Is.False);
        Assert.That(ReferenceEquals(before, after), Is.True);
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
        A.CallTo(() => storage.Update(A<string>.Ignored, A<IModelChanges>.Ignored))
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

        A.CallTo(() => storage.Update(A<string>.Ignored, A<IModelChanges>.Ignored))
            .MustNotHaveHappened();
        Assert.That(task, Is.Not.Null);
        Assert.That(task.IsCompleted, Is.True);
    }

    [Test]
    public void SaveModelDoMergingOfModelChangesIntoOneChangeTest()
    {
        var modelChanges1 = A.Fake<IModelChanges>(c => c.Implements<IMutableModelChanges>());
        var modelChanges2 = A.Fake<IModelChanges>(c => c.Implements<IMutableModelChanges>());

        var storage = A.Fake<IStorage>();
        var model = new TestDomainModel(new[] { new FakeModelShard() }, storage);

        var task = model.Save("", new[] { modelChanges1, modelChanges2 });

        A.CallTo(() => ((IMutableModelChanges)modelChanges1).Merge(modelChanges2))
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

        model.Subscribe(c => notificationSent = true);

        A.CallTo(() => storage.Load(A<string>.Ignored, A<IModel>.Ignored))
            .Throws<Exception>();

        Assert.ThrowsAsync<ModelLoadingException>(() => model.Load("fake"));
        Assert.That(notificationSent, Is.False);
    }

    [Test]
    public async Task ModelShouldNotBeUpdatedAndNotificationsSentIfNothingLoaded()
    {
        var storage = A.Fake<IStorage>();
        var changesReceived = false;
        var model = new TestDomainModel(
            new[] { new FakeModelShard() },
            storage,
            m => changesReceived = true);

        A.CallTo(() => storage.Load(A<string>.Ignored, A<IModel>.Ignored))
            .Invokes(c =>
            {
                // Acquiring mutable shard here we force model to make a shard's copy.
                // This is needed for shard's reference check. In case when nothing was
                // changed, copy of the model shard should not become a part of the new
                // model (it should be just discarded). So, in case when nothing was changed
                // ApplySnapshot should not be called and model should not be changed
                var shard = ((IModel)c.Arguments[1]!).Shard<IMutableFakeModelShard>();

                Assert.That(shard, Is.Not.Null);
            });

        model.Subscribe(c => changesReceived = true);

        var before = model.Shard<IFakeModelShard>();
        await model.Load("fake");
        var after = model.Shard<IFakeModelShard>();

        Assert.That(changesReceived, Is.False);
        Assert.That(ReferenceEquals(before, after), Is.True);
    }

    [Test]
    public void ApplyThrowsWhenApplyExecutionFailsTest()
    {
        var storage = A.Fake<IStorage>();
        var model = new TestDomainModel(new[] { new FakeModelShard() }, storage);
        var modelChanges = A.Fake<IMutableModelChanges>();
        var notificationSent = false;

        A.CallTo(() => modelChanges.HasChanges()).Returns(true);
        A.CallTo(() => modelChanges.Apply(A<IModel>.Ignored)).Throws<Exception>();

        model.Subscribe(x => notificationSent = true);

        Assert.ThrowsAsync<ApplyModelChangesException>(() => model.Apply(modelChanges));
        Assert.That(notificationSent, Is.False);
    }

    [Test]
    public async Task ChangesToTheOneCollectionShouldNotCauseCopyingOfOtherCollections()
    {
        var storage = A.Fake<IStorage>();
        var model = new TestDomainModel(new[] { new FakeModelShard() }, storage);
        var originalShard = model.Shard<IFakeModelShard>();

        await model.Run<IMutableFakeModelShard>((shard, _) =>
        {
            shard.FirstCollection.Add(new());
        });

        var changedShard = model.Shard<IFakeModelShard>();

        Assert.That(ReferenceEquals(originalShard, changedShard), Is.False);
        Assert.That(ReferenceEquals(originalShard.FirstCollection, changedShard.FirstCollection), Is.False);
        Assert.That(ReferenceEquals(originalShard.SecondCollection, changedShard.SecondCollection), Is.True);
    }

    [Test]
    public async Task ChangesToTheOneRelationShouldNotCauseCopyingOfOtherRelations()
    {
        var storage = A.Fake<IStorage>();
        var model = new TestDomainModel(new[] { new FakeModelShard() }, storage);
        var originalShard = model.Shard<IFakeModelShard>();

        await model.Run<IMutableFakeModelShard>((shard, _) =>
        {
            var parent = shard.FirstCollection.Add(new());
            var child = shard.SecondCollection.Add(new());

            shard.OneToOneRelation.Add(parent, child);
        });

        var changedShard = model.Shard<IFakeModelShard>();

        Assert.That(ReferenceEquals(originalShard, changedShard), Is.False);
        Assert.That(ReferenceEquals(originalShard.OneToOneRelation, changedShard.OneToOneRelation), Is.False);
        Assert.That(ReferenceEquals(originalShard.ManyToManyRelation, changedShard.ManyToManyRelation), Is.True);
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

        public async Task Apply(IMutableModelChanges changes)
        {
            await Apply(changes, CancellationToken.None);
        }

        protected override void OnModelChanged(Change<IModelChanges> change)
        {
            _onModelChanged?.Invoke(change);
        }
    }
}
