namespace Navitski.Crystalized.Model.Engine.Subscription;

/// <summary>
///     A container for changes
/// </summary>
/// <typeparam name="T">A type of changes</typeparam>
/// <param name="OldModel">The old version of the domain model</param>
/// <param name="NewModel">The new version of the domain model</param>
/// <param name="Hunk">A hunk of changes between old and new models</param>
public sealed record Change<T>(IModel OldModel, IModel NewModel, T Hunk)
{
    /// <summary>
    ///     Maps changes in the <see cref="Hunk"/> to another type keeping models with it
    /// </summary>
    /// <typeparam name="U">New type of the <see cref="Hunk"/></typeparam>
    /// <param name="mapper">A mapping function</param>
    /// <returns>Mapped changes from <typeparamref name="T"/> type to <typeparamref name="U"/></returns>
    public Change<U> Map<U>(Func<T, U> mapper)
    {
        return new Change<U>(OldModel, NewModel, mapper(Hunk));
    }
}
