namespace CoreCraft.Features.CoW;

internal class CoWFeature : IFeature
{
    public IMutableCollection<TEntity, TProperties> Decorate<TEntity, TProperties>(
        IFrameFactory factory,
        IMutableCollection<TEntity, TProperties> collection)
        where TEntity : Entity
        where TProperties : Properties
    {
        return new CoWCollection<TEntity, TProperties>(collection);
    }

    public IMutableRelation<TParent, TChild> Decorate<TParent, TChild>(
        IFrameFactory factory,
        IMutableRelation<TParent, TChild> relation)
        where TParent : Entity
        where TChild : Entity
    {
        return new CoWRelation<TParent, TChild>(relation);
    }
}
