using System;
using System.IO;
using PricingCalc.Core;
using PricingCalc.Model.Engine;
using PricingCalc.Model.Engine.Persistence;

namespace PricingCalc.Model.UserSettings
{
    internal class UserSettingsHistory : DisposableBase, IUserSettingsHistory
    {
        private const string _userSettingsFile = "UserSettings.user";

        private readonly IView _view;
        private readonly IStorage _storage;

        public UserSettingsHistory(IView view, IStorage storage)
        {
            _view = view;
            _storage = storage;

            _view.Changed += OnModelChanged;
        }

        public void Load()
        {
            _view.Changed -= OnModelChanged;
            try
            {
                _view.Mutate(model => _storage.Load(_userSettingsFile, model));
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error occurred on User Settings loading");
            }
            _view.Changed += OnModelChanged;
        }

        private void OnModelChanged(object? sender, ModelChangedEventArgs e)
        {
            _storage.Save(_userSettingsFile, _view.UnsafeModel, new[] { e.Changes });
        }

        protected override void DisposeManagedObjects()
        {
            _view.Changed -= OnModelChanged;
        }
    }
}
