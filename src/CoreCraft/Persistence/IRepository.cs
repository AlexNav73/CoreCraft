﻿using CoreCraft.ChangesTracking;

namespace CoreCraft.Persistence;

/// <summary>
///     An abstraction over some physical storage of the model data.
/// </summary>
/// <remarks>
///     To store or load model from file or a database <see cref="IRepository"/> is used.
///     <see cref="IRepository"/> provides methods to store and load for both collections and relations.
///     <see cref="CollectionInfo"/> is a description of a collection type. It contains a list of properties
///     with they names, types of value, null-ability flag and so one.
///     <see cref="RelationInfo"/> is a description of a relation type. It contains shard and relation names.
///     They are used instead of reflection when it's not clear, how to save or load entities and their properties.
/// </remarks>
public interface IRepository
{
    /// <summary>
    ///     Call this method for a specific collection of a model shard to update data
    /// </summary>
    /// <typeparam name="TEntity">A type of an entity</typeparam>
    /// <typeparam name="TProperties">A type of properties</typeparam>
    /// <param name="changes">Changes to apply</param>
    void Update<TEntity, TProperties>(
        ICollectionChangeSet<TEntity, TProperties> changes)
        where TEntity : Entity
        where TProperties : Properties;

    /// <summary>
    ///     Call this method for a specific relation of a model shard to update data
    /// </summary>
    /// <typeparam name="TParent">A type of a parent entity</typeparam>
    /// <typeparam name="TChild">A type of a child entity</typeparam>
    /// <param name="changes">Changes to apply</param>
    void Update<TParent, TChild>(
        IRelationChangeSet<TParent, TChild> changes)
        where TParent : Entity
        where TChild : Entity;

    /// <summary>
    ///     Call this method for a specific collection of a model shard to store all it's data
    /// </summary>
    /// <typeparam name="TEntity">A type of an entity</typeparam>
    /// <typeparam name="TProperties">A type of properties</typeparam>
    /// <param name="collection">A collection to store</param>
    void Save<TEntity, TProperties>(
        ICollection<TEntity, TProperties> collection)
        where TEntity : Entity
        where TProperties : Properties;

    /// <summary>
    ///     Call this method for a specific relation of a model shard to store all it's data
    /// </summary>
    /// <typeparam name="TParent">A type of a parent entity</typeparam>
    /// <typeparam name="TChild">A type of a child entity</typeparam>
    /// <param name="relation">A relation to store</param>
    void Save<TParent, TChild>(
        IRelation<TParent, TChild> relation)
        where TParent : Entity
        where TChild : Entity;

    /// <summary>
    ///     Call this method for a specific collection of a model shard to load all it's data
    /// </summary>
    /// <typeparam name="TEntity">A type of an entity</typeparam>
    /// <typeparam name="TProperties">A type of properties</typeparam>
    /// <param name="collection">A collection to store</param>
    void Load<TEntity, TProperties>(
        IMutableCollection<TEntity, TProperties> collection)
        where TEntity : Entity
        where TProperties : Properties;

    /// <summary>
    ///     Call this method for a specific relation of a model shard to load all it's data
    /// </summary>
    /// <typeparam name="TParent">A type of a parent entity</typeparam>
    /// <typeparam name="TChild">A type of a child entity</typeparam>
    /// <param name="relation">A relation to store</param>
    /// <param name="parents">A parent entities collection</param>
    /// <param name="children">A child entities collection</param>
    void Load<TParent, TChild>(
        IMutableRelation<TParent, TChild> relation,
        IEnumerable<TParent> parents,
        IEnumerable<TChild> children)
        where TParent : Entity
        where TChild : Entity;
}
