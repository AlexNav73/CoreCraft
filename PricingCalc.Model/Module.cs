using Autofac;
using PricingCalc.Core.Extensions;
using PricingCalc.Model.AppModel;
using PricingCalc.Model.Engine;
using PricingCalc.Model.Engine.Commands;
using PricingCalc.Model.UserSettings;

namespace PricingCalc.Model
{
    public class Module : Autofac.Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<CommandFactory>().As<ICommandFactory>();
            builder.RegisterType<CommandRunner>().As<ICommandRunner>();
            builder.RegisterType<JobService>().As<IJobService>();

            builder.RegisterType<ApplicationModel>().As<IApplicationModel>().SingleInstance();
            builder.RegisterType<UserSettingsModel>().As<IUserSettingsModel>().SingleInstance();

            builder.RegisterStartupAction<LoadUserSettingsStartupAction>();
        }
    }
}
