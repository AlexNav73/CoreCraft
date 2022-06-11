namespace Navitski.Crystalized.Model.Engine.Core;

/// <summary>
///     An entity is a sort of typed Id in collections or relations
///     by which properties or relations can be queried
/// </summary>
/// <param name="Id">An unique identifier for a given entity</param>
public abstract record Entity(Guid Id)
{
}
