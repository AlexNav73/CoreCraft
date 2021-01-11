using PricingCalc.Core.Startup;

namespace PricingCalc.Model.UserSettings
{
    internal class LoadUserSettingsStartupAction : IStartupAction
    {
        private readonly IUserSettingsModel _userSettings;

        public LoadUserSettingsStartupAction(IUserSettingsModel userSettings)
        {
            _userSettings = userSettings;
        }

        public StartupStage Stage => StartupStage.LoadUserSettings;

        public void Execute()
        {
            _userSettings.History.Load();
        }
    }
}
