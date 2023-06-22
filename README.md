[![build](https://github.com/AlexNav73/Navitski.Crystalized/workflows/releasing/badge.svg)](https://github.com/AlexNav73/Navitski.Crystalized/actions)
[![codecov](https://codecov.io/gh/AlexNav73/Navitski.Crystalized/branch/master/graph/badge.svg?token=Q6ZY0WHL9J)](https://codecov.io/gh/AlexNav73/Navitski.Crystalized) ![Nuget](https://img.shields.io/nuget/dt/Navitski.Crystalized.Model) ![GitHub](https://img.shields.io/github/license/AlexNav73/Navitski.Crystalized) ![Lines of code](https://img.shields.io/tokei/lines/github/AlexNav73/Navitski.Crystalized)  
[![Nuget (with prereleases)](https://img.shields.io/nuget/vpre/Navitski.Crystalized.Model?color=blue&label=Navitski.Crystalized.Model)](https://www.nuget.org/packages/Navitski.Crystalized.Model)  
[![Nuget (with prereleases)](https://img.shields.io/nuget/vpre/Navitski.Crystalized.Model.Generators?color=blue&label=Navitski.Crystalized.Model.Generators)](https://www.nuget.org/packages/Navitski.Crystalized.Model.Generators)  
[![Nuget (with prereleases)](https://img.shields.io/nuget/vpre/Navitski.Crystalized.Model.Storage.SQLite?color=blue&label=Navitski.Crystalized.Model.Storage.SQLite)](https://www.nuget.org/packages/Navitski.Crystalized.Model.Storage.SQLite)  
[![Nuget (with prereleases)](https://img.shields.io/nuget/vpre/Navitski.Crystalized.Model.Storage.Json?color=blue&label=Navitski.Crystalized.Model.Storage.Json)](https://www.nuget.org/packages/Navitski.Crystalized.Model.Storage.Json)

## Introduction

`Navitski.Crystalized` is a comprehensive toolkit designed to simplify domain modeling and data management in .NET applications. It offers a range of powerful features and functionalities that enable developers to build robust and scalable domain models.

## Main Features

The `Navitski.Crystalized` toolkit provides a wealth of features, including:

1. **Automatic Model Generation with Roslyn Source Generators**: `Navitski.Crystalized` leverages Roslyn Source Generators to automatically generate domain models based on your schema. This automated process eliminates the need for manual coding, saving you time and effort. The generated models are accurate, consistent, and reflect the structure of your schema.

2. **Change Tracking**: `Navitski.Crystalized` incorporates change tracking mechanisms that allow you to monitor modifications to your domain model. By tracking changes at a granular level, `Navitski.Crystalized` notifies you of specific modifications, enabling you to respond effectively. This feature eliminates the need for manual change detection and parsing of the entire model.

3. **Undo/Redo Support**: `Navitski.Crystalized` simplifies the implementation of undo and redo operations in your application. It provides built-in support for managing and reverting changes, giving users the ability to undo actions and redo them as needed.

4. **Command-Query Responsibility Segregation (CQRS) Pattern**: `Navitski.Crystalized` ensures the integrity of your application data during command execution. Commands operate on model snapshots, safeguarding your data from corruption in case of exceptions. If an exception occurs in the middle of command execution, the toolkit rolls back any changes made within the command, maintaining the consistency and reliability of your data. `Navitski.Crystalized` follows the CQRS pattern to handle domain model querying and modification.

5. **Persistence Options**: `Navitski.Crystalized` offers seamless support for persisting your generated domain model. With `Navitski.Crystalized`, there's no need for additional code to handle persistence. It supports saving and loading the model's state to a SQLite database and JSON files. The toolkit takes care of the storage and retrieval process, making it convenient and hassle-free. Additionally, `Navitski.Crystalized` allows for easy implementation of additional storage options, making it flexible to adapt to your specific requirements.

6. **Plugin Architecture Support**: `Navitski.Crystalized` is well-suited for use in a plugin architecture. It provides the necessary abstractions and features to support modular development, allowing different plugins to contribute to the overall application state. This promotes extensibility and adaptability, enabling you to build versatile and customizable applications.

7. **Reactive Extensions (Rx.NET) Integration**: `Navitski.Crystalized` incorporates Reactive Extensions ([Rx.NET](https://github.com/dotnet/reactive)) to provide a flexible subscription mechanism. It utilizes the `IObservable` and `IObserver` interfaces, allowing you to leverage the power of Rx.NET for event-driven programming and reactive data processing. This integration enables you to easily subscribe to change events and apply custom logic using the extensive set of operators provided by `Rx.NET`.

`Navitski.Crystalized` empowers developers to create robust and scalable domain models with ease. With automatic model generation, change tracking, persistence options, and support for undo/redo operations, `Navitski.Crystalized` simplifies data management and enhances the user experience. By incorporating Reactive Extensions for flexible subscriptions and following the CQRS pattern for improved scalability, `Navitski.Crystalized` offers a powerful toolkit for efficient domain modeling and data management.

## Basic usage

The only thing is needed to start using the `Navitski.Crystalized` toolkit is to define the schema for the domain model. Create a `*.model.json` file that describes your entities, properties and their relations. Here's an example:

```json
{
  "shards": [
    {
      "name": "ToDo",
      "entities": [
        {
          "name": "ToDoItem",
          "properties": [
            { "name": "Name", "type": "string", "defaultValue": "string.Empty" }
          ]
        }
      ],
      "collections": [
        { "name": "Items", "entityType": "ToDoItem" }
      ],
      "relations": []
    }
  ]
}
```

The model schema is the only piece needed to define data of your domain model. Everything else will be automatically generated by the `Navitski.Crystalized.Model.Generators` package.

Now, an instance of the domain model can be created using an instance of generated `ToDoModelShard` class:

```cs
// Create an instance of the domain model
var model = new DomainModel(new[] { new ToDoModelShard() });
```

> **Note:** instead of using `DomainModel` class directly, you can use build-in classes (`AutoSaveDomainModel`, `UndoRedoDomainModel`) or inherit from it and implement custom logic

Then we need to subscribe to the model changes by providing an event handler method to handle the collection changes.:

```cs
// Subscribe to Items collection change events 
using var subscription = model.For<IToDoChangesFrame>()
    .With(x => x.Items)
    .Subscribe(OnItemChanged);

// Observe changes
void OnItemChanged(Change<ICollectionChangeSet<ToDoItem, ToDoItemProperties>> changes)
{
    foreach (var c in changes.Hunk)
    {
        Console.WriteLine($"Entity [{c.Entity}] has been {c.Action}ed.");
        Console.WriteLine($"   Old data: {c.OldData}");
        Console.WriteLine($"   New data: {c.NewData}");
    }
}
```

When subscription is done, let's execute a command to modify the model:

```cs
// Adds new item to the collection
model.Run<IMutableToDoModelShard>(
    (shard, _) => shard.Items.Add(new() { Name = "test" }));
```

Save the domain model to an SQLite database file.

```cs
var storage = new SqliteStorage(
    Array.Empty<IMigration>(),
    new[] { new ToDoModelShardStorage() });

model.Save(storage, "my_data.db");
```

Please refer to the [documentation](https://github.com/AlexNav73/Navitski.Crystalized/wiki/Getting-Started) for comprehensive information on using the `Navitski.Crystalized` toolkit and its features.
