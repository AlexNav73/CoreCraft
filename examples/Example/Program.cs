using Navitski.Crystalized.Model.Engine;
using Navitski.Crystalized.Model.Engine.Commands;
using Navitski.Crystalized.Model.Engine.Core;
using Navitski.Crystalized.Model.Engine.Scheduling;

namespace Example;

class Program
{
    public static void Main()
    {
        var model = new MyModel(new[]
        {
            new Model.ExampleModelShard()
        });
        using (model.Subscribe(OnModelChanged))
        {
            var addCommand = new MyAddCommand(model);
            addCommand.Count.Set(2);
            addCommand.Execute();

            var shard = model.Shard<Model.IExampleModelShard>();
            var entity = shard.FirstCollection.First();

            var modifyCommand = new MyModifyCommand(model);
            modifyCommand.Entity.Set(entity);
            modifyCommand.Execute();

            var deleteCommand = new MyDeleteCommand(model);
            deleteCommand.Entity.Set(entity);
            deleteCommand.Execute();
        }
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
    public MyModel(IEnumerable<IModelShard> shards)
        : base(shards, new SyncScheduler())
    {
    }
}

class MyAddCommand : ModelCommand<MyModel>
{
    public MyAddCommand(MyModel model)
        : base(model)
    {
        Count = Parameter<int>(nameof(Count));
    }

    public ICommandParameter<int> Count { get; }

    protected override void ExecuteInternal(IModel model, CancellationToken token)
    {
        var shard = model.Shard<Model.IMutableExampleModelShard>();

        for (int i = 0; i < Count.Value; i++)
        {
            var first = shard.FirstCollection.Add(new() { StringProperty = "test", IntegerProperty = 42 });
            var second = shard.SecondCollection.Add(new() { BoolProperty = true, DoubleProperty = 0.5 });

            shard.OneToOneRelation.Add(first, second);
        }
    }
}

class MyModifyCommand : ModelCommand<MyModel>
{
    public MyModifyCommand(MyModel model)
        : base(model)
    {
        Entity = Parameter<Model.Entities.FirstEntity>(nameof(Entity));
    }

    public ICommandParameter<Model.Entities.FirstEntity> Entity { get; }

    protected override void ExecuteInternal(IModel model, CancellationToken token)
    {
        var shard = model.Shard<Model.IMutableExampleModelShard>();

        shard.FirstCollection.Modify(Entity.Value, props => props with { StringProperty = "modified" });
    }
}

class MyDeleteCommand : ModelCommand<MyModel>
{
    public MyDeleteCommand(MyModel model)
        : base(model)
    {
        Entity = Parameter<Model.Entities.FirstEntity>(nameof(Entity));
    }

    public ICommandParameter<Model.Entities.FirstEntity> Entity { get; }

    protected override void ExecuteInternal(IModel model, CancellationToken token)
    {
        var shard = model.Shard<Model.IMutableExampleModelShard>();

        shard.FirstCollection.Remove(Entity.Value);
    }
}