using System.Collections.Generic;

namespace PricingCalc.Model.Engine.Core
{
    public interface IMapping<TParent, TChild> : IEnumerable<TParent>, ICopy<IMapping<TParent, TChild>>
        where TParent : IEntity
        where TChild : IEntity
    {
        void Add(TParent parent, TChild child);

        void Remove(TParent parent, TChild child);

        IEnumerable<TChild> Children(TParent parent);

        void Clear();
    }
}
