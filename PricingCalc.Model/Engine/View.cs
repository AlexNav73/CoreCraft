using System.Collections.Generic;
using System.Linq;
using System.Threading;
using PricingCalc.Model.Engine.ChangesTracking;
using PricingCalc.Model.Engine.Core;

namespace PricingCalc.Model.Engine
{
    internal sealed class View
    {
        private volatile IModel _model;

        public View(IEnumerable<IModelShard> shards)
        {
            _model = new Model(shards);
        }

        public IModel UnsafeModel => _model;

        public TrackableSnapshot CreateTrackableSnapshot()
        {
            return new TrackableSnapshot(_model);
        }

        public Snapshot CreateSnapshot()
        {
            return new Snapshot(_model);
        }

        public IModel CopyModel()
        {
            return new Model(_model.Select(x => ((ICopy<IModelShard>)x).Copy()));
        }

        public ModelChangeResult ApplySnapshot(Snapshot snapshot, IWritableModelChanges changes)
        {
            var newModel = new Model(snapshot.GetShardsInternalUnsafe());
            var oldModel = Interlocked.Exchange(ref _model, newModel);

            return new ModelChangeResult(oldModel, newModel, changes);
        }
    }
}
