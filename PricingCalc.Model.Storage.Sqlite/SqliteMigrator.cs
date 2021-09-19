using PricingCalc.Model.Engine.Persistence;

namespace PricingCalc.Model.Storage.Sqlite
{
    internal class SqliteMigrator : IMigrator
    {
        private readonly SqliteRepository _repository;

        public SqliteMigrator(SqliteRepository repository)
        {
            _repository = repository;
        }

        public void DropTable(string name)
        {
            _repository.ExecuteNonQuery(QueryBuilder.DropTable(name));
        }
    }
}
