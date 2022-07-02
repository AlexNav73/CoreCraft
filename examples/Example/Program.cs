using Navitski.Crystalized.Model.Engine;
using Navitski.Crystalized.Model.Engine.ChangesTracking;
using Navitski.Crystalized.Model.Engine.Commands;
using Navitski.Crystalized.Model.Engine.Core;
using Navitski.Crystalized.Model.Engine.Persistence;
using Navitski.Crystalized.Model.Engine.Scheduling;
using Navitski.Crystalized.Model.Storage.Sqlite;
using Navitski.Crystalized.Model.Storage.Sqlite.Migrations;

namespace Example;

class Program
{
    private const string Path = "test.db";

    public static void Main()
    {
        if (File.Exists(Path))
        {
            File.Delete(Path);
        }

        var model = new MyModel(new[]
        {
            new Model.ExampleModelShard()
        });
        using (model.Subscribe(OnModelChanged))
        {
            var addCommand = new DelegateCommand(model, shard =>
            {
                for (int i = 0; i < 2; i++)
                {
                    var first = shard.FirstCollection.Add(new() { StringProperty = "test", IntegerProperty = 42 });
                    var second = shard.SecondCollection.Add(new() { BoolProperty = true, DoubleProperty = 0.5 });

                    shard.OneToOneRelation.Add(first, second);
                }
            });
            addCommand.Execute();

            var shard = model.Shard<Model.IExampleModelShard>();

            var modifyCommand = new DelegateCommand(model, shard =>
            {
                var entity = shard.FirstCollection.First();

                shard.FirstCollection.Modify(entity, props => props with { StringProperty = "modified 1" });
                shard.FirstCollection.Modify(entity, props => props with { IntegerProperty = "modified 2".GetHashCode() });
                shard.FirstCollection.Modify(entity, props => props with { StringProperty = "modified 3", IntegerProperty = "modified 3".GetHashCode() });
            });
            modifyCommand.Execute();

            var deleteCommand = new DelegateCommand(model, shard => shard.FirstCollection.Remove(shard.FirstCollection.Last()));
            deleteCommand.Execute();
        }

        model.Save(Path);
    }

    private static void OnModelChanged(ModelChangedEventArgs args)
    {
        if (args.Changes.TryGetFrame<Model.IExampleChangesFrame>(out var frame))
        {
            foreach(var change in frame!.FirstCollection)
            {
                Console.WriteLine($"Entity [{change.Entity}] has been {change.Action}ed.");
                Console.WriteLine($"   Old data: {change.OldData}");
                Console.WriteLine($"   New data: {change.NewData}");
                Console.WriteLine();
            }
        }
    }
}

class MyModel : DomainModel
{
    private readonly IStorage _storage;
    private readonly IList<IModelChanges> _changes;

    public MyModel(IEnumerable<IModelShard> shards)
        : base(shards, new SyncScheduler())
    {
        _storage = new SqliteStorage(
            Array.Empty<IMigration>(),
            new[] { new Model.ExampleModelShardStorage() },
            new SqliteRepositoryFactory());
        _changes = new List<IModelChanges>();
    }

    public void SaveAs(string path)
    {
        if (File.Exists(path))
        {
            File.Delete(path);
        }

        Save(_storage, path);
    }

    public void Save(string path)
    {
        Save(_storage, path, _changes);
        _changes.Clear();
    }

    protected override void OnModelChanged(ModelChangedEventArgs args)
    {
        _changes.Add(args.Changes);
    }
}

class DelegateCommand : ModelCommand<MyModel>
{
    private readonly Action<Model.IMutableExampleModelShard> _action;

    public DelegateCommand(MyModel model, Action<Model.IMutableExampleModelShard> action)
        : base(model)
    {
        _action = action;
    }

    protected override void ExecuteInternal(IModel model, CancellationToken token)
    {
        _action(model.Shard<Model.IMutableExampleModelShard>());
    }
}