using System.Collections.Generic;

namespace PricingCalc.Model.Engine.Core
{
    public interface IRelation<TParent, TChild> : IEnumerable<TParent>
        where TParent : IEntity
        where TChild : IEntity
    {
        void Add(TParent parent, TChild child);

        void Remove(TParent parent, TChild child);

        IEnumerable<TChild> Children(TParent parent);

        IEnumerable<TParent> Parents(TChild child);

        void Clear();
    }
}
