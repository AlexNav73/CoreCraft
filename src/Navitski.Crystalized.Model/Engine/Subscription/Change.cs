namespace Navitski.Crystalized.Model.Engine.Subscription;

/// <summary>
///     A container for changes
/// </summary>
/// <typeparam name="T">A type of changes</typeparam>
/// <param name="OldModel">The old version of the domain model</param>
/// <param name="NewModel">The new version of the domain model</param>
/// <param name="Hunk">A hunk of changes between old and new models</param>
public sealed record Change<T>(IModel OldModel, IModel NewModel, T Hunk);
