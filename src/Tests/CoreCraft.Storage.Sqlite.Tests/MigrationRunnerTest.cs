using CoreCraft.ChangesTracking;
using CoreCraft.Storage.Sqlite.Migrations;
using CoreCraft.Storage.Sqlite.Tests.Migrations;

namespace CoreCraft.Storage.Sqlite.Tests;

internal class MigrationRunnerTest
{
    [Test]
    public void DropCollectionTableMigrationTest()
    {
        using var repository = new SqliteRepository(":memory:");
        var runner = new MigrationRunner(new[] { new DropCollectionTableMigration(FakeModelShardStorage.FirstCollectionInfo) });

        repository.Update(
            FakeModelShardStorage.FirstCollectionInfo,
            new CollectionChangeSet<FirstEntity, FirstEntityProperties>("")
            {
                { CollectionAction.Add, new(), null, new() }
            });

        Assert.That(repository.Exists(FakeModelShardStorage.FirstCollectionInfo), Is.True);
        Assert.That(repository.GetDatabaseVersion(), Is.EqualTo(0));

        runner.Run(repository);

        Assert.That(repository.Exists(FakeModelShardStorage.FirstCollectionInfo), Is.False);
        Assert.That(repository.GetDatabaseVersion(), Is.EqualTo(1));
    }

    [Test]
    public void DropRelationTableMigrationTest()
    {
        using var repository = new SqliteRepository(":memory:");
        var runner = new MigrationRunner(new[] { new DropRelationTableMigration(FakeModelShardStorage.OneToOneRelationInfo) });

        repository.Update(
            FakeModelShardStorage.OneToOneRelationInfo,
            new RelationChangeSet<FirstEntity, SecondEntity>("")
            {
                { RelationAction.Linked, new(), new() }
            });

        Assert.That(repository.Exists(FakeModelShardStorage.OneToOneRelationInfo), Is.True);
        Assert.That(repository.GetDatabaseVersion(), Is.EqualTo(0));

        runner.Run(repository);

        Assert.That(repository.Exists(FakeModelShardStorage.OneToOneRelationInfo), Is.False);
        Assert.That(repository.GetDatabaseVersion(), Is.EqualTo(1));
    }

    [Test]
    public void AddColumnMigrationTest()
    {
        using var repository = new SqliteRepository(":memory:");
        var nonNullColumn = "NonNullInt";
        var nullColumn = "NullInt";
        var notNullStringColumn = "NonNullString";
        var nullStringColumn = "NullString";
        var runner = new MigrationRunner(new[]
        {
            new AddColumnMigration<int>(FakeModelShardStorage.FirstCollectionInfo, nonNullColumn, false, 5) as IMigration,
            new AddColumnMigration<int>(FakeModelShardStorage.FirstCollectionInfo, nullColumn, true),
            new AddColumnMigration<string>(FakeModelShardStorage.FirstCollectionInfo, notNullStringColumn, false, "test"),
            new AddColumnMigration<string>(FakeModelShardStorage.FirstCollectionInfo, nullStringColumn, true),
        });

        repository.Update(
            FakeModelShardStorage.FirstCollectionInfo,
            new CollectionChangeSet<FirstEntity, FirstEntityProperties>("")
            {
                { CollectionAction.Add, new(), null, new() }
            });

        var columns = repository.QueryTableColumns(FakeModelShardStorage.FirstCollectionInfo).ToArray();

        Assert.That(columns.Any(x => x.Name == nonNullColumn && x.Type == "INTEGER" && x.IsNotNull && string.Equals(x.DefaultValue?.ToString(), "5")), Is.False);
        Assert.That(columns.Any(x => x.Name == nullColumn && x.Type == "INTEGER" && !x.IsNotNull), Is.False);
        Assert.That(columns.Any(x => x.Name == notNullStringColumn && x.Type == "TEXT" && x.IsNotNull && string.Equals(x.DefaultValue?.ToString(), "test")), Is.False);
        Assert.That(columns.Any(x => x.Name == nullStringColumn && x.Type == "TEXT" && !x.IsNotNull), Is.False);
        Assert.That(repository.GetDatabaseVersion(), Is.EqualTo(0));

        runner.Run(repository);

        columns = repository.QueryTableColumns(FakeModelShardStorage.FirstCollectionInfo).ToArray();

        Assert.That(columns.Any(x => x.Name == nonNullColumn && x.Type == "INTEGER" && x.IsNotNull && string.Equals(x.DefaultValue?.ToString(), "5")), Is.True);
        Assert.That(columns.Any(x => x.Name == nullColumn && x.Type == "INTEGER" && !x.IsNotNull), Is.True);
        Assert.That(columns.Any(x => x.Name == notNullStringColumn && x.Type == "TEXT" && x.IsNotNull && string.Equals(x.DefaultValue?.ToString(), "\"test\"")), Is.True);
        Assert.That(columns.Any(x => x.Name == nullStringColumn && x.Type == "TEXT" && !x.IsNotNull), Is.True);
        Assert.That(repository.GetDatabaseVersion(), Is.EqualTo(2));
    }

