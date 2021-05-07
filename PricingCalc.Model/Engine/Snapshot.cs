using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PricingCalc.Model.Engine.Core;

namespace PricingCalc.Model.Engine
{
    internal class Snapshot : IModel
    {
        private readonly IModel _model;
        private readonly IList<IModelShard> _copies;

        public Snapshot(IModel model)
        {
            _model = model;
            _copies = new List<IModelShard>();
        }

        public virtual T Shard<T>() where T : IModelShard
        {
            var shard = _copies.OfType<T>().SingleOrDefault();
            if (shard != null)
            {
                return shard;
            }

            var modelShard = _model.Shard<T>();
            var copy = ((ICopy<T>)modelShard).Copy();
            _copies.Add(copy);

            return copy;
        }

        /// <summary>
        ///     [Unsafe] Returns model shards without copying if it is not necessary.
        ///     Changes of model shards, returned by this method, won't be tracked!
        ///     (If you want to track changes, use <see cref="GetEnumerator"/> instead)
        /// </summary>
        public IEnumerable<IModelShard> GetShardsInternalUnsafe()
        {
            var shards = _model
                .Where(x => !_copies.Any(k => k.GetType() == x.GetType()))
                .Union(_copies);

            return shards;
        }

        public IEnumerator<IModelShard> GetEnumerator()
        {
            var copies = _model
                .Where(x => !_copies.Any(k => k.GetType() == x.GetType()))
                .Select(x => ((ICopy<IModelShard>)x).Copy());

            foreach (var copy in copies)
            {
                _copies.Add(copy);
            }

            return _copies.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
