using System;
using System.Collections.Generic;
using System.Linq;
using PricingCalc.Core;
using PricingCalc.Model.Engine;
using PricingCalc.Model.Engine.Core;

namespace PricingCalc.Model.AppModel
{
    internal class ApplicationHistory : DisposableBase, IApplicationHistory
    {
        private readonly ApplicationModel _model;
        private readonly Stack<IWritableModelChanges> _undoStack;
        private readonly Stack<IWritableModelChanges> _redoStack;

        public ApplicationHistory(ApplicationModel model)
        {
            _model = model;
            _undoStack = new Stack<IWritableModelChanges>();
            _redoStack = new Stack<IWritableModelChanges>();
        }

        public event EventHandler? Changed;

        public void Save(string path)
        {
            var changes = _undoStack.Reverse().ToArray();

            _model.Save(path, changes, () =>
            {
                _redoStack.Clear();
                _undoStack.Clear();

                Changed?.Invoke(this, EventArgs.Empty);
            });
        }

        public void Load(string path)
        {
            _model.Load(path);
        }

        public void Clear()
        {
            _model.Clear(() =>
            {
                _undoStack.Clear();
                _redoStack.Clear();

                Changed?.Invoke(this, EventArgs.Empty);
            });
        }

        public void Undo()
        {
            if (_undoStack.Count > 0)
            {
                var changes = _undoStack.Pop();
                _redoStack.Push(changes);
                _model.Apply(
                    changes.Invert(),
                    () => Changed?.Invoke(this, EventArgs.Empty));
            }
        }

        public void Redo()
        {
            if (_redoStack.Count > 0)
            {
                var changes = _redoStack.Pop();
                _undoStack.Push(changes);
                _model.Apply(
                    changes,
                    () => Changed?.Invoke(this, EventArgs.Empty));
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
