namespace PricingCalc.Model.Engine.Persistence
{
    public abstract class Migration : IMigration
    {
        protected Migration(long timestamp)
        {
            Timestamp = timestamp;
        }

        public long Timestamp { get; }

        public abstract void Migrate(IMigrator migrator);
    }
}
