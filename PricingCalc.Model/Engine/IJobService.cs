using System;
using System.Threading.Tasks;

namespace PricingCalc.Model.Engine
{
    public interface IJobService
    {
        Task StartNew<T>(Func<T> job, Action<T> continueWith);

        Task StartNew(Action job, Action continueWith);
    }
}
