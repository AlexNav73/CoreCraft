using CoreCraft.ChangesTracking;

namespace CoreCraft.Subscription;

internal sealed class RelationSubscription<TChangesFrame, TParent, TChild> :
    Subscription<IRelationChangeSet<TParent, TChild>>,
    ISubscription<TChangesFrame>
    where TChangesFrame : class, IChangesFrame
    where TParent : Entity
    where TChild : Entity
{
    private readonly HashSet<DataView<TChangesFrame>> _views;

    public RelationSubscription(Func<TChangesFrame, IRelationChangeSet<TParent, TChild>> accessor)
    {
        _views = new();

        Accessor = accessor;
    }

    public Func<TChangesFrame, IRelationChangeSet<TParent, TChild>> Accessor { get; }

    public TView SubscribeView<TView>(TView view)
        where TView : DataView<TChangesFrame>
    {
        if (!_views.Add(view))
        {
            view = (TView)_views.Single(x => x.Equals(view));
        }
        else
        {
            view.Subscription = new UnsubscribeOnDispose<DataView<TChangesFrame>>(view, s => _views.Remove(s));
        }

        return view;
    }

    public void Publish(Change<TChangesFrame> change, bool forView = false)
    {
        var relationChangeSet = Accessor(change.Hunk);
        if (relationChangeSet.HasChanges())
        {
            if (forView)
            {
                foreach (var view in _views.ToList())
                {
                    view.OnNext(change);
                }
                return;
            }

            var msg = change.Map(_ => relationChangeSet);

            Publish(msg, false);
        }
    }
}
