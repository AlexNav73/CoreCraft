using System.Threading.Tasks;
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

            _model.ModelChanged += OnModelChanged;
        }

        public async Task Load()
        {
            await _model.Load(_userSettingsFile);
        }

        internal async void OnModelChanged(object? sender, ModelChangedEventArgs args)
        {
            await _model.Save(_userSettingsFile, new[] { args.Changes });
        }
    }
}
