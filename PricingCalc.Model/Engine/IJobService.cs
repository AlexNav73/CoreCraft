using System;
using System.Threading.Tasks;

namespace PricingCalc.Model.Engine
{
    public interface IJobService
    {
        Task<T> Enqueue<T>(Func<T> job);

        Task Enqueue(Action job);
    }
}
