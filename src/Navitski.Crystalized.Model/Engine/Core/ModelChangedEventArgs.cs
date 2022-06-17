using Navitski.Crystalized.Model.Engine.ChangesTracking;

namespace Navitski.Crystalized.Model.Engine.Core;

/// <summary>
///     A model changes args passed to the subscribers of the model
/// </summary>
public sealed class ModelChangedEventArgs
{
    internal ModelChangedEventArgs(IModel oldModel, IModel newModel, IModelChanges changes)
    {
        OldModel = oldModel;
        NewModel = newModel;
        Changes = changes;
    }

    /// <summary>
    ///     An old version of the model to provide an insight in which state model was
    /// </summary>
    public IModel OldModel { get; private set; }

    /// <summary>
    ///     A new version of the model after all <see cref="Changes"/> applied
    /// </summary>
    public IModel NewModel { get; private set; }

    /// <summary>
    ///     Difference between <see cref="OldModel"/> and <see cref="NewModel"/>
    /// </summary>
    public IModelChanges Changes { get; private set; }
}
