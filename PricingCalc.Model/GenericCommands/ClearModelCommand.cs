using System.Linq;
using PricingCalc.Model.Engine;
using PricingCalc.Model.Engine.ChangesTracking;
using PricingCalc.Model.Engine.Commands;
using PricingCalc.Model.Engine.Commands.Runners;
using PricingCalc.Model.Engine.Core;

namespace PricingCalc.Model.GenericCommands
{
    internal class ClearModelCommand<TModel> : ModelCommand<TModel>, IClearModelCommand<TModel>
        where TModel : IBaseModel
    {
        public ClearModelCommand(TModel model, ISyncCommandRunner runner)
            : base(model, runner)
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
