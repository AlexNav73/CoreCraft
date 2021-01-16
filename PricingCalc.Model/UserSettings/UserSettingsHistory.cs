using System;
using PricingCalc.Core;
using PricingCalc.Model.Engine;
using PricingCalc.Model.Engine.Persistence;

namespace PricingCalc.Model.UserSettings
{
    internal class UserSettingsHistory : IUserSettingsHistory
    {
        private const string _userSettingsFile = "UserSettings.user";

        private readonly UserSettingsModel _model;
        private readonly IStorage _storage;

        public UserSettingsHistory(UserSettingsModel model, IStorage storage)
        {
            _model = model;
            _storage = storage;
        }

        public void Load()
        {
            try
            {
                _model.Mutate(model => _storage.Load(_userSettingsFile, model));
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error occurred on User Settings loading");
            }
        }

        internal void OnModelChanged(ModelChangeResult result)
        {
            _storage.Save(_userSettingsFile, _model.UnsafeModel, new[] { result.Changes });
        }
    }
}
