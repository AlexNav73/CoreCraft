namespace Navitski.Crystalized.Model.Engine.Core;

public interface IMutableRelation<TParent, TChild> : IRelation<TParent, TChild>
    where TParent : Entity
    where TChild : Entity
{
    void Add(TParent parent, TChild child);

    void Remove(TParent parent, TChild child);
}
