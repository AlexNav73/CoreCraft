namespace PricingCalc.Model.Engine.ChangesTracking;

public interface ITrackableModelShard<out TShard> : IModelShard
    where TShard : IModelShard
{
    TShard AsTrackable(IWritableModelChanges modelChanges);
}
