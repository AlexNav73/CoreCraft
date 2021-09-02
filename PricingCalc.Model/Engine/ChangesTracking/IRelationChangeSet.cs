using System.Collections.Generic;
using PricingCalc.Model.Engine.Core;

namespace PricingCalc.Model.Engine.ChangesTracking
{
    public interface IRelationChangeSet<TParent, TChild> : IEnumerable<IRelationChange<TParent, TChild>>
        where TParent : IEntity
        where TChild : IEntity
    {
        void Add(RelationAction action, TParent parent, TChild child);

        IRelationChangeSet<TParent, TChild> Invert();

        void Apply(IRelation<TParent, TChild> relation);

        bool HasChanges();
    }
}
