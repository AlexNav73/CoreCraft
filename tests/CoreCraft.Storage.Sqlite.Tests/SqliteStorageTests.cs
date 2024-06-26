﻿using FakeItEasy;
using CoreCraft.ChangesTracking;
using CoreCraft.Persistence;
using CoreCraft.Storage.Sqlite.Migrations;
using System.Data;
using CoreCraft.Persistence.Lazy;
using CoreCraft.Persistence.Operations;
using CoreCraft.Persistence.History;

namespace CoreCraft.Storage.Sqlite.Tests;

public class SqliteStorageTests
{
    private ISqliteRepositoryFactory? _factory;
    private ISqliteRepository? _repo;
    private IDbTransaction? _transaction;

    [SetUp]
    public void SetUp()
    {
        _transaction = A.Fake<IDbTransaction>();
        _repo = A.Fake<ISqliteRepository>();
        _factory = A.Fake<ISqliteRepositoryFactory>();
        
        A.CallTo(() => _factory.Create(A<string>.Ignored, A<Action<string>>.Ignored)).Returns(_repo);
        A.CallTo(() => _repo.BeginTransaction()).Returns(_transaction);
    }

    [Test]
    public void UpdateTransactionRollbackOnExceptionTest()
    {
        var change = A.Fake<IChangesFrameEx>();
        A.CallTo(() => change.Do(A<UpdateChangesFrameOperation>.Ignored))
            .Throws<InvalidOperationException>();

        var storage = new SqliteStorage("", [], _factory!);

        Assert.Throws<InvalidOperationException>(() => storage.Update([change]));

        A.CallTo(() => _repo!.BeginTransaction()).MustHaveHappenedOnceExactly();
        A.CallTo(() => _transaction!.Commit()).MustNotHaveHappened();
        A.CallTo(() => _transaction!.Rollback()).MustHaveHappenedOnceExactly();
    }

    [Test]
    public void UpdateIsCalledOnModelShardStorageTest()
    {
        var change = A.Fake<IChangesFrameEx>();
        var storage = new SqliteStorage("", [], _factory!);

        storage.Update([change]);

        A.CallTo(() => change.Do(A<UpdateChangesFrameOperation>.Ignored))
            .MustHaveHappenedOnceExactly();
    }

    [Test]
    public void SaveTransactionCreatedSuccessfullyAndCommittedTest()
    {
        var storage = new SqliteStorage("", [], _factory!);

        storage.Save(A.Fake<IEnumerable<IModelShard>>());

        A.CallTo(() => _repo!.BeginTransaction()).MustHaveHappenedOnceExactly();
        A.CallTo(() => _transaction!.Commit()).MustHaveHappenedOnceExactly();
        A.CallTo(() => _transaction!.Rollback()).MustNotHaveHappened();
    }

    [Test]
    public void SaveTransactionRollbackOnExceptionTest()
    {
        var shard = A.Fake<IModelShard>();
        A.CallTo(() => shard.Save(A<IRepository>.Ignored)).Throws<InvalidOperationException>();

        var storage = new SqliteStorage("", [], _factory!);

        Assert.Throws<InvalidOperationException>(() => storage.Save([shard]));

        A.CallTo(() => _repo!.BeginTransaction()).MustHaveHappenedOnceExactly();
        A.CallTo(() => _transaction!.Commit()).MustNotHaveHappened();
        A.CallTo(() => _transaction!.Rollback()).MustHaveHappenedOnceExactly();
    }

    [Test]
    public void SaveIsCalledOnModelShardStorageTest()
    {
        var shard = A.Fake<IModelShard>();
        var storage = new SqliteStorage("", [], _factory!);

        storage.Save([shard]);

        A.CallTo(() => shard.Save(A<IRepository>.Ignored)).MustHaveHappenedOnceExactly();
    }

