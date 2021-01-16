using System;
using System.Collections.Generic;
using System.Linq;
using PricingCalc.Model.Engine.Core;

namespace PricingCalc.Model.Engine
{
    internal abstract class BaseModel : IBaseModel
    {
        private readonly IView _view;
        private readonly HashSet<Action<ModelChangedEventArgs>> _subscriptions;

        private volatile ModelChangedEventArgs? _currentChanges;

        protected BaseModel(IView view)
        {
            _subscriptions = new HashSet<Action<ModelChangedEventArgs>>();
            _view = view;
        }

        internal IModel UnsafeModel => _view.UnsafeModel;

        public T Shard<T>() where T : IModelShard
        {
            return _view.UnsafeModel.Shard<T>();
        }

        public IDisposable Subscribe(Action<ModelChangedEventArgs> onModelChanges)
        {
            if (!_subscriptions.Contains(onModelChanges))
            {
                _subscriptions.Add(onModelChanges);
            }

            if (_currentChanges != null)
            {
                onModelChanges(_currentChanges);
            }

            return new UnsubscribeOnDispose(onModelChanges, _subscriptions);
        }

        internal ModelChangeResult Mutate(Action<IModel> action, bool notify = true)
        {
            var result = _view.Mutate(action);

            if (result.Changes.HasChanges() && notify)
            {
                RaiseModelChangesEvent(result);
            }

            return result;
        }

        internal void Apply(IWritableModelChanges changes)
        {
            if (changes.HasChanges())
            {
                var result = _view.Apply(changes);

                RaiseModelChangesEvent(result);
            }
        }

        internal abstract void RaiseEvent(ModelChangeResult result);

        protected void RaiseModelChangesEvent(ModelChangeResult result)
        {
            _currentChanges = new ModelChangedEventArgs(result.OldModel, result.NewModel, result.Changes);

            var observers = _subscriptions.ToArray();
            foreach (var observer in observers)
            {
                observer(_currentChanges);
            }

            _currentChanges = null;
        }
    }
}
