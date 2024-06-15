## Version 0.7.0

**Changed**:

- The `DomainModel` no longer accepts the path argument in the `Save` and `Load` methods. Instead, storage should be configured to know where to save or load data.
- Collection/relation information is now stored inside collection/relation instances. It no longer needed to specify CollectionInfo/RelationInfo everywhere.
- 🎉 Implemented the `Lazy Loading API`.
  - Model Shards can now be marked as `loadManually` and will be skipped during regular loading. (Can be later loaded fully or partially)
  - Collections can be marked as `loadManually` and loaded manually when needed.
- 🎉 Implemented changes' history saving and loading
- Added `Remove(Parent)` method to remove the parent and all its child entities

**Breaking changes**:

- Introduced the `IMutableModel` interface as a pair to its read-only counterpart `IModel`. Now the compiler will tell if the wrong interface is used to query a model shard (for example: in commands, now it is impossible to get an exception while querying the read-only interface `IExampleModelShard`, instead of mutable `IMutableExampleModelShard`).
- Replaced `parentRelationType` and `childRelationType` with a single `relationType` property in the `*.model.json` file.
- Removed the word "Type" from the relation's parent and child type definitions in the `*.model.json` file.
- Moved everything related to model changes history into the separate class `ChangesHistory` and made it available using the History property on the `UndoRedoDomainModel` class
- Renamed `AreLinked` to `Contains` for the `Relation` type
- Moved subscription extension methods to the separate namespace called `CoreCraft.Subscription.Extensions`
- Renamed `BindingChanges` to the `CollectionChangeGroups` and changed the signature of the collection change handler to `Change<CollectionChangeGroups<TEntity, TProperties>>`

## Version 0.6.0

**Changed**:

- Removed generated storage classes
- Created static Info classes to describe model shards
- Added `ICanBeSaved/ICanBeLoaded` interfaces to denote who can be saved/loaded
- Added `ITableOperations` interface allowing to use of Fluent API in migrations to perform operations with tables
- Made `IScheduler` optional parameter for domain model classes
- Changed visibility options in the schema file

## Version 0.5.0

**Changed**:

- Added the possibility to create and apply custom wrappers over collections and relations when creating mutable model shards
- Revised how visibility is specified in the `*.model.json` file
- Renamed the interfaces `ICanBeReadOnly` and `ICanBeMutable` to avoid confusion regarding which class represents each state
- Removed the `IModelShardAccessor` interface. The `IModel` interface now describes each object that can be queried for model shards.
- Introduced the `ExecuteRawSql` method for the `IMigrator`
- Completely reworked the subscription API to follow the Fluent API pattern
- Fixed an issue with subscribing to the same collection/relation using a different name for the lambda argument.
- Implemented the ability to bind to collection and entity changes.
- Subscription builders now implement the `IObservable` interface (allowing the use of [Rx.NET](https://github.com/dotnet/reactive) to subscribe to events)
- Made the `DomainModel` class non-abstract, providing the option to use it directly without the need to create a new class and inherit from it
- Now packages produced by release branches are available (the master branch now produces versions without the `*-alpha` postfix).
- Added the `AreLinked` method to the `IRelation` interface, enabling checks to determine if the parent and child entities are linked with each other

## Version 0.4.0

**Changed**:

- Merging of model changes happens in the `DomainModel` base class rather than doing so in each storage implementation
- Pass `IStorage` as a parameter to Save/Load methods
- Added `Json` storage
- Made generated properties types partial
- Removed `ISqliteRepositoryFactory` from the storage constructor
- Changed `Contains` methods in the `Relation` class to `ContainParent/ContainChild`
- Do not copy model shards before saving the model to the storage
- Implemented CoW collections/relations to copy only those collections/relations which are modified

## Version 0.3.0

**Changed**:

- Added `Pairs` method to the `ICollection<TEntity, TProperties>`
- Added perf testing project
- Compress changes when adding them to a change set and before saving them to the database
- The generated namespace will be called like model file
- Added logging support for `SQLite` repository (SQL queries now can be logged)

## Version 0.2.0

**Changed**:

- Renamed `Message` type to `Change` record (can be deconstructed)
- Simplified model subscription DSL
- Changed how commands should be implemented (+ added some helper methods to even avoid creating of new command type)
- Added WPF example
- `.model.json` file properties for visibility and relation types now map to enums
- Sealed all internal and most public classes
- Made generated model shard class partial and implemented `ICopy` and `ITrackable` interfaces in the separate
- Added Undo and Redo stacks to `UndoRedoDomainModel`
- Added `.editorconfig`
- Fixed bug with not clearing Redo stack when new changes happened
- Added snapshot testing for Source Generators
- Added Contains method for `IRelation` and `ICollection` interfaces
- Use Incremental compilation for source generation
