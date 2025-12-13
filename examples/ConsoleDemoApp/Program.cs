using ConsoleDemoApp.Model;
using ConsoleDemoApp.Model.Entities;
using CoreCraft;
using CoreCraft.Core;
using CoreCraft.Persistence;
using CoreCraft.Scheduling;
using CoreCraft.Storage.Json;
using CoreCraft.Storage.Sqlite;
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

        var shard = new LazyModelShard(db);

        //var entity1 = await shard.FirstCollection.AddAsync(() => new FirstEntityProperties()
        //{
        //    IntegerProperty = 1,
        //    StringProperty = "First",
        //});
        //var entity2 = await shard.FirstCollection.AddAsync(() => new FirstEntityProperties()
        //{
        //    IntegerProperty = 2,
        //    StringProperty = "Second",
        //});
        //var entity3 = await shard.FirstCollection.AddAsync(() => new FirstEntityProperties()
        //{
        //    IntegerProperty = 3,
        //    StringProperty = "Third",
        //});

        //var entity4 = await shard.SecondCollection.AddAsync(() => new SecondEntityProperties()
        //{
        //    BoolProperty = false,
        //    DoubleProperty = 0.5,
        //    FloatProperty = 0.7f,
        //    IntProperty = 5,
        //});

        //var id2or3 = shard.FirstCollection
        //    .Where(p => Sql.Property<int>(p, "Id") == 2 || Sql.Property<int>(p, "Id") == 3)
        //    .Select(p => Sql.Property<int>(p, "Id"));

        //shard.FirstCollection
        //    .Where(p => id2or3.Contains(Sql.Property<int>(p, "Id")))
        //    .Select(p => new { Id = Sql.Property<int>(p, "Id"), p.IntegerProperty, p.StringProperty })
        //    .ToList()
        //    .ForEach(p => Console.WriteLine($"Id: {p.Id}, Number: {p.IntegerProperty}, String: {p.StringProperty}"));

        //var p = await shard.FirstCollection.GetAsync(entity2);
        //Console.WriteLine($"Id: {entity2.Id}, Number: {p?.IntegerProperty}, String: {p?.StringProperty}");

        //await shard.FirstCollection.ModifyAsync(entity2, p => p.IntegerProperty, 3);

        //p = await shard.FirstCollection.GetAsync(entity2);
        //Console.WriteLine($"Id: {entity2.Id}, Number: {p?.IntegerProperty}, String: {p?.StringProperty}");

        //p = await shard.FirstCollection.GetAsync(entity3);
        //Console.WriteLine(p is not null);

        //await shard.FirstCollection.RemoveAsync(entity3);

        //p = await shard.FirstCollection.GetAsync(entity3);
        //Console.WriteLine(p is null);

        //var contains = await shard.FirstCollection.ContainsAsync(entity1);
        //Console.WriteLine("Contains entity1: {0}", contains);

        var storage = new SqliteStorage(Path, [], Console.WriteLine);
        var historyStorage = new JsonStorage(History, new() { Formatting = Newtonsoft.Json.Formatting.Indented });
        var model = new UndoRedoDomainModel([new ExampleModelShard(), shard], new SyncScheduler());

        using (model.For<IExampleChangesFrame>().Subscribe(OnExampleShardChanged))
        {
            Console.WriteLine("======================== Modifying ========================");

            await model.Run<IMutableLazyModelShard>(async (shard, _) =>
            {
                var first = await shard.FirstCollection.AddAsync(new() { StringProperty = "test", IntegerProperty = 42 });
                var second = await shard.SecondCollection.AddAsync(new() { BoolProperty = true, DoubleProperty = 0.5, FloatProperty = 0.75f, IntProperty = (int)SecondEntityEnum.Second });

                //shard.OneToOneRelation.Add(first, second);
            });

            await model.Run<IMutableLazyModelShard>(async (shard, _) =>
            {
                var entity = shard.FirstCollection.Entities.First();

                await shard.FirstCollection.ModifyAsync(entity, props => props.StringProperty, "modified 1");
                await shard.FirstCollection.ModifyAsync(entity, props => props.IntegerProperty, "modified 2".GetHashCode());
                await shard.FirstCollection.ModifyAsync(entity, props => props.StringProperty, "modified 3");
                await shard.FirstCollection.ModifyAsync(entity, props => props.IntegerProperty, "modified 3".GetHashCode());
            });

            await model.Run<IMutableLazyModelShard>(async (shard, _) =>
            {
                var entity = await shard.SecondCollection.Entities.FirstAsync();

                await shard.SecondCollection.ModifyAsync(entity, props => props.IntProperty, (int)SecondEntityEnum.Second);
            });

            await model.Run<IMutableLazyModelShard>(async (shard, _) =>
            {
                var entity = await shard.FirstCollection.Entities.FirstAsync();

                await shard.FirstCollection.RemoveAsync(entity);
                //shard.OneToOneRelation.Remove(entity);
            });
        }

        //Console.WriteLine("======================== Saving ========================");
        //await model.Save(storage);
        //await model.History.Save(historyStorage);

        //model = new UndoRedoDomainModel([new ExampleModelShard()], new SyncScheduler());
        //using (model.For<IExampleChangesFrame>().Subscribe(OnExampleShardChanged))
        //{
        //    Console.WriteLine("======================== Loading ========================");

        //    await model.History.Load(historyStorage);
        //    await model.Load(storage, force: true);

        //    Console.WriteLine("======================== Adding new change ========================");

        //    await model.Run<IMutableExampleModelShard>((shard, _) =>
        //    {
        //        shard.FirstCollection.Add(new() { StringProperty = "test", IntegerProperty = 42 });
        //    });
        //    await model.History.Save(historyStorage);
        //    await model.History.Load(historyStorage);

        //    Console.WriteLine("======================== Undo after load ========================");

        //    var historySize = model.History.UndoStack.Count;
        //    for (int i = 0; i < historySize; i++)
        //    {
        //        await model.History.Undo();
        //    }
        //}
    }

    private static void OnExampleShardChanged(Change<IExampleChangesFrame> change)
    {
        //foreach (var c in change.Hunk.FirstCollection)
        //{
        //    Console.WriteLine($"Entity [{c.Entity}] has been {c.Action}ed.");
        //    Console.WriteLine($"   Old data: {c.OldData}");
        //    Console.WriteLine($"   New data: {c.NewData}");
        //    Console.WriteLine();
        //}

        //foreach (var c in change.Hunk.SecondCollection)
        //{
        //    Console.WriteLine($"Entity [{c.Entity}] has been {c.Action}ed.");
        //    Console.WriteLine($"   Old data: {c.OldData}");
        //    Console.WriteLine($"   New data: {c.NewData}");
        //    Console.WriteLine();
        //}

        //foreach (var c in change.Hunk.OneToOneRelation)
        //{
        //    Console.WriteLine($"Parent [{c.Parent}] and Child [{c.Child}] has been {c.Action}.");
        //    Console.WriteLine();
        //}
    }
}

