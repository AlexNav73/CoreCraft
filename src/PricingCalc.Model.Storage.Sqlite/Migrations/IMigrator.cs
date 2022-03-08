namespace PricingCalc.Model.Storage.Sqlite.Migrations;

public interface IMigrator
{
    void DropTable(string name);
}
