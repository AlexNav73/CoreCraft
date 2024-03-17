using CoreCraft.ChangesTracking;

namespace CoreCraft.Features.Tracking;

internal class TrackableFeature : IFeature
{
    private readonly IMutableModelChanges _modelChanges;

    public TrackableFeature(IMutableModelChanges modelChanges)
    {
        _modelChanges = modelChanges;
    }

    public IMutableCollection<TEntity, TProperties> Decorate<TEntity, TProperties>(
        IFrameFactory factory,
        IMutableCollection<TEntity, TProperties> collection)
        where TEntity : Entity
        where TProperties : Properties
    {
        var frame = (IChangesFrameEx)_modelChanges.AddOrGet(factory.Create());
        var collectionChangesSet = frame.Get(collection);

        if (collectionChangesSet is not null)
        {
            return new TrackableCollection<TEntity, TProperties>(collectionChangesSet, collection);
        }

        return collection;
    }

    public IMutableRelation<TParent, TChild> Decorate<TParent, TChild>(
        IFrameFactory factory,
        IMutableRelation<TParent, TChild> relation)
        where TParent : Entity
        where TChild : Entity
    {
        var frame = (IChangesFrameEx)_modelChanges.AddOrGet(factory.Create());
        var relationChangesSet = frame.Get(relation);

        if (relationChangesSet is not null)
        {
            return new TrackableRelation<TParent, TChild>(relationChangesSet, relation);
        }

        return relation;
    }
}
