using System.Windows;

namespace WpfDemoApp
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected async override void OnStartup(StartupEventArgs e)
        {
            await UserSettings.LoadAsync();
            base.OnStartup(e);
        }
    }
}
