using System;
using System.Threading.Tasks;

namespace PricingCalc.Model.Engine
{
    public interface IJobService
    {
        Task<T> StartNew<T>(Func<T> job);

        Task StartNew(Action job);
    }
}
