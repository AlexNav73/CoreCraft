using PricingCalc.Model.Engine.ChangesTracking;

namespace PricingCalc.Model.Engine.Core
{
    public interface IWritableModelChanges : IModelChanges
    {
        T Add<T>(T newChanges) where T : IWritableChangesFrame;

        IWritableModelChanges Invert();

        void Apply(IModel model);
    }
}
