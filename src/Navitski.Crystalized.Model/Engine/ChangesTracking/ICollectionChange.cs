namespace Navitski.Crystalized.Model.Engine.ChangesTracking;

public interface ICollectionChange<TEntity, TData>
    where TEntity : Entity
    where TData : Properties
{
    CollectionAction Action { get; }

    TEntity Entity { get; }

    TData? OldData { get; }

    TData? NewData { get; }

    ICollectionChange<TEntity, TData> Invert();
}
