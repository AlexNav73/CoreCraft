namespace PricingCalc.Model.Engine.ChangesTracking
{
    public interface IWritableChangesFrame : IChangesFrame
    {
        void Apply(IModel model);

        IWritableChangesFrame Invert();
    }
}
