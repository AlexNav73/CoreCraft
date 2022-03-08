namespace PricingCalc.Model.Engine.ChangesTracking;

public interface IWritableModelChanges : IModelChanges
{
    T Add<T>(T newChanges) where T : IWritableChangesFrame;

    IWritableModelChanges Invert();

    void Apply(IModel model);
}
