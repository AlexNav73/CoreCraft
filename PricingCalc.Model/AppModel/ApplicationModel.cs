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
            : base(new View(shards), storage)
        {
            _history = new ApplicationHistory(this);
            Commands = commands;
        }

        public IApplicationHistory History => _history;

        public IApplicationModelCommands Commands { get; }

        public override void RaiseEvent(ModelChangeResult result)
        {
            if (result.Changes.HasChanges())
            {
                base.RaiseEvent(result);
                _history.OnModelChanged(result);
            }
        }
    }
}
