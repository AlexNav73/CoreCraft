using PricingCalc.Model.Engine.Core;
using System;
using System.Collections.Generic;

namespace PricingCalc.Model.Extensions
{
    internal static class CollectionsExtensions
    {
        public static IList<T> Copy<T>(this IList<T> collection)
            where T : ICopy<T>
        {
            var copy = new List<T>(collection.Count);
            for (int i = 0; i < collection.Count; i++)
            {
                copy.Add(collection[i].Copy());
            }
            return copy;
        }

        public static IDictionary<Guid, int> Copy(this IDictionary<Guid, int> dictionary)
        {
            var copy = new Dictionary<Guid, int>();
            foreach (var pair in dictionary)
            {
                copy[pair.Key] = pair.Value;
            }
            return copy;
        }
    }
}
