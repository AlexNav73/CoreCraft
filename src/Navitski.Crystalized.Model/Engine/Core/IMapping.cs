namespace Navitski.Crystalized.Model.Engine.Core;

public interface IMapping<TParent, TChild> : IEnumerable<TParent>, ICopy<IMapping<TParent, TChild>>
    where TParent : Entity
    where TChild : Entity
{
    void Add(TParent parent, TChild child);

    void Remove(TParent parent, TChild child);

    IEnumerable<TChild> Children(TParent parent);

    void Clear();
}
