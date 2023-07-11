using ConsoleDemoApp.Model;
using CoreCraft;
using CoreCraft.Subscription;
using CoreCraft.Storage.Sqlite;
using CoreCraft.Storage.Sqlite.Migrations;

namespace ConsoleDemoApp;

class Program
{
    private const string Path = "test.db";

    public static async Task Main()
    {
        if (File.Exists(Path))
        {
            File.Delete(Path);
        }

        var storage = new SqliteStorage(
            Array.Empty<IMigration>(),
            new[] { new ExampleModelShardStorage() },
            Console.WriteLine);
        var model = new DomainModel(new[] { new ExampleModelShard() });

        using (model.For<IExampleChangesFrame>().Subscribe(OnCollectionChanged))
        {
            Console.WriteLine("======================== Modifying ========================");

            await model.Run<IMutableExampleModelShard>((shard, _) =>
            {
                var first = shard.FirstCollection.Add(new() { StringProperty = "test", IntegerProperty = 42 });
                var second = shard.SecondCollection.Add(new() { BoolProperty = true, DoubleProperty = 0.5, EnumProperty = Model.Entities.SecondEntityEnum.First });

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

                shard.SecondCollection.Modify(entity, props => props with { EnumProperty = Model.Entities.SecondEntityEnum.Second });
            });

            await model.Run<IMutableExampleModelShard>((shard, _) =>
            {
                var entity = shard.FirstCollection.Last();

                shard.FirstCollection.Remove(entity);
                shard.OneToOneRelation.Remove(entity, shard.OneToOneRelation.Children(entity).Single());
            });
        }

        Console.WriteLine("======================== Saving ========================");
        await model.Save(storage, Path);

        model = new DomainModel(new[] { new ExampleModelShard() });
        using (model.For<IExampleChangesFrame>().Subscribe(OnCollectionChanged))
        {
            Console.WriteLine("======================== Loading ========================");

            await model.Load(storage, Path);
        }
    }

    private static void OnCollectionChanged(Change<IExampleChangesFrame> change)
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
    }
}
