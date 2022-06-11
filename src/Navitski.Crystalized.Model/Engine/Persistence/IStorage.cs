using Navitski.Crystalized.Model.Engine.ChangesTracking;

namespace Navitski.Crystalized.Model.Engine.Persistence;

/// <summary>
///     A top level storage for a whole model
/// </summary>
/// <remarks>
///     The implementation of the interface will be responsible for storing and loading
///     data of the whole model. Inside a <see cref="IStorage"/> implementation all
///     model shards will be stored or loaded.
/// </remarks>
public interface IStorage
{
    /// <summary>
    ///     Migrates existing stored data by applying new changes to them
    /// </summary>
    /// <param name="path">A path to the file</param>
    /// <param name="model">A model with all the changes applied</param>
    /// <param name="changes">A collection of model changes</param>
    void Migrate(string path, IModel model, IReadOnlyList<IModelChanges> changes);

    /// <summary>
    ///     Saves the whole model
    /// </summary>
    /// <remarks>
    ///     Use this method to save model data to the empty storage (for example SQLite database)
    /// </remarks>
    /// <param name="path">A path to the file</param>
    /// <param name="model">A model to store</param>
    void Save(string path, IModel model);

    /// <summary>
    ///     Loads all data to the model
    /// </summary>
    /// <remarks>
    ///     After loading is done, all the data will be published as a changes, so
    ///     that the application can react on loaded data
    /// </remarks>
    /// <param name="path">A path to the file</param>
    /// <param name="model">A mutable model</param>
    void Load(string path, IModel model);
}
