namespace PricingCalc.Model.Storage.Sqlite.Migrations;

public interface IMigration
{
    long Timestamp { get; }

    void Migrate(IMigrator migrator);
}
