using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using PricingCalc.Model.Engine.Core;

namespace PricingCalc.Model.Engine
{
    internal class View : IView
    {
        private volatile IModel _model;

        public View(IReadOnlyCollection<IModelShard> shards)
        {
            _model = new Model(shards);
        }

        public IModel UnsafeModel => _model;

        public MutateResult Mutate(Action<IModel> action)
        {
            var snapshot = new TrackableModel(_model);

            action(snapshot);

            var newModel = new Model(snapshot.GetShardsInternalUnsafe().ToList());
            var oldModel = Interlocked.Exchange(ref _model, newModel);

            return new MutateResult(oldModel, newModel, snapshot.Changes);
        }

        public MutateResult Apply(IWritableModelChanges changes)
        {
            var snapshot = new CachedModel(_model);

            changes.Apply(snapshot);

            var newModel = new Model(snapshot.GetShardsInternalUnsafe().ToList());
            var oldModel = Interlocked.Exchange(ref _model, newModel);

            return new MutateResult(oldModel, newModel, changes);
        }
    }
}
