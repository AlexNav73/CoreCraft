using ConsoleDemoApp.Model;
using ConsoleDemoApp.Model.Entities;
using CoreCraft;
using CoreCraft.Scheduling;
using CoreCraft.Storage.Json;
using CoreCraft.Storage.Sqlite;
using CoreCraft.Subscription;
using CoreCraft.Subscription.Extensions;

namespace ConsoleDemoApp;

static class Program
{
    private const string Path = "test.db";
    private const string History = "history.json";

    public static async Task Main()
    {
        if (File.Exists(Path))
        {
            File.Delete(Path);
        }
        if (File.Exists(History))
        {
            File.Delete(History);
        }

        var storage = new SqliteStorage(Path, [], Console.WriteLine);
        var historyStorage = new JsonStorage(History, new() { Formatting = Newtonsoft.Json.Formatting.Indented });
        var model = new UndoRedoDomainModel(new[] { new ExampleModelShard() }, new SyncScheduler());

        using (model.For<IExampleChangesFrame>().Subscribe(OnExampleShardChanged))
        {
            Console.WriteLine("======================== Modifying ========================");

            await model.Run<IMutableExampleModelShard>((shard, _) =>
            {
                var first = shard.FirstCollection.Add(new() { StringProperty = "test", IntegerProperty = 42 });
                var second = shard.SecondCollection.Add(new() { BoolProperty = true, DoubleProperty = 0.5, EnumProperty = SecondEntityEnum.First });

                shard.OneToOneRelation.Add(first, second);
            });

            await model.Run<IMutableExampleModelShard>((shard, _) =>
            {
                var entity = shard.FirstCollection.First();

                shard.FirstCollection.Modify(entity, props => props with { StringProperty = "modified 1" });
                shard.FirstCollection.Modify(entity, props => props with { IntegerProperty = "modified 2".GetHashCode() });
                shard.FirstCollection.Modify(entity, props => props with { StringProperty = "modified 3", IntegerProperty = "modified 3".GetHashCode() });
            });

            await model.Run<IMutableExampleModelShard>((shard, _) =>
            {
                var entity = shard.SecondCollection.First();

                shard.SecondCollection.Modify(entity, props => props with { EnumProperty = SecondEntityEnum.Second });
            });

            await model.Run<IMutableExampleModelShard>((shard, _) =>
            {
                var entity = shard.FirstCollection.Last();

                shard.FirstCollection.Remove(entity);
                shard.OneToOneRelation.Remove(entity);
            });
        }

        Console.WriteLine("======================== Saving ========================");
        await model.Save(storage);
        await model.History.Save(historyStorage);

        model = new UndoRedoDomainModel(new[] { new ExampleModelShard() }, new SyncScheduler());
        using (model.For<IExampleChangesFrame>().Subscribe(OnExampleShardChanged))
        {
            Console.WriteLine("======================== Loading ========================");

            await model.History.Load(historyStorage);
            await model.Load(storage, force: true);

            Console.WriteLine("======================== Adding new change ========================");

            await model.Run<IMutableExampleModelShard>((shard, _) =>
            {
                shard.FirstCollection.Add(new() { StringProperty = "test", IntegerProperty = 42 });
            });
            await model.History.Save(historyStorage);
            await model.History.Load(historyStorage);

            Console.WriteLine("======================== Undo after load ========================");

            var historySize = model.History.UndoStack.Count;
            for (int i = 0; i < historySize; i++)
            {
                await model.History.Undo();
            }
        }
    }

    private static void OnExampleShardChanged(Change<IExampleChangesFrame> change)
    {
        foreach (var c in change.Hunk.FirstCollection)
        {
            Console.WriteLine($"Entity [{c.Entity}] has been {c.Action}ed.");
            Console.WriteLine($"   Old data: {c.OldData}");
            Console.WriteLine($"   New data: {c.NewData}");
            Console.WriteLine();
        }

        foreach (var c in change.Hunk.SecondCollection)
        {
            Console.WriteLine($"Entity [{c.Entity}] has been {c.Action}ed.");
            Console.WriteLine($"   Old data: {c.OldData}");
            Console.WriteLine($"   New data: {c.NewData}");
            Console.WriteLine();
        }

        foreach (var c in change.Hunk.OneToOneRelation)
        {
            Console.WriteLine($"Parent [{c.Parent}] and Child [{c.Child}] has been {c.Action}.");
            Console.WriteLine();
        }
    }
}