    [Test]
    public void SaveMigrationRunnerUpdatedDatabaseVersionTest()
    {
        var shard = A.Fake<IModelShard>();
        var migration1 = A.Fake<IMigration>();
        var migration2 = A.Fake<IMigration>();
        A.CallTo(() => migration1.Version).Returns(1);
        A.CallTo(() => migration2.Version).Returns(2);
        var storage = new SqliteStorage("", [migration1, migration2], _factory!);

        storage.Save([shard]);

        A.CallTo(() => _repo!.SetDatabaseVersion(2))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => shard.Save(A<IRepository>.Ignored))
            .MustHaveHappenedOnceExactly();
    }

    [Test]
    public void SaveHistoryTest()
    {
        var modelChanges = A.Fake<IModelChanges>();
        var storage = new SqliteStorage("", [], _factory!);

        storage.Save([modelChanges]);

        A.CallTo(() => modelChanges.Save(A<IHistoryRepository>.Ignored))
            .MustHaveHappenedOnceExactly();
    }

    [Test]
    public void SavingNewChangesHistoryShouldClearOldHistoryTest()
    {
        var storage = new SqliteStorage("", [], _factory!);
        var changes = A.Fake<IModelChanges>();

        storage.Save([changes]);

        A.CallTo(() => _repo!.ExecuteNonQuery(QueryBuilder.DropTable(QueryBuilder.History.CollectionHistory)))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _repo!.ExecuteNonQuery(QueryBuilder.DropTable(QueryBuilder.History.RelationHistory)))
            .MustHaveHappenedOnceExactly();
    }

    [Test]
    public void LoadModelTest()
    {
        var shard = A.Fake<IMutableModelShard>();
        var migration1 = A.Fake<IMigration>();
        var migration2 = A.Fake<IMigration>();
        A.CallTo(() => migration1.Version).Returns(1);
        A.CallTo(() => migration2.Version).Returns(2);
        var storage = new SqliteStorage("", [migration1, migration2], _factory!);

        storage.Load([shard]);

        A.CallTo(() => _repo!.GetDatabaseVersion()).MustHaveHappenedOnceExactly();
        A.CallTo(() => _repo!.SetDatabaseVersion(1)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _repo!.SetDatabaseVersion(2)).MustHaveHappenedOnceExactly();
        A.CallTo(() => shard.Load(A<IRepository>.Ignored, A<bool>.Ignored)).MustHaveHappenedOnceExactly();

        A.CallTo(() => _repo!.BeginTransaction()).MustHaveHappened(2, Times.Exactly);
        A.CallTo(() => _transaction!.Commit()).MustHaveHappened(2, Times.Exactly);
        A.CallTo(() => _transaction!.Rollback()).MustNotHaveHappened();
    }

    [Test]
    public void LazyLoadModelTest()
    {
        var migration1 = A.Fake<IMigration>();
        var migration2 = A.Fake<IMigration>();
        A.CallTo(() => migration1.Version).Returns(1);
        A.CallTo(() => migration2.Version).Returns(2);
        var storage = new SqliteStorage("", [migration1, migration2], _factory!);
        var loader = A.Fake<ILazyLoader>();

        storage.Load(loader);

        A.CallTo(() => _repo!.GetDatabaseVersion()).MustHaveHappenedOnceExactly();
        A.CallTo(() => _repo!.SetDatabaseVersion(1)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _repo!.SetDatabaseVersion(2)).MustHaveHappenedOnceExactly();
        A.CallTo(() => loader.Load(A<IRepository>.Ignored)).MustHaveHappenedOnceExactly();

        A.CallTo(() => _repo!.BeginTransaction()).MustHaveHappened(2, Times.Exactly);
        A.CallTo(() => _transaction!.Commit()).MustHaveHappened(2, Times.Exactly);
        A.CallTo(() => _transaction!.Rollback()).MustNotHaveHappened();
    }

    [Test]
    public void LoadHistoryTest()
    {
        var storage = new SqliteStorage("", [], _factory!);
        var shard = A.Fake<IModelShard>();

        var changes = storage.Load([shard]);

        A.CallTo(() => _repo!.ExecuteNonQuery(A<string>.Ignored)).MustHaveHappenedTwiceExactly();
        A.CallTo(() => _repo!.RestoreHistory(A<IEnumerable<IModelShard>>.Ignored)).MustHaveHappenedOnceExactly();
    }
}
