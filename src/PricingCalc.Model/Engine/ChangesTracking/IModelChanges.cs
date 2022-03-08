namespace PricingCalc.Model.Engine.ChangesTracking;

public interface IModelChanges
{
    bool TryGetFrame<T>(out T frame)
        where T : class, IChangesFrame;

    bool HasChanges();
}
