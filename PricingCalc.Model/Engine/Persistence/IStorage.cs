using PricingCalc.Model.Engine.ChangesTracking;

namespace PricingCalc.Model.Engine.Persistence;

public interface IStorage
{
    void Save(string path, IModel model, IReadOnlyList<IModelChanges> changes);

    void Save(string path, IModel model);

    void Load(string path, IModel model);
}