public interface ILazyModelShard : IModelShard
{
    ILazyCollection<FirstEntity, FirstEntityProperties> FirstCollection { get; }
    ILazyCollection<SecondEntity, SecondEntityProperties> SecondCollection { get; }
}

public interface IMutableLazyModelShard : IMutableModelShard
{
    IMutableLazyCollection<FirstEntity, FirstEntityProperties> FirstCollection { get; }
    IMutableLazyCollection<SecondEntity, SecondEntityProperties> SecondCollection { get; }
}

public partial class LazyModelShard : ILazyModelShard
{
    public LazyModelShard(DataConnection db)
    {
        FirstCollection = new LazyCollection<FirstEntity, FirstEntityProperties>(
            db.GetTable<FirstEntityProperties>(),
            ExampleModelShardInfo.FirstCollectionInfo);
        SecondCollection = new LazyCollection<SecondEntity, SecondEntityProperties>(
            db.GetTable<SecondEntityProperties>(),
            ExampleModelShardInfo.SecondCollectionInfo);
    }

    internal LazyModelShard(IMutableLazyModelShard mutableShard)
    {
        FirstCollection = mutableShard.FirstCollection;
        SecondCollection = mutableShard.SecondCollection;
    }

    public ILazyCollection<FirstEntity, FirstEntityProperties> FirstCollection { get; }

    public ILazyCollection<SecondEntity, SecondEntityProperties> SecondCollection { get; }

    public void Save(IRepository repository)
    {
    }
}

public partial class MutableLazyModelShard : IMutableLazyModelShard, IMutableState<ILazyModelShard>
{
    public required IMutableLazyCollection<FirstEntity, FirstEntityProperties> FirstCollection { get; init; }

    public required IMutableLazyCollection<SecondEntity, SecondEntityProperties> SecondCollection { get; init; }

    public bool ManualLoadRequired { get; }

    public ILazyModelShard AsReadOnly()
    {
        return new LazyModelShard(this);
    }

    public void Load(IRepository repository, bool force = false)
    {
    }

    public void Save(IRepository repository)
    {
    }
}

public partial class LazyModelShard : IReadOnlyState<IMutableLazyModelShard>
{
    public IMutableLazyModelShard AsMutable(IEnumerable<IFeature> features)
    {
        return new MutableLazyModelShard()
        {
            FirstCollection = (IMutableLazyCollection<FirstEntity, FirstEntityProperties>)FirstCollection,
            SecondCollection = (IMutableLazyCollection<SecondEntity, SecondEntityProperties>)SecondCollection,
        };
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
