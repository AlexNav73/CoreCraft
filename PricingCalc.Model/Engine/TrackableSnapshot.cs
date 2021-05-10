using System;
using System.Collections.Generic;
using PricingCalc.Model.Engine.ChangesTracking;
using PricingCalc.Model.Engine.Core;

namespace PricingCalc.Model.Engine
{
    internal class TrackableSnapshot : Snapshot
    {
        private readonly IDictionary<Type, IModelShard> _trackables;

        public TrackableSnapshot(IModel model) : base(model)
        {
            _trackables = new Dictionary<Type, IModelShard>();

            Changes = new WritableModelChanges();
        }

        public IWritableModelChanges Changes { get; }

        public override T Shard<T>()
        {
            if (_trackables.TryGetValue(typeof(T), out var shard))
            {
                return (T)shard;
            }

            var modelShard = (ITrackableModelShard)base.Shard<T>();
            var trackable = modelShard.AsTrackable(Changes);
            _trackables.Add(typeof(T), trackable);

            return (T)trackable;
        }

        public override IEnumerator<IModelShard> GetEnumerator()
        {
            var trackables = new List<IModelShard>();
            var baseEnumerator = base.GetEnumerator();

            while (baseEnumerator.MoveNext())
            {
                trackables.Add(((ITrackableModelShard)baseEnumerator.Current).AsTrackable(Changes));
            }

            return trackables.GetEnumerator();
        }
    }
}
