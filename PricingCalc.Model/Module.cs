using Autofac;
using PricingCalc.Core.Extensions;
using PricingCalc.Model.AppModel;
using PricingCalc.Model.Engine.Commands;
using PricingCalc.Model.GenericCommands;
using PricingCalc.Model.UserSettings;

namespace PricingCalc.Model
{
    public class Module : Autofac.Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<CommandFactory>().As<ICommandFactory>();
            builder.RegisterType<InstantCommandRunner>().As<ICommandRunner>();

            builder.RegisterType<ApplicationModelCommands>().As<IApplicationModelCommands>();
            builder.RegisterGeneric(typeof(ClearModelCommand<>)).As(typeof(IClearModelCommand<>));

            builder.RegisterType<ApplicationModel>().As<IApplicationModel>().SingleInstance();
            builder.RegisterType<UserSettingsModel>().As<IUserSettingsModel>().SingleInstance();

            builder.RegisterStartupAction<LoadUserSettingsStartupAction>();
        }
    }
}
