using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PricingCalc.Model.Engine.Core;

namespace PricingCalc.Model.Engine
{
    internal class Model : IModel
    {
        private readonly IReadOnlyCollection<IModelShard> _shards;

        public Model(IReadOnlyCollection<IModelShard> shards)
        {
            _shards = shards;
        }

        public T Shard<T>() where T : IModelShard
        {
            return _shards.OfType<T>().Single();
        }

        public IEnumerator<IModelShard> GetEnumerator()
        {
            return _shards.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
