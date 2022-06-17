namespace Navitski.Crystalized.Model.Engine.Core;

/// <summary>
///     A collection of model shards
/// </summary>
/// <remarks>
///     A <see cref="IModel"/> differs from <see cref="IDomainModel"/>
///     in a sense that <see cref="IDomainModel"/> is a entry point of the
///     whole domain model and it provides a lot of functionality. On the other
///     hand, <see cref="IModel"/> is just a container for <see cref="IModelShard"/>s
///     which passes to the commands or received in model changes notification handlers.
/// </remarks>
public interface IModel : IModelShardAccessor
{
}
