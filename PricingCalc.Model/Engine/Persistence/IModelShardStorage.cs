using PricingCalc.Model.Engine.Core;

namespace PricingCalc.Model.Engine.Persistence
{
    public interface IModelShardStorage
    {
        void Save(string path, IRepository repository, IModelChanges changes);

        void Load(string path, IRepository repository, IModel model);
    }
}
