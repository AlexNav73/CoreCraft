using System;
using System.Collections.Generic;
using System.Linq;
using PricingCalc.Model.Engine.Commands;
using PricingCalc.Model.Engine.Core;
using PricingCalc.Model.Engine.Persistence;

namespace PricingCalc.Model.Engine
{
    internal abstract class BaseModel : IBaseModel
    {
        private readonly IView _view;
        private readonly IStorage _storage;
        private readonly HashSet<Action<ModelChangedEventArgs>> _subscriptions;

        private volatile ModelChangedEventArgs? _currentChanges;

        protected BaseModel(IView view, IStorage storage)
        {
            _subscriptions = new HashSet<Action<ModelChangedEventArgs>>();
            _view = view;
            _storage = storage;
        }

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

        internal void Save(string path, IReadOnlyList<IModelChanges> changes)
        {
            _storage.Save(path, _view.UnsafeModel, changes);
        }

        internal void Load(string path)
        {
            var result = _view.Mutate(snapshot => _storage.Load(path, snapshot));
            if (result.Changes.HasChanges())
            {
                RaiseModelChangesEvent(result);
            }
        }

        internal void Apply(IWritableModelChanges changes)
        {
            if (changes.HasChanges())
            {
                var result = _view.Apply(changes);

                RaiseModelChangesEvent(result);
            }
        }

        internal ModelChangeResult Run(ModelCommand command)
        {
            return _view.Mutate(snapshot => command.Run(snapshot));
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
