namespace PricingCalc.Model.Engine.ChangesTracking;

public interface ITrackableModelShard
{
    IModelShard AsTrackable(IWritableModelChanges modelChanges);
}
