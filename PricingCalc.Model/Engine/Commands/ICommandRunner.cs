using System.Threading.Tasks;

namespace PricingCalc.Model.Engine.Commands
{
    internal interface ICommandRunner
    {
        Task Run(IRunnable runnable);
    }
}