    [Test]
    public void DropColumnMigrationTest()
    {
        using var repository = new SqliteRepository(":memory:");
        var columnName = "NonNullableStringProperty";
        var runner = new MigrationRunner(new[]
        {
            new DropColumnMigration(FakeModelShardStorage.FirstCollectionInfo, columnName)
        });

        repository.Update(
            FakeModelShardStorage.FirstCollectionInfo,
            new CollectionChangeSet<FirstEntity, FirstEntityProperties>("")
            {
                { CollectionAction.Add, new(), null, new() }
            });

        var columns = repository.QueryTableColumns(FakeModelShardStorage.FirstCollectionInfo).ToArray();

        Assert.That(columns.Any(x => x.Name == columnName), Is.True);
        Assert.That(repository.GetDatabaseVersion(), Is.EqualTo(0));

        runner.Run(repository);

        columns = repository.QueryTableColumns(FakeModelShardStorage.FirstCollectionInfo).ToArray();

        Assert.That(columns.Any(x => x.Name == columnName), Is.False);
        Assert.That(repository.GetDatabaseVersion(), Is.EqualTo(3));
    }

    [Test]
    public void RenameColumnMigrationTest()
    {
        using var repository = new SqliteRepository(":memory:");
        var columnName = "NonNullableStringProperty";
        var newColumnName = "Test";
        var runner = new MigrationRunner(new[]
        {
            new RenameColumnMigration(FakeModelShardStorage.FirstCollectionInfo, columnName, newColumnName)
        });

        repository.Update(
            FakeModelShardStorage.FirstCollectionInfo,
            new CollectionChangeSet<FirstEntity, FirstEntityProperties>("")
            {
                { CollectionAction.Add, new(), null, new() }
            });

        var columns = repository.QueryTableColumns(FakeModelShardStorage.FirstCollectionInfo).ToArray();

        Assert.That(columns.Any(x => x.Name == columnName), Is.True);
        Assert.That(columns.Any(x => x.Name == newColumnName), Is.False);
        Assert.That(repository.GetDatabaseVersion(), Is.EqualTo(0));

        runner.Run(repository);

        columns = repository.QueryTableColumns(FakeModelShardStorage.FirstCollectionInfo).ToArray();

        Assert.That(columns.Any(x => x.Name == columnName), Is.False);
        Assert.That(columns.Any(x => x.Name == newColumnName), Is.True);
        Assert.That(repository.GetDatabaseVersion(), Is.EqualTo(4));
    }

    [Test]
    public void RunFailsOnApplyingTransactionTest()
    {
        using var repository = new SqliteRepository(":memory:");
        var runner = new MigrationRunner(new[] { new FailingMigration() });

        Assert.Throws<NotImplementedException>(() => runner.Run(repository));
    }

    [Test]
    public void ExecuteRawSqlTest()
    {
        var table = new CollectionInfo("Test", "TestCollection");
        using var repository = new SqliteRepository(":memory:");
        var sql = $"CREATE TABLE [{QueryBuilder.InferName(table)}]([Id] INTEGER);";
        var runner = new MigrationRunner(new[] { new ExecuteRawSqlMigration(sql) });

        Assert.That(repository.Exists(table), Is.False);
        Assert.That(repository.GetDatabaseVersion(), Is.EqualTo(0));

        runner.Run(repository);

        Assert.That(repository.Exists(table), Is.True);
        Assert.That(repository.GetDatabaseVersion(), Is.EqualTo(1));
    }
}
