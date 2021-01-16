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
        private readonly ApplicationModel _model;
        private readonly IStorage _storage;
        private readonly Stack<IWritableModelChanges> _undoStack;
        private readonly Stack<IWritableModelChanges> _redoStack;

        public ApplicationHistory(ApplicationModel model, IStorage storage)
        {
            _model = model;
            _storage = storage;
            _undoStack = new Stack<IWritableModelChanges>();
            _redoStack = new Stack<IWritableModelChanges>();
        }

        public event EventHandler? Changed;

        public void Save(string path)
        {
            var changes = _undoStack.Reverse().ToArray();

            try
            {
                _storage.Save(path, _model.UnsafeModel, changes);

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
            try
            {
                _model.Mutate(snapshot => _storage.Load(path, snapshot));
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error occurred while model loading from {Path}", path);
            }
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
                var changes = _undoStack.Pop();
                _redoStack.Push(changes);
                _model.Apply(changes.Invert());

                Changed?.Invoke(this, EventArgs.Empty);
            }
        }

        public void Redo()
        {
            if (_redoStack.Count > 0)
            {
                var changes = _redoStack.Pop();
                _undoStack.Push(changes);
                _model.Apply(changes);

                Changed?.Invoke(this, EventArgs.Empty);
            }
        }

        public bool HasChanges()
        {
            return _undoStack.Count > 0;
        }

        internal void OnModelChanged(ModelChangeResult result)
        {
            _undoStack.Push(result.Changes);
            Changed?.Invoke(this, EventArgs.Empty);
        }
    }
}
