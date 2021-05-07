using System;
using System.Threading.Tasks;

namespace PricingCalc.Model.Engine
{
    public interface IJobService
    {
        Task Enqueue(Action job);

        Task RunParallel(Action job);
    }
}
