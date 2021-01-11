﻿using PricingCalc.Model.Engine.ChangesTracking;
using PricingCalc.Model.Engine.Core;

namespace PricingCalc.Model.Engine
{
    internal class TrackableModel : CachedModel
    {
        public TrackableModel(IModel model) : base(model)
        {
            Changes = new WritableModelChanges();
        }

        public IWritableModelChanges Changes { get; }

        public override T Shard<T>()
        {
            var modelShard = (ITrackableModelShard)base.Shard<T>();

            return (T)modelShard.AsTrackable(Changes);
        }
    }
}
