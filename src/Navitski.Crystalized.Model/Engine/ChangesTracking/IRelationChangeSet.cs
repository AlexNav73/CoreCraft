﻿namespace Navitski.Crystalized.Model.Engine.ChangesTracking;

public interface IRelationChangeSet<TParent, TChild> : IEnumerable<IRelationChange<TParent, TChild>>
    where TParent : Entity
    where TChild : Entity
{
    void Add(RelationAction action, TParent parent, TChild child);

    IRelationChangeSet<TParent, TChild> Invert();

    void Apply(IMutableRelation<TParent, TChild> relation);

    bool HasChanges();
}