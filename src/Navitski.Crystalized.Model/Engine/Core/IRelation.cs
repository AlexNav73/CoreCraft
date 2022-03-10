namespace Navitski.Crystalized.Model.Engine.Core;

public interface IRelation<TParent, TChild> : IEnumerable<TParent>, ICopy<IRelation<TParent, TChild>>
    where TParent : Entity
    where TChild : Entity
{
    IEnumerable<TChild> Children(TParent parent);

    IEnumerable<TParent> Parents(TChild child);
}
