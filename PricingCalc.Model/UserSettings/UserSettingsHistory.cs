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
            _model.Load(_userSettingsFile);
        }

        internal void OnModelChanged(ModelChangeResult result)
        {
            _model.Save(_userSettingsFile, new[] { result.Changes }, () => { });
        }
    }
}
