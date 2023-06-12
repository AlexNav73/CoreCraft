using System.Runtime.CompilerServices;
using Navitski.Crystalized.Model.Engine.ChangesTracking;

namespace Navitski.Crystalized.Model.Engine.Subscription.Builders;

internal sealed class ModelShardSubscriptionBuilder<T> : IModelShardSubscriptionBuilder<T>
    where T : class, IChangesFrame
{
    private readonly ModelShardSubscription<T> _root;
    private readonly Change<T>? _changes;

    public ModelShardSubscriptionBuilder(ModelShardSubscription<T> root, Change<IModelChanges>? changes)
    {
        _root = root;
        _changes = Map(changes);
    }

    public ICollectionSubscriptionBuilder<TEntity, TProperties> With<TEntity, TProperties>(
        Func<T, ICollectionChangeSet<TEntity, TProperties>> accessor,
        [CallerArgumentExpression(nameof(accessor))] string expression = "")
        where TEntity : Entity
        where TProperties : Properties
    {
        return new CollectionSubscriptionBuilder<T, TEntity, TProperties>(_root.With(accessor, expression), _changes);
    }

    public IRelationSubscriptionBuilder<TParent, TChild> With<TParent, TChild>(
        Func<T, IRelationChangeSet<TParent, TChild>> accessor,
        [CallerArgumentExpression(nameof(accessor))] string expression = "")
        where TParent : Entity
        where TChild : Entity
    {
        return new RelationSubscriptionBuilder<T, TParent, TChild>(_root.With(accessor, expression), _changes);
    }

    public IDisposable Subscribe(IObserver<Change<T>> observer)
    {
        var subscription = _root.Subscribe(observer);

        if (_changes != null)
        {
            observer.OnNext(_changes);
        }

        return subscription;
    }

    private static Change<T>? Map(Change<IModelChanges>? changes)
    {
        if (changes != null && changes.Hunk.TryGetFrame<T>(out var frame) && frame.HasChanges())
        {
            return new Change<T>(changes.OldModel, changes.NewModel, frame);
        }

        return null;
    }
}
