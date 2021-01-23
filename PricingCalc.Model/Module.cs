using Autofac;
using PricingCalc.Model.Engine;
using PricingCalc.Model.Engine.Commands;

namespace PricingCalc.Model
{
    public class Module : Autofac.Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<CommandFactory>().As<ICommandFactory>();
            builder.RegisterType<JobService>().As<IJobService>();
        }
    }
}
