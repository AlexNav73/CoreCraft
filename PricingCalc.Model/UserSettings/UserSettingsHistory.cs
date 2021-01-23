using System;
using System.Threading.Tasks;
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

        public async Task Load()
        {
            await _model.Load(_userSettingsFile);
        }

        internal async void OnModelChanged(ModelChangeResult result)
        {
            await _model.Save(_userSettingsFile, new[] { result.Changes });
        }
    }
}
