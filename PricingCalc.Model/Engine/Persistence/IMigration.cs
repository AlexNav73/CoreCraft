namespace PricingCalc.Model.Engine.Persistence
{
    public interface IMigration
    {
        long Timestamp { get; }

        void Migrate(IMigrator migrator);
    }
}
