namespace Navitski.Crystalized.Model.Engine.Subscription;

/// <summary>
///     A container for changes
/// </summary>
/// <typeparam name="T">A type of changes</typeparam>
public sealed record Change<T>(IModel OldModel, IModel NewModel, T Hunk);
