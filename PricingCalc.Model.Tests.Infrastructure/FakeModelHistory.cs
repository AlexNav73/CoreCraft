using System;
using System.Collections.Generic;
using PricingCalc.Model.Engine;
using PricingCalc.Model.Engine.ChangesTracking;

namespace PricingCalc.Model.Tests.Infrastructure
{
    public class FakeModelHistory : DisposableBase
    {
        private readonly FakeModel _model;
        private readonly Stack<IWritableModelChanges> _undoStack;
        private readonly Stack<IWritableModelChanges> _redoStack;

        public FakeModelHistory(FakeModel model)
        {
            _model = model;

            _undoStack = new Stack<IWritableModelChanges>();
            _redoStack = new Stack<IWritableModelChanges>();

            _model.ModelChanged += OnModelChanged;
        }

        public event EventHandler? Changed;

        internal void OnModelChanged(object? sender, ModelChangedEventArgs args)
        {
            _undoStack.Push((IWritableModelChanges)args.Changes);
            Changed?.Invoke(this, EventArgs.Empty);
        }
    }
}
