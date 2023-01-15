[![build](https://github.com/AlexNav73/Navitski.Crystalized/workflows/releasing/badge.svg)](https://github.com/AlexNav73/Navitski.Crystalized/actions)
[![codecov](https://codecov.io/gh/AlexNav73/Navitski.Crystalized/branch/master/graph/badge.svg?token=Q6ZY0WHL9J)](https://codecov.io/gh/AlexNav73/Navitski.Crystalized)

## 📖 About

`Navitski.Crystalized` is a set of cross-platform libraries which helps in building of your own domain model. Think of it as a in-memory database which provides some interesting features like: precise diffing, undo/redo support, persisting to the file (for now only SQLite database is supported), changes notifications and code generation!

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
          "entityType": "MyEntity"
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

When our first domain model created, we can start implementing commands. Commands are used to group related modifications in one single change (CQRS pattern). **All modifications can be made only inside a command**.

```cs
class MyAddCommand : ICommand
{
    public void Execute(IModel model, CancellationToken token)
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

private static void OnModelChanged(Change<IModelChanges> change)
{
    // here we request changes related to the `MyApp` shard.
    // all this interfaces will be generated automatically for you
    if (change.Hunk.TryGetFrame<Model.IMyAppChangesFrame>(out var frame) && frame.HasChanges())
    {
        // IMyAppChangesFrame have the same structure as a IMyAppModelShard
        // but instead of ICollection type of MyEntitiesCollection it will
        // have ICollectionChangeSet which contains an action performed,
        // entity, old and new properties 
        foreach(var cc in frame.MyEntitiesCollection)
        {
            // here we just print what was changed in the domain model
            // if an entity hasn't changed - the MyEntitiesCollection will
            // not have a record for this entity
            Console.WriteLine($"Entity [{cc.Entity}] has been {cc.Action}ed.");
            Console.WriteLine($"   Old data: {cc.OldData}");
            Console.WriteLine($"   New data: {cc.NewData}");
        }
    }
}
```

It is also possible to subscribe to specific changes like model shard changes or collection/relation changes

```cs
using (model.SubscribeTo<Model.IMyAppChangesFrame>(x => x.With(y => y.MyEntitiesCollection).By(OnModelChanged)))
{
    // command should be executed here ...
}

private static void OnModelChanged(Change<ICollectionChangeSet<MyEntity, MyEntityProperties>> change)
{
    foreach(var c in change.Hunk)
    {
        // here we just print what was changed in the domain model
        // if an entity hasn't changed - the MyEntitiesCollection will
        // not have a record for this entity
        Console.WriteLine($"Entity [{c.Entity}] has been {c.Action}ed.");
        Console.WriteLine($"   Old data: {c.OldData}");
        Console.WriteLine($"   New data: {c.NewData}");
    }
}
```

When we setup everything, we can execute our command:
```cs
using (model.Subscribe(OnModelChanged))
{
    model.Run(new MyAddCommand());
}
```

The result of the execution will be printed on the console:
```
Entity [MyEntity { Id = 262485ee-3426-4da6-96d5-f65976e7fe9e }] has been Added.
   Old data:
   New data: MyEntityProperties { MyProperty = test }
```

### Store data in the SQLite database

To store model data in the SQLite database, the `Navitski.Crystalized.Model.Storage.Sqlite` package can be used.

```
dotnet add package Navitski.Crystalized.Model.Storage.Sqlite
```

When we have added this package we can then implement a save method for the model class:

```cs
// the "DomainModel" base class provides couple of methods to save and load data.
class MyModel : DomainModel
{
    // "IStorage" interface comes from "Navitski.Crystalized.Model" package
    // and everybody can implement it to store data in a suitable way.
    // Out-of-the-box only SQLite is supported.
    private readonly IStorage _storage;

    public MyModel(IEnumerable<IModelShard> shards)
        : base(shards, new SyncScheduler())
    {
        _storage = new SqliteStorage(
            // To create a SQLite storage, we need to provide a collection
            // of migrations (each database store the latest version so it will
            // execute only those migrations that are needed).
            Array.Empty<IMigration>(),
            // Also, we need to provide model shard storages which are generated
            // automatically for each shard. Storages know, how to store a specific
            // model shard.
            new[] { new Model.ExampleModelShardStorage() },
            // Repository factory is needed to create a new repository instance
            // when we have a path to the file
            new SqliteRepositoryFactory());
    }

    public void Save(string path)
    {
        // base Save method writes all the data to the file.
        // We don't want to corrupt the data so we delete the old
        // file before we save all the data again 
        if (File.Exists(path))
        {
            File.Delete(path);
        }

        // in these case, we call base Save method to save whole
        // data to the file. If we want to save only changes, then we need
        // to call another Save overload which accepts a collection of IModelChanges.
        Save(_storage, path);
    }
}
```

Now, we are ready to save our model.

```cs
var model = new MyModel(new[] { /* ... */ });
using (model.Subscribe(OnModelChanged))
{
    // executing some commands ...
}
// save all data to the file
model.Save("test.db");
```