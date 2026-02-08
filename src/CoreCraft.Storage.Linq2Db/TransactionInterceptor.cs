using CoreCraft.Core;
using LinqToDB.Data;

namespace CoreCraft.Storage.Linq2Db;

public sealed class TransactionInterceptor(DataConnection db) : Interceptor
{
    private DataConnectionTransaction? _transaction;

    public override void BeforeCommand()
    {
        _transaction = db.BeginTransaction();
    }

    public override void AfterCommand()
    {
        _transaction?.Commit();
        _transaction?.Dispose();
        _transaction = null;
    }

    public override void CommandFailed(Exception ex)
    {
        _transaction?.Rollback();
        _transaction?.Dispose();
        _transaction = null;
    }

    public override void BeforeApply()
    {
        _transaction = db.BeginTransaction();
    }

    public override void AfterApply()
    {
        _transaction?.Commit();
        _transaction?.Dispose();
        _transaction = null;
    }

    public override void ApplyFailed(Exception ex)
    {
        _transaction?.Rollback();
        _transaction?.Dispose();
        _transaction = null;
    }
}
