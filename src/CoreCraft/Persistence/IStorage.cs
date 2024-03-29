﻿namespace CoreCraft.Persistence;

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
    ///     Updates existing stored data by applying new changes to them
    /// </summary>
    /// <param name="path">A path to the file</param>
    /// <param name="modelChanges">A model shards' changes happened since model creation or last save</param>
    void Update(string path, IEnumerable<ICanBeSaved> modelChanges);

    /// <summary>
    ///     Saves the whole model
    /// </summary>
    /// <remarks>
    ///     Use this method to save model data to the empty storage (for example SQLite database)
    /// </remarks>
    /// <param name="path">A path to the file</param>
    /// <param name="modelShards">A collection of model shards to store</param>
    void Save(string path, IEnumerable<ICanBeSaved> modelShards);

    /// <summary>
    ///     Loads all data to the model
    /// </summary>
    /// <remarks>
    ///     After loading is done, all the data will be published as a changes, so
    ///     that the application can react on loaded data
    /// </remarks>
    /// <param name="path">A path to the file</param>
    /// <param name="modelShards">A collection of model shards which can be loaded</param>
    void Load(string path, IEnumerable<ICanBeLoaded> modelShards);
}
