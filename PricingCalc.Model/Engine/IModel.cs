using System.Collections.Generic;
using PricingCalc.Model.Engine.Core;

namespace PricingCalc.Model.Engine
{
    public interface IModel : IEnumerable<IModelShard>
    {
        T Shard<T>() where T : IModelShard;
    }
}
