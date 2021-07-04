namespace PricingCalc.Model.Engine.Persistence
{
    public interface IMigration
    {
        string ModelSharedName { get; }

        Version Version { get; }

        void Migrate(IMigrator migrator);
    }
}
