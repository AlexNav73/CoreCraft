using FakeItEasy;
using Navitski.Crystalized.Model.Engine.ChangesTracking;
using Navitski.Crystalized.Model.Engine.Persistence;
using Navitski.Crystalized.Model.Storage.Sqlite.Migrations;
using System.Data;

namespace Navitski.Crystalized.Model.Storage.Sqlite.Tests;

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
        var shardStorage = A.Fake<IModelShardStorage>();
        A.CallTo(() => shardStorage.Update(A<IRepository>.Ignored, A<IModelChanges>.Ignored))
            .Throws<InvalidOperationException>();
        var modelChanges = A.Fake<IModelChanges>(c => c.Implements<IWritableModelChanges>());

        var storage = new SqliteStorage(Array.Empty<IMigration>(), new[] { shardStorage }, _factory!);

        Assert.Throws<InvalidOperationException>(() => storage.Update("", modelChanges));

        A.CallTo(() => _repo!.BeginTransaction()).MustHaveHappenedOnceExactly();
        A.CallTo(() => _transaction!.Commit()).MustNotHaveHappened();
        A.CallTo(() => _transaction!.Rollback()).MustHaveHappenedOnceExactly();
    }

    [Test]
    public void UpdateIsCalledOnModelShardStorageTest()
    {
        var shardStorage = A.Fake<IModelShardStorage>();
        var storage = new SqliteStorage(Array.Empty<IMigration>(), new[] { shardStorage }, _factory!);
        var modelChanges = A.Fake<IModelChanges>(c => c.Implements<IWritableModelChanges>());

        storage.Update("", modelChanges);

        A.CallTo(() => shardStorage.Update(A<IRepository>.Ignored, A<IModelChanges>.Ignored))
            .MustHaveHappenedOnceExactly();
    }

    [Test]
    public void SaveTransactionCreatedSuccessfulyAndCommitedTest()
    {
        var storage = new SqliteStorage(Array.Empty<IMigration>(), Array.Empty<IModelShardStorage>(), _factory!);

        storage.Save("", A.Fake<IModel>());

        A.CallTo(() => _repo!.BeginTransaction()).MustHaveHappenedOnceExactly();
        A.CallTo(() => _transaction!.Commit()).MustHaveHappenedOnceExactly();
        A.CallTo(() => _transaction!.Rollback()).MustNotHaveHappened();
    }

    [Test]
    public void SaveTransactionRollbackOnExceptionTest()
    {
        var shardStorage = A.Fake<IModelShardStorage>();
        A.CallTo(() => shardStorage.Save(A<IRepository>.Ignored, A<IModel>.Ignored))
            .Throws<InvalidOperationException>();

        var storage = new SqliteStorage(Array.Empty<IMigration>(), new[] { shardStorage }, _factory!);

        Assert.Throws<InvalidOperationException>(() => storage.Save("", A.Fake<IModel>()));

        A.CallTo(() => _repo!.BeginTransaction()).MustHaveHappenedOnceExactly();
        A.CallTo(() => _transaction!.Commit()).MustNotHaveHappened();
        A.CallTo(() => _transaction!.Rollback()).MustHaveHappenedOnceExactly();
    }

    [Test]
    public void SaveIsCalledOnModelShardStorageTest()
    {
        var shardStorage = A.Fake<IModelShardStorage>();
        var storage = new SqliteStorage(Array.Empty<IMigration>(), new[] { shardStorage }, _factory!);

        storage.Save("", A.Fake<IModel>());

        A.CallTo(() => shardStorage.Save(A<IRepository>.Ignored, A<IModel>.Ignored))
            .MustHaveHappenedOnceExactly();
    }

    [Test]
    public void SaveMigrationRunnerUpdatedDatabaseVersionTest()
    {
        var shardStorage = A.Fake<IModelShardStorage>();
        var migration1 = A.Fake<IMigration>();
        var migration2 = A.Fake<IMigration>();
        A.CallTo(() => migration1.Version).Returns(1);
        A.CallTo(() => migration2.Version).Returns(2);
        var storage = new SqliteStorage(new[] { migration1, migration2 }, new[] { shardStorage }, _factory!);

        storage.Save("", A.Fake<IModel>());

        A.CallTo(() => _repo!.SetDatabaseVersion(2))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => shardStorage.Save(A<IRepository>.Ignored, A<IModel>.Ignored))
            .MustHaveHappenedOnceExactly();
    }

    [Test]
    public void LoadModelTest()
    {
        var shardStorage = A.Fake<IModelShardStorage>();
        var migration1 = A.Fake<IMigration>();
        var migration2 = A.Fake<IMigration>();
        A.CallTo(() => migration1.Version).Returns(1);
        A.CallTo(() => migration2.Version).Returns(2);
        var storage = new SqliteStorage(new[] { migration1, migration2 }, new[] { shardStorage }, _factory!);

        storage.Load("", A.Fake<IModel>());

        A.CallTo(() => _repo!.GetDatabaseVersion()).MustHaveHappenedOnceExactly();
        A.CallTo(() => _repo!.SetDatabaseVersion(1)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _repo!.SetDatabaseVersion(2)).MustHaveHappenedOnceExactly();
        A.CallTo(() => shardStorage.Load(A<IRepository>.Ignored, A<IModel>.Ignored))
            .MustHaveHappenedOnceExactly();

        A.CallTo(() => _repo!.BeginTransaction()).MustHaveHappened(2, Times.Exactly);
        A.CallTo(() => _transaction!.Commit()).MustHaveHappened(2, Times.Exactly);
        A.CallTo(() => _transaction!.Rollback()).MustNotHaveHappened();
    }
}
