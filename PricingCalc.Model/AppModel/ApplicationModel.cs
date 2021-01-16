using System.Collections.Generic;
using PricingCalc.Model.Engine;
using PricingCalc.Model.Engine.Persistence;

namespace PricingCalc.Model.AppModel
{
    internal class ApplicationModel : BaseModel, IApplicationModel
    {
        private readonly ApplicationHistory _history;

        public ApplicationModel(
            IReadOnlyCollection<IApplicationModelShard> shards,
            IStorage storage,
            IApplicationModelCommands commands)
            : base(new View(shards))
        {
            _history = new ApplicationHistory(this, storage);
            Commands = commands;
        }

        public IApplicationHistory History => _history;

        public IApplicationModelCommands Commands { get; }

        internal override void RaiseEvent(ModelChangeResult result)
        {
            if (result.Changes.HasChanges())
            {
                RaiseModelChangesEvent(result);
                _history.OnModelChanged(result);
            }
        }
    }
}
