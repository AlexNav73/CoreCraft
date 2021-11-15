using PricingCalc.Model.Engine.ChangesTracking;

namespace PricingCalc.Model.Engine.Persistence;

public interface IModelShardStorage
{
    void Save(string path, IRepository repository, IModelChanges changes);

    void Save(string path, IRepository repository);

    void Load(string path, IRepository repository);
}
