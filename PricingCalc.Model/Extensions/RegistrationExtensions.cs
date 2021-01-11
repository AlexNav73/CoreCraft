using Autofac;
using PricingCalc.Model.AppModel;
using PricingCalc.Model.UserSettings;

namespace PricingCalc.Model.Extensions
{
    public static class RegistrationExtensions
    {
        public static void RegisterApplicationModelShard<TShard>(this ContainerBuilder builder)
            where TShard : class, IApplicationModelShard
        {
            builder.RegisterType<TShard>().As<IApplicationModelShard>();
        }

        public static void RegisterUserSettingsModelShard<TShard>(this ContainerBuilder builder)
            where TShard : class, IUserSettingsModelShard
        {
            builder.RegisterType<TShard>().As<IUserSettingsModelShard>();
        }
    }
}
