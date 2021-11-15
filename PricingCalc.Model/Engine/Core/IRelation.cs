namespace PricingCalc.Model.Engine.Core;

public interface IRelation<TParent, TChild> : IEnumerable<TParent>, ICopy<IRelation<TParent, TChild>>
    where TParent : Entity
    where TChild : Entity
{
    void Add(TParent parent, TChild child);

    void Remove(TParent parent, TChild child);

    IEnumerable<TChild> Children(TParent parent);

    IEnumerable<TParent> Parents(TChild child);
}
