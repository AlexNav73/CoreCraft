namespace PricingCalc.Model.Engine.Persistence
{
    public interface IMigrator
    {
        void DropTable(string name);
    }
}
