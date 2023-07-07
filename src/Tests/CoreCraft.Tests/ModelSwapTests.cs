using CoreCraft.ChangesTracking;
using CoreCraft.Core;
using CoreCraft.Persistence;
using CoreCraft.Scheduling;

namespace CoreCraft.Tests;

public class ModelSwapTests
{
    [Test]
    public async Task ChangesHappenedAfterSaveStartedShouldNotInterfereWithSaveOperation()
    {
        var scheduler = new DelayedAsyncScheduler(2000, 0);
        var model = new TestDomainModel(scheduler);

        var task = model.Save(m =>
        {
            var shard = m.Shard<IFakeModelShard>();

            Assert.That(shard.FirstCollection.Count, Is.EqualTo(0));
        });

        await model.Run<IMutableFakeModelShard>((shard, _) =>
        {
            shard.FirstCollection.Add(new() { NonNullableStringProperty = "test" });
        });

        var shard = model.Shard<IFakeModelShard>();

        Assert.That(shard.FirstCollection.Count, Is.EqualTo(1));
        // when we are here - this means that command already changed the model
        // and save operation haven't started. So model is not accessed and
        // we will know if we will use new or old model only when Sleep will
        // be finished.

        await task; // should wait until save operation finish so assert in Save delegate will be called
    }
}

class TestDomainModel : DomainModel
{
    public TestDomainModel(DelayedAsyncScheduler scheduler)
        : base(new[] { new FakeModelShard() }, scheduler)
    {
    }

    public Task Save(Action<IModel> assert)
    {
        return Save(new TestStorage(assert), "test.txt");
    }
}

class TestStorage : IStorage
{
    private readonly Action<IModel> _assert;

    public TestStorage(Action<IModel> assert)
    {
        _assert = assert;
    }

    public void Update(string path, IModelChanges changes)
    {
        throw new NotImplementedException();
    }

    public void Save(string path, IModel model)
    {
        _assert(model);
    }

    public void Load(string path, IModel model)
    {
        throw new NotImplementedException();
    }
}

class DelayedAsyncScheduler : IScheduler
{
    private readonly int _delay;
    private readonly int _commandDelay;

    public DelayedAsyncScheduler(int delay, int commandDelay)
    {
        _delay = delay;
        _commandDelay = commandDelay;
    }

    public Task Enqueue(Action job, CancellationToken token)
    {
        return Task.Factory.StartNew(
            () =>
            {
                Thread.Sleep(_commandDelay);
                job();
            },
            token,
            TaskCreationOptions.DenyChildAttach,
            SequentialTaskScheduler.Instance);
    }

    public Task RunParallel(Action job, CancellationToken token)
    {
        return Task.Run(() =>
        {
            Thread.Sleep(_delay);
            job();
        }, token);
    }
}
