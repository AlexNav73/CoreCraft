using Navitski.Crystalized.Model.Engine.ChangesTracking;

namespace Navitski.Crystalized.Model.Engine.Features;

internal class TrackableFeature : IFeature
{
    private readonly IWritableModelChanges _modelChanges;

    public TrackableFeature(IWritableModelChanges modelChanges)
    {
        _modelChanges = modelChanges;
    }

    public IMutableCollection<TEntity, TProperties> Decorate<TEntity, TProperties>(
        IFeatureContext context,
        IMutableCollection<TEntity, TProperties> collection)
        where TEntity : Entity
        where TProperties : Properties
    {
        var frame = context.GetOrAddFrame(_modelChanges);
        var collectionChangesSet = frame.Get(collection);

        return new TrackableCollection<TEntity, TProperties>(collectionChangesSet, collection);
    }

    public IMutableRelation<TParent, TChild> Decorate<TParent, TChild>(
        IFeatureContext context,
        IMutableRelation<TParent, TChild> relation)
        where TParent : Entity
        where TChild : Entity
    {
        var frame = context.GetOrAddFrame(_modelChanges);
        var relationChangesSet = frame.Get(relation);

        return new TrackableRelation<TParent, TChild>(relationChangesSet, relation);
    }
}
