using System;
using PricingCalc.Core;
using PricingCalc.Model.Engine;

namespace PricingCalc.Model.UserSettings
{
    internal class UserSettingsHistory : IUserSettingsHistory
    {
        private const string _userSettingsFile = "UserSettings.user";

        private readonly UserSettingsModel _model;

        public UserSettingsHistory(UserSettingsModel model)
        {
            _model = model;
        }

        public void Load()
        {
            try
            {
                _model.Load(_userSettingsFile);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error occurred on User Settings loading");
            }
        }

        internal void OnModelChanged(ModelChangeResult result)
        {
            _model.Save(_userSettingsFile, new[] { result.Changes });
        }
    }
}
