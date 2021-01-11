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

        public event EventHandler<ModelChangedEventArgs>? Changed;

        public void Mutate(Action<IModel> action)
        {
            var snapshot = new TrackableModel(_model);

            action(snapshot);

            var newModel = new Model(snapshot.GetShardsInternalUnsafe().ToList());
            var oldModel = Interlocked.Exchange(ref _model, newModel);
            if (snapshot.Changes.HasChanges())
            {
                Changed?.Invoke(this, new ModelChangedEventArgs(oldModel, newModel, snapshot.Changes));
            }
        }

        public void Apply(IWritableModelChanges changes)
        {
            if (changes.HasChanges())
            {
                var snapshot = new CachedModel(_model);

                changes.Apply(snapshot);

                var newModel = new Model(snapshot.ToList());
                var oldModel = Interlocked.Exchange(ref _model, newModel);

                Changed?.Invoke(this, new ModelChangedEventArgs(oldModel, newModel, changes));
            }
        }
    }
}
