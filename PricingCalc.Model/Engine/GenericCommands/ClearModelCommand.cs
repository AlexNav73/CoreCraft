using System.Linq;
using PricingCalc.Model.Engine.ChangesTracking;
using PricingCalc.Model.Engine.Commands;
using PricingCalc.Model.Engine.Core;

namespace PricingCalc.Model.Engine.GenericCommands
{
    internal class ClearModelCommand : ModelCommand<BaseModel>, IModelCommand
    {
        public ClearModelCommand(BaseModel model) : base(model)
        {
        }

        protected override void ExecuteInternal(IModel model)
        {
            var trackableModel = (TrackableModel)model;
            var writableShards = trackableModel
                .OfType<ITrackableModelShard>()
                .Select(x => x.AsTrackable(trackableModel.Changes))
                .Cast<IWritableModelShard>();

            foreach (var shard in writableShards)
            {
                shard.Clear();
            }
        }
    }
}
