using ConsoleDemoApp.Model;
using ConsoleDemoApp.Model.Entities;
using CoreCraft;
using CoreCraft.Scheduling;
using CoreCraft.Storage.Json;
using CoreCraft.Storage.Linq2Db;
using CoreCraft.Storage.Linq2Db.Extensions;
using CoreCraft.Storage.Sqlite;
using CoreCraft.Subscription;
using CoreCraft.Subscription.Extensions;
using LinqToDB;
using LinqToDB.Async;
using LinqToDB.Data;

namespace ConsoleDemoApp;

static class Program
{
    private const string Path = "test.db";
    private const string History = "history.json";
    
    private const ConsoleColor SectionColor = ConsoleColor.DarkRed;
    private const ConsoleColor ChangesColor = ConsoleColor.Green;
    private const ConsoleColor SqlQueriesColor = ConsoleColor.DarkGray;

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

        DataConnection.TurnTraceSwitchOn();
        DataConnection.WriteTraceLine = (s1, s2, lvl) =>
        {
            ConsoleWriteLine(s1, SqlQueriesColor);
        };

        var options = new DataOptions()
            .UseSQLite(@$"DataSource={Path};")
            .UseMappingSchema(new ExampleMappingSchema());

        using var db = new DataConnection(options);

        var storage = new SqliteStorage(Path, [], Console.WriteLine);
        var historyStorage = new JsonStorage(History, new() { Formatting = Newtonsoft.Json.Formatting.Indented });
        var model = new UndoRedoDomainModel([new ExampleModelShard(db)], new SyncScheduler());

        db.AddInterceptor(new ExampleModelShardInterceptor(model));
        model.AddInterceptor(new TransactionInterceptor(db));

        using (model.For<IExampleChangesFrame>().Subscribe(OnExampleShardChanged))
        {
            ConsoleWriteLine("======================== Modifying ========================", SectionColor);

            await model.Run<IMutableExampleModelShard>(shard =>
            {
                var first = shard.FirstCollection.Add(new() { StringProperty = "test", IntegerProperty = 42 });
                var second = shard.SecondCollection.Add(new() { BoolProperty = true, DoubleProperty = 0.5, FloatProperty = 0.75f, IntProperty = (int)SecondEntityEnum.Second });

                shard.OneToOneRelation.Add(first, second);
            });

            await model.Run<IMutableExampleModelShard>(shard =>
            {
                var entity = shard.FirstCollection.Entities.First();

                shard.FirstCollection.Modify(entity, props => new()
                {
                    StringProperty = "modified 1",
                    IntegerProperty = "modified 1".GetHashCode()
                });
                shard.FirstCollection.Modify(entity, props => new()
                {
                    StringProperty = "modified 2",
                    IntegerProperty = "modified 2".GetHashCode()
                });
            });

            await model.Run<IMutableExampleModelShard>(shard =>
            {
                var entity = shard.SecondCollection.Entities.First();

                //var pairs = shard.FirstCollection
                //    .JoinWith(shard.OneToOneRelation, (props, pair) => new { props, pair.Child })
                //    .JoinWith(shard.SecondCollection, x => x.Child, (x, second) => new { First = x.props, Second = second })
                //    .FirstOrDefault();

                //shard.SecondCollection.Modify(pairs!.Second.EntityId, props => new() { IntProperty = (int)SecondEntityEnum.First });
 
                shard.SecondCollection.Modify(entity, props => new() { IntProperty = (int)SecondEntityEnum.Second });
            });

            await model.Run<IMutableExampleModelShard>(shard =>
            {
                var entity = shard.FirstCollection.Entities.First();

                shard.FirstCollection.Remove(entity);
            });
        }

        ConsoleWriteLine("======================== Saving ========================", SectionColor);
        await model.Save(storage);
        await model.History.Save(historyStorage);

        model = new UndoRedoDomainModel([new ExampleModelShard(db)], new SyncScheduler());
        using (model.For<IExampleChangesFrame>().Subscribe(OnExampleShardChanged))
        {
            ConsoleWriteLine("======================== Loading ========================", SectionColor);

            await model.History.Load(historyStorage);
            await model.Load(storage, force: true);

            ConsoleWriteLine("======================== Adding new change ========================", SectionColor);

            await model.Run<IMutableExampleModelShard>(shard =>
            {
                shard.FirstCollection.Add(new() { StringProperty = "modified after load history", IntegerProperty = 42 });
            });
            await model.History.Save(historyStorage);
            await model.History.Load(historyStorage);

            ConsoleWriteLine("======================== Undo after load ========================", SectionColor);

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
            ConsoleWriteLine($"Entity [{c.Entity}] has been {c.Action}ed.", ChangesColor);
            ConsoleWriteLine($"   Old data: {c.OldData}", ChangesColor);
            ConsoleWriteLine($"   New data: {c.NewData}", ChangesColor);
            Console.WriteLine();
        }

        foreach (var c in change.Hunk.SecondCollection)
        {
            ConsoleWriteLine($"Entity [{c.Entity}] has been {c.Action}ed.", ChangesColor);
            ConsoleWriteLine($"   Old data: {c.OldData}", ChangesColor);
            ConsoleWriteLine($"   New data: {c.NewData}", ChangesColor);
            Console.WriteLine();
        }

        foreach (var c in change.Hunk.OneToOneRelation)
        {
            ConsoleWriteLine($"Parent [{c.Parent}] and Child [{c.Child}] has been {c.Action}.", ChangesColor);
            Console.WriteLine();
        }
    }

    private static void ConsoleWriteLine(string? value, ConsoleColor color)
    {
        var oldColor = Console.ForegroundColor;
        Console.ForegroundColor = color;
        Console.WriteLine(value);
        Console.ForegroundColor = oldColor;
    }
}
