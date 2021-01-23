using System.Collections.Generic;
using System.Threading.Tasks;
using PricingCalc.Model.Engine.Core;

namespace PricingCalc.Model.Engine.Persistence
{
    public interface IStorage
    {
        Task Save(string path, IModel model, IReadOnlyList<IModelChanges> changes);

        void Load(string path, IModel model);
    }
}
