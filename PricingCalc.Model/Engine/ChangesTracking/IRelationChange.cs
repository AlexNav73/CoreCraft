using PricingCalc.Model.Engine.Core;

namespace PricingCalc.Model.Engine.ChangesTracking
{
    public interface IRelationChange<TParent, TChild>
        where TParent : IEntity
        where TChild : IEntity
    {
        RelationAction Action { get; }

        TParent Parent { get; }

        TChild Child { get; }

        IRelationChange<TParent, TChild> Invert();
    }
}
