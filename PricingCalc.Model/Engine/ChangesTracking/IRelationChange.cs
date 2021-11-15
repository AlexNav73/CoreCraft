namespace PricingCalc.Model.Engine.ChangesTracking;

public interface IRelationChange<TParent, TChild>
    where TParent : Entity
    where TChild : Entity
{
    RelationAction Action { get; }

    TParent Parent { get; }

    TChild Child { get; }

    IRelationChange<TParent, TChild> Invert();
}
