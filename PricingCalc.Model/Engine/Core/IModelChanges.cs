using PricingCalc.Model.Engine.ChangesTracking;

namespace PricingCalc.Model.Engine.Core
{
    public interface IModelChanges
    {
        bool TryGetFrame<T>(out T frame)
            where T : class, IChangesFrame;

        bool HasChanges();
    }
}
