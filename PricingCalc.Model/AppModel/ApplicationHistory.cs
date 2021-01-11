using System;
using System.Collections.Generic;
using System.Linq;
using PricingCalc.Core;
using PricingCalc.Model.Engine;
using PricingCalc.Model.Engine.Core;
using PricingCalc.Model.Engine.Persistence;

namespace PricingCalc.Model.AppModel
{
    internal class ApplicationHistory : DisposableBase, IApplicationHistory
    {
        private readonly IView _view;
        private readonly IStorage _storage;
        private readonly Stack<IWritableModelChanges> _undoStack;
        private readonly Stack<IWritableModelChanges> _redoStack;

        public ApplicationHistory(IView view, IStorage storage)
        {
            _view = view;
            _storage = storage;
            _undoStack = new Stack<IWritableModelChanges>();
            _redoStack = new Stack<IWritableModelChanges>();

            _view.Changed += OnModelChanged;
        }

        public event EventHandler? Changed;

        public void Save(string path)
        {
            var changes = _undoStack.Reverse().ToArray();

            try
            {
                _storage.Save(path, _view.UnsafeModel, changes);

                _redoStack.Clear();
                _undoStack.Clear();
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error occurred while saving changes to {Path}", path);
            }

            Changed?.Invoke(this, EventArgs.Empty);
        }

        public void Load(string path)
        {
            _view.Changed -= OnModelChanged;
            try
            {
                _view.Mutate(snapshot => _storage.Load(path, snapshot));
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error occurred while model loading from {Path}", path);
            }
            _view.Changed += OnModelChanged;
        }

        public void Clear()
        {
            _undoStack.Clear();
            _redoStack.Clear();

            Changed?.Invoke(this, EventArgs.Empty);
        }

        public void Undo()
        {
            if (_undoStack.Count > 0)
            {
                _view.Changed -= OnModelChanged;

                var changes = _undoStack.Pop();
                _redoStack.Push(changes);
                _view.Apply(changes.Invert());

                Changed?.Invoke(this, EventArgs.Empty);
                _view.Changed += OnModelChanged;
            }
        }

        public void Redo()
        {
            if (_redoStack.Count > 0)
            {
                _view.Changed -= OnModelChanged;

                var changes = _redoStack.Pop();
                _undoStack.Push(changes);
                _view.Apply(changes);

                Changed?.Invoke(this, EventArgs.Empty);
                _view.Changed += OnModelChanged;
            }
        }

        public bool HasChanges()
        {
            return _undoStack.Count > 0;
        }

        private void OnModelChanged(object? sender, ModelChangedEventArgs e)
        {
            _undoStack.Push((IWritableModelChanges)e.Changes);
            Changed?.Invoke(this, EventArgs.Empty);
        }

        protected override void DisposeManagedObjects()
        {
            _view.Changed -= OnModelChanged;
        }
    }
}
