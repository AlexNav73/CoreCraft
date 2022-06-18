[![build](https://github.com/AlexNav73/Navitski.Crystalized/workflows/releasing/badge.svg)](https://github.com/AlexNav73/Navitski.Crystalized/actions)
[![codecov](https://codecov.io/gh/AlexNav73/Navitski.Crystalized/branch/master/graph/badge.svg?token=Q6ZY0WHL9J)](https://codecov.io/gh/AlexNav73/Navitski.Crystalized)

## 📖 About

`Navitski.Crystalized` is a set of cross-platform libraries which helps to build your own domain model. Think of it as a in-memory database which provides some interesting features like: precise diffing, undo/redo support, persisting to the file (for now only SQLite database is supported), changes notifications and code generation!

Using the simple json configuration, the `Navitski.Crystalized.Model.Generators` can generate a new domain model for you with all entities, relations, persistence infrastructure and changes handling. All you need to do is start implementing your business logic everything else will be generated for you. `Navitski.Crystalized.Model.Generators` is using brand new Roslyn Source Generators and targeting _.NET Standart 2.0_, so it can be used in old .NET Framework applications.

## 🚀 Getting Started

> To see the full example, go to the [examples](https://github.com/AlexNav73/Navitski.Crystalized/tree/master/examples/Example) folder.

To get started first of all, you need to add references to libraries.

```
dotnet add package Navitski.Crystalized.Model
```

> This is a main package with all core functionality.

To enable code generation add another package:
```
dotnet add package Navitski.Crystalized.Model.Generators
```

After this two packages are installed we can start writing a model description. Create file called `Model.model.json` and add it to the project as a additional file like so:

```xml
<ItemGroup>
    <AdditionalFiles Include="Model.model.json" />
</ItemGroup>
```

> Files with an extension `*.model.json` will be used to generate parts of the domain model called **Shards**. Think of it as a sub-databases which can help to separate domain model by functionality or provide a possibility to register new shards by plugins.

The content of the `Model.model.json` should include shards (sub-databases), entities with their properties, collections and relations between entities. See the example below:
```json
{
  "shards": [
    {
      "name": "MyApp",
      "entities": [
        {
          "name": "MyEntity",
          "properties": [
            {
              "name": "MyProperty",
              "type": "string",
              "defaultValue": "string.Empty"
            }
          ]
        }
      ],
      "collections": [
        {
          "name": "MyEntitiesCollection",
          "type": "MyEntity"
        }
      ],
      "relations": []
    }
  ]
}
```

> For the simplicity, we will use only one entity without any relations

Next, we need to create a domain model class. We could use one of out-of-box implementations. But to show you how simple implement it, we create a new class. This class must inherit from the `DomainModel` abstract class.

```cs
class MyModel : DomainModel
{
    public MyModel(IEnumerable<IModelShard> shards)
        : base(shards, new SyncScheduler())
    {
    }
}
```

> `DomainModel` base constructor takes a collection of shards (which are generated from `Model.model.json`) and a `IScheduler` implementation. For now, we use `SyncScheduler`. It just run everything in the same thread so it can be used for unit testing as an example.

Now we can create our model model:

```cs
var model = new MyModel(new[]
{
    // this model shard will be generated in the {my-project-namespace}.Model
    // and all entities will be generated in {my-project-namespace}.Model.Entities namespace
    new Model.MyAppModelShard()
});
```

When our first domain model created, we can start implementing commands. Commands are used to group related modifications in one single change (SQRS pattern). **All modifications can be made only inside a command**.

```cs
class MyAddCommand : ModelCommand<MyModel>
{
    public MyAddCommand(MyModel model) : base(model) { }

    protected override void ExecuteInternal(IModel model, CancellationToken token)
    {
        // for each shard two interfaces will be generated
        // IMyAppModelShard and IMutableMyAppModelShard
        // first one is a read-only interface and the second one
        // is for commands to allow modifications
        var shard = model.Shard<Model.IMutableMyAppModelShard>();
        shard.MyEntitiesCollection.Add(new() { MyProperty = "test" });
    }
}
```

> This command will simply add new entity to the collection with `MyProperty` set to `"test"`.

Now we need to execute the command, but before that we need to subscribe to the domain model changes.

```cs
using (model.Subscribe(OnModelChanged))
{
    // command should be executed here ...
}

private static void OnModelChanged(ModelChangedEventArgs args)
{
    // here we request changes related to the `MyApp` shard.
    // all this interfaces will be generated automatically for you
    if (args.Changes.TryGetFrame<Model.IMyAppChangesFrame>(out var frame))
    {
        // IMyAppChangesFrame have the same structure as a IMyAppModelShard
        // but instead of ICollection type of MyEntitiesCollection it will
        // have ICollectionChangeSet which contains an action performed,
        // entity, old and new properties 
        foreach(var change in frame.MyEntitiesCollection)
        {
            // here we just print what was changed in the domain model
            // if an entity hasn't changed - the MyEntitiesCollection will
            // not have a record for this entity
            Console.WriteLine($"Entity [{change.Entity}] has been {change.Action}ed.");
            Console.WriteLine($"   Old data: {change.OldData}");
            Console.WriteLine($"   New data: {change.NewData}");
        }
    }
}
```

When we setup everything, we can execute our command:
```cs
new MyAddCommand(model).Execute();
```

The result of the execution will be printed on the console:
```
Entity [MyEntity { Id = 262485ee-3426-4da6-96d5-f65976e7fe9e }] has been Added.
   Old data:
   New data: MyEntityProperties { MyProperty = test }
```