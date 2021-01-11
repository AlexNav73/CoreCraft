using System.Collections.Generic;

namespace PricingCalc.Model.Engine.Core
{
    public interface IPropertiesBag : IEnumerable<KeyValuePair<string, object>>
    {
        void Write<T>(string name, T value);

        T Read<T>(string name);
    }
}
