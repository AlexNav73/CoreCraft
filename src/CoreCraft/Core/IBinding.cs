using CoreCraft.ChangesTracking;

namespace CoreCraft.Core;

/// <summary>
/// 
/// </summary>
public interface IBinding<TEntity, TProperties> : IHasEntity<TEntity>
    where TEntity : Entity
    where TProperties : Properties
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="change"></param>
    void Update(IEntityChange<TEntity, TProperties> change);
}
