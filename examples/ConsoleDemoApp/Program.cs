using ConsoleDemoApp.Model;
using ConsoleDemoApp.Model.Entities;
using CoreCraft;
using CoreCraft.Scheduling;
using CoreCraft.Storage.Json;
using CoreCraft.Storage.Sqlite;
using CoreCraft.Storage.Linq2Db.Extensions;
using CoreCraft.Subscription;
using CoreCraft.Subscription.Extensions;
using LinqToDB;
using LinqToDB.Data;
using LinqToDB.Mapping;

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

        DataConnection.TurnTraceSwitchOn();
        DataConnection.WriteTraceLine = (s1, s2, lvl) =>
        {
            Console.WriteLine(s1);
        };

        var options = new DataOptions()
            .UseSQLite(@$"DataSource={Path};")
            .UseMappingSchema(new MyCustomMappingSchema());
        using var db = new DataConnection(options);

        await db.CreateTableAsync<FirstEntityProperties>();
        await db.CreateTableAsync<SecondEntityProperties>();

        var storage = new SqliteStorage(Path, [], Console.WriteLine);
        var historyStorage = new JsonStorage(History, new() { Formatting = Newtonsoft.Json.Formatting.Indented });
        var model = new UndoRedoDomainModel([new ExampleModelShard(db)], new SyncScheduler());

        using (model.For<IExampleChangesFrame>().Subscribe(OnExampleShardChanged))
        {
            Console.WriteLine("======================== Modifying ========================");

            await model.Run<IMutableExampleModelShard>(async (shard, _) =>
            {
                var first = await shard.FirstCollection.AddAsync(new() { StringProperty = "test", IntegerProperty = 42 });
                var second = await shard.SecondCollection.AddAsync(new() { BoolProperty = true, DoubleProperty = 0.5, FloatProperty = 0.75f, IntProperty = (int)SecondEntityEnum.Second });

                //var first = shard.FirstCollection.Add(new() { StringProperty = "test", IntegerProperty = 42 });
                //var second = shard.SecondCollection.Add(new() { BoolProperty = true, DoubleProperty = 0.5, FloatProperty = 0.75f, IntProperty = (int)SecondEntityEnum.Second });

                //shard.OneToOneRelation.Add(first, second);
            });

            await model.Run<IMutableExampleModelShard>(async (shard, _) =>
            {
                var entity = await shard.FirstCollection.Entities.FirstAsync();
                //var entity = shard.FirstCollection.First();

                await shard.FirstCollection.ModifyAsync(entity, props => props.StringProperty, "modified 1");
                await shard.FirstCollection.ModifyAsync(entity, props => props.IntegerProperty, "modified 2".GetHashCode());
                await shard.FirstCollection.ModifyAsync(entity, props => props.StringProperty, "modified 3");
                await shard.FirstCollection.ModifyAsync(entity, props => props.IntegerProperty, "modified 3".GetHashCode());

                //shard.FirstCollection.Modify(entity, props => props with { StringProperty = "modified 1" });
                //shard.FirstCollection.Modify(entity, props => props with { IntegerProperty = "modified 2".GetHashCode() });
                //shard.FirstCollection.Modify(entity, props => props with { StringProperty = "modified 3" });
                //shard.FirstCollection.Modify(entity, props => props with { IntegerProperty = "modified 3".GetHashCode() });
            });

            await model.Run<IMutableExampleModelShard>(async (shard, _) =>
            {
                var entity = await shard.SecondCollection.Entities.FirstAsync();
                //var entity = shard.SecondCollection.First();

                await shard.SecondCollection.ModifyAsync(entity, props => props.IntProperty, (int)SecondEntityEnum.Second);
                //shard.SecondCollection.Modify(entity, props => props with { IntProperty = (int)SecondEntityEnum.Second });
            });

            await model.Run<IMutableExampleModelShard>(async (shard, _) =>
            {
                var entity = await shard.FirstCollection.Entities.FirstAsync();
                //var entity = shard.FirstCollection.First();

                await shard.FirstCollection.RemoveAsync(entity);
                //shard.OneToOneRelation.Remove(entity);
            });
        }

        Console.WriteLine("======================== Saving ========================");
        await model.Save(storage);
        await model.History.Save(historyStorage);

        model = new UndoRedoDomainModel([new ExampleModelShard(db)], new SyncScheduler());
        using (model.For<IExampleChangesFrame>().Subscribe(OnExampleShardChanged))
        {
            Console.WriteLine("======================== Loading ========================");

            await model.History.Load(historyStorage);
            await model.Load(storage, force: true);

            Console.WriteLine("======================== Adding new change ========================");

            await model.Run<IMutableExampleModelShard>(async (shard, _) =>
            {
                //shard.FirstCollection.Add(new() { StringProperty = "test", IntegerProperty = 42 });
                await shard.FirstCollection.AddAsync(new() { StringProperty = "test", IntegerProperty = 42 });
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

/// <summary>
/// 
/// </summary>
public class MyCustomMappingSchema : MappingSchema
{
    /// <summary>
    /// 
    /// </summary>
    public MyCustomMappingSchema() : base(nameof(MyCustomMappingSchema))
    {
        var builder = new FluentMappingBuilder(this);
        ConfigureMappings(builder);
        builder.Build();
    }

    private static void ConfigureMappings(FluentMappingBuilder builder)
    {
        builder.Entity<FirstEntityProperties>()
            .HasTableName(ExampleModelShardInfo.FirstCollectionInfo.Name)
            .HasSchemaName(ExampleModelShardInfo.FirstCollectionInfo.ShardName)
            .Property(p => p.EntityId)
                .IsPrimaryKey()
                .IsNotNull()
                .HasDataType(DataType.Guid)
                .HasConversion(x => x.Id, x => new FirstEntity(x));

        builder.Entity<SecondEntityProperties>()
            .HasTableName(ExampleModelShardInfo.SecondCollectionInfo.Name)
            .HasSchemaName(ExampleModelShardInfo.FirstCollectionInfo.ShardName)
            .Property(p => p.EntityId)
                .IsPrimaryKey()
                .HasDataType(DataType.Guid)
                .IsNotNull()
                .HasConversion(x => x.Id, x => new SecondEntity(x))
            .Property(p => p.EnumProperty)
                .IsNotColumn()
                .HasSkipOnInsert()
                .SkipOnEntityFetch()
                .HasSkipOnUpdate();
    }
}
