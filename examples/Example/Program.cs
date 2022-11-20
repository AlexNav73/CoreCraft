using Example.Model;
using Example.Model.Entities;
using Navitski.Crystalized.Model.Engine;
using Navitski.Crystalized.Model.Engine.ChangesTracking;
using Navitski.Crystalized.Model.Engine.Core;
using Navitski.Crystalized.Model.Engine.Persistence;
using Navitski.Crystalized.Model.Engine.Scheduling;
using Navitski.Crystalized.Model.Engine.Subscription;
using Navitski.Crystalized.Model.Storage.Sqlite;
using Navitski.Crystalized.Model.Storage.Sqlite.Migrations;

namespace Example;

class Program
{
    private const string Path = "test.db";

    public static async Task Main()
    {
        if (File.Exists(Path))
        {
            File.Delete(Path);
        }

        var model = new MyModel(new[] { new ExampleModelShard() });

        using (model.SubscribeTo<IExampleChangesFrame>(x => x.With(y => y.FirstCollection).By(OnFirstCollectionChanged)))
        {
            await model.Run<IMutableExampleModelShard>((shard, _) =>
            {
                var first = shard.FirstCollection.Add(new() { StringProperty = "test", IntegerProperty = 42 });
                var second = shard.SecondCollection.Add(new() { BoolProperty = true, DoubleProperty = 0.5 });

                shard.OneToOneRelation.Add(first, second);
            });

            var shard = model.Shard<IExampleModelShard>();

            await model.Run<IMutableExampleModelShard>((shard, _) =>
            {
                var entity = shard.FirstCollection.First();

                shard.FirstCollection.Modify(entity, props => props with { StringProperty = "modified 1" });
                shard.FirstCollection.Modify(entity, props => props with { IntegerProperty = "modified 2".GetHashCode() });
                shard.FirstCollection.Modify(entity, props => props with { StringProperty = "modified 3", IntegerProperty = "modified 3".GetHashCode() });
            });

            await model.Run<IMutableExampleModelShard>((shard, _) =>
            {
                var entity = shard.FirstCollection.Last();

                shard.FirstCollection.Remove(entity);
                shard.OneToOneRelation.Remove(entity, shard.OneToOneRelation.Children(entity).Single());
            });
        }

        model.Save(Path);

        model = new MyModel(new[] { new ExampleModelShard() });
        using (model.SubscribeTo<IExampleChangesFrame>(x => x.With(y => y.FirstCollection).By(OnFirstCollectionChanged)))
        {
            Console.WriteLine("======================== Loaded ========================");

            await model.Load(Path);
        }
    }

    private static void OnFirstCollectionChanged(Change<ICollectionChangeSet<FirstEntity, FirstEntityProperties>> change)
    {
        foreach (var c in change.Hunk)
        {
            Console.WriteLine($"Entity [{c.Entity}] has been {c.Action}ed.");
            Console.WriteLine($"   Old data: {c.OldData}");
            Console.WriteLine($"   New data: {c.NewData}");
            Console.WriteLine();
        }
    }
}

class MyModel : DomainModel
{
    private readonly IStorage _storage;
    private readonly List<IModelChanges> _changes;

    public MyModel(IEnumerable<IModelShard> shards)
        : base(shards, new SyncScheduler())
    {
        _storage = new SqliteStorage(
            Array.Empty<IMigration>(),
            new[] { new ExampleModelShardStorage() },
            new SqliteRepositoryFactory(),
            Console.WriteLine);
        _changes = new List<IModelChanges>();
    }

    public void Save(string path)
    {
        Save(_storage, path, _changes);
        _changes.Clear();
    }

    public async Task Load(string path)
    {
        await Load(_storage, path);
    }

    protected override void OnModelChanged(Change<IModelChanges> change)
    {
        _changes.Add(change.Hunk);
    }
}
