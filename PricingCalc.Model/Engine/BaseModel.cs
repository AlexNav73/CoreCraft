using System;
using System.Collections.Generic;
using System.Linq;
using PricingCalc.Core;
using PricingCalc.Model.Engine.Core;

namespace PricingCalc.Model.Engine
{
    internal abstract class BaseModel : DisposableBase, IBaseModel
    {
        private readonly IView _view;
        private readonly HashSet<Action<ModelChangedEventArgs>> _subscriptions;

        private volatile ModelChangedEventArgs? _currentChanges;

        protected BaseModel(IView view)
        {
            _subscriptions = new HashSet<Action<ModelChangedEventArgs>>();
            _view = view;

            view.Changed += OnModelChanged;
        }

        internal IView View => _view;

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

        private void OnModelChanged(object? sender, ModelChangedEventArgs e)
        {
            _currentChanges = e;

            var observers = _subscriptions.ToArray();
            foreach (var observer in observers)
            {
                observer(_currentChanges);
            }

            _currentChanges = null;
        }

        protected override void DisposeManagedObjects()
        {
            _view.Changed -= OnModelChanged;
        }
    }
}
