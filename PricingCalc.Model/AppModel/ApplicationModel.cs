using System.Collections.Generic;
using PricingCalc.Model.Engine;
using PricingCalc.Model.Engine.Persistence;

namespace PricingCalc.Model.AppModel
{
    internal class ApplicationModel : BaseModel, IApplicationModel
    {
        public ApplicationModel(
            IReadOnlyCollection<IApplicationModelShard> shards,
            IStorage storage,
            IApplicationModelCommands commands)
            : base(new View(shards))
        {
            History = new ApplicationHistory(View, storage);
            Commands = commands;
        }

        public IApplicationHistory History { get; }

        public IApplicationModelCommands Commands { get; }
    }
}
