﻿using CoreCraft.ChangesTracking;
using CoreCraft.Commands;
using CoreCraft.Core;
using CoreCraft.Exceptions;
using CoreCraft.Persistence;
using CoreCraft.Persistence.Lazy;
using CoreCraft.Scheduling;
using CoreCraft.Subscription;

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

        A.CallTo(() => command.Execute(A<IMutableModel>.Ignored, A<CancellationToken>.Ignored))
            .Throws<Exception>();

        Assert.ThrowsAsync<CommandInvocationException>(() => model.Run(command));
    }

    [Test]
    public void SaveChangesThrowsExceptionTest()
    {
        var storage = A.Fake<IStorage>();
        A.CallTo(() => storage.Update(A<IEnumerable<IChangesFrame>>.Ignored))
            .Throws<InvalidOperationException>();
        var model = new TestDomainModel(new[] { new FakeModelShard() }, storage);

        Assert.ThrowsAsync<ModelSaveException>(() => model.Save([A.Fake<IModelChanges>()]));
    }

    [Test]
    public void SaveChangesWithEmptyChangesCollectionShouldNotTriggerUpdateOnStorageTest()
    {
        var storage = A.Fake<IStorage>();
        var model = new TestDomainModel(new[] { new FakeModelShard() }, storage);

        var task = model.Save([]);

        A.CallTo(() => storage.Update(A<IEnumerable<IChangesFrame>>.Ignored))
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

        var task = model.Save([modelChanges1, modelChanges2]);

        A.CallTo(() => ((IMutableModelChanges)modelChanges1).Merge(modelChanges2))
            .MustHaveHappenedOnceExactly();
    }

    [Test]
    public void SaveAllThrowsExceptionTest()
    {
        var storage = A.Fake<IStorage>();
        A.CallTo(() => storage.Save(A<IEnumerable<IModelShard>>.Ignored))
            .Throws<InvalidOperationException>();
        var model = new TestDomainModel(new[] { new FakeModelShard() }, storage);

        Assert.ThrowsAsync<ModelSaveException>(model.Save);
    }

    [Test]
    public void LoadThrowsWhenLoadExecutionFailsTest()
    {
        var storage = A.Fake<IStorage>();
        var model = new TestDomainModel(new[] { new FakeModelShard() }, storage);
        var notificationSent = false;

        model.Subscribe(c => notificationSent = true);

        A.CallTo(() => storage.Load(A<IEnumerable<IMutableModelShard>>.Ignored, A<bool>.Ignored))
            .Throws<Exception>();

        Assert.ThrowsAsync<ModelLoadingException>(() => model.Load());
        Assert.That(notificationSent, Is.False);
    }

    [Test]
    public async Task ModelShouldNotBeUpdatedAndNotificationsSentIfNothingIsLoaded()
    {
        var storage = A.Fake<IStorage>();
        var changesReceived = false;
        var model = new TestDomainModel(
            new[] { new FakeModelShard() },
            storage,
            m => changesReceived = true);

        A.CallTo(() => storage.Load(A<IEnumerable<IMutableModelShard>>.Ignored, A<bool>.Ignored))
            .Invokes(c =>
            {
                // In case when nothing was changed, copy of the model shard should not
                // become a part of the new model (it should be just discarded).
                // So, in case when nothing was changed ApplySnapshot should not be called
                // and model should not be changed
                var loadables = c.Arguments.Get<IEnumerable<IMutableModelShard>>(0)!;
                var shard = loadables.OfType<IMutableFakeModelShard>().Single();

                Assert.That(shard, Is.Not.Null);
            });

        using (model.Subscribe(c => changesReceived = true))
        {
            var before = model.Shard<IFakeModelShard>();
            await model.Load();
            var after = model.Shard<IFakeModelShard>();

            Assert.That(changesReceived, Is.False);
            Assert.That(ReferenceEquals(before, after), Is.True);
        }
    }

    [Test]
    public async Task LoadShouldNotThrowExceptionWhenCollectionLoadsMultipleTimesTest()
    {
        var storage = A.Fake<IStorage>();
        var repo = A.Fake<IRepository>();
        var model = new TestDomainModel(new[] { new FakeModelShard() }, storage);
        var entityId = Guid.NewGuid();

        A.CallTo(() => repo.Load(A<IMutableCollection<FirstEntity, FirstEntityProperties>>.Ignored))
            .Invokes(c =>
            {
                var collection = c.Arguments[0] as IMutableCollection<FirstEntity, FirstEntityProperties>;
                collection!.Add(entityId, p => p with { NonNullableStringProperty = "a" });
            });
        A.CallTo(() => storage.Load(A<IEnumerable<IMutableModelShard>>.Ignored, A<bool>.Ignored))
            .Invokes(c =>
            {
                var modelShards = c.Arguments[0] as IEnumerable<IMutableModelShard>;

                foreach (var shard in modelShards!)
                {
                    shard.Load(repo);
                }
            });
        A.CallTo(() => storage.Load(A<ILazyLoader>.Ignored))
            .Invokes(c =>
            {
                var loader = c.Arguments[0] as ILazyLoader;
                loader!.Load(repo);
            });

        await model.Load();

        Assert.DoesNotThrowAsync(() => model.Load<IMutableFakeModelShard>(x => x.Collection(y => y.FirstCollection)));
    }

    [Test]
    public async Task LoadShouldNotThrowExceptionWhenRelationLoadsMultipleTimesTest()
    {
        var storage = A.Fake<IStorage>();
        var repo = A.Fake<IRepository>();
        var model = new TestDomainModel(new[] { new FakeModelShard() }, storage);
        var entityId1 = new FirstEntity();
        var entityId2 = new SecondEntity();

        A.CallTo(() => repo.Load(A<IMutableCollection<FirstEntity, FirstEntityProperties>>.Ignored))
            .Invokes(c =>
            {
                var collection = c.Arguments[0] as IMutableCollection<FirstEntity, FirstEntityProperties>;
                collection!.Add(entityId1, new() { NonNullableStringProperty = "a" });
            });
        A.CallTo(() => repo.Load(A<IMutableCollection<SecondEntity, SecondEntityProperties>>.Ignored))
            .Invokes(c =>
            {
                var collection = c.Arguments[0] as IMutableCollection<SecondEntity, SecondEntityProperties>;
                collection!.Add(entityId2, new() { IntProperty = 42 });
            });
        A.CallTo(() => repo.Load(A<IMutableRelation<FirstEntity, SecondEntity>>.Ignored, A<IEnumerable<FirstEntity>>.Ignored, A<IEnumerable<SecondEntity>>.Ignored))
            .Invokes(c =>
            {
                var relation = c.Arguments[0] as IMutableRelation<FirstEntity, SecondEntity>;
                relation!.Add(entityId1, entityId2);
            });
        A.CallTo(() => storage.Load(A<IEnumerable<IMutableModelShard>>.Ignored, A<bool>.Ignored))
            .Invokes(c =>
            {
                var modelShards = c.Arguments[0] as IEnumerable<IMutableModelShard>;

                foreach (var shard in modelShards!)
                {
                    shard.Load(repo);
                }
            });
        A.CallTo(() => storage.Load(A<ILazyLoader>.Ignored))
            .Invokes(c =>
            {
                var loader = c.Arguments[0] as ILazyLoader;
                loader!.Load(repo);
            });

        await model.Load();

        Assert.DoesNotThrowAsync(() => model.Load<IMutableFakeModelShard>(x => x
            .Relation(y => y.OneToOneRelation, y => y.FirstCollection, y => y.SecondCollection)));
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

        public async Task Save(IReadOnlyList<IModelChanges> changes)
        {
            await Save(_storage, changes);
        }

        public async Task Save()
        {
            await Save(_storage);
        }

        public async Task Load()
        {
            await Load(_storage);
        }

        public async Task Load<T>(Func<IModelShardLoader<T>, ILazyLoader> configure)
            where T : IMutableModelShard
        {
            await Load(_storage, configure);
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
