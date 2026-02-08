namespace CoreCraft.Core;

/// <summary>
/// 
/// </summary>
public abstract class Interceptor
{
    /// <summary>
    /// 
    /// </summary>
    public virtual void BeforeCommand() { }

    /// <summary>
    /// 
    /// </summary>
    public virtual void AfterCommand() { }

    /// <summary>
    /// 
    /// </summary>
    public virtual void CommandFailed(Exception ex) { }

    /// <summary>
    /// 
    /// </summary>
    public virtual void BeforeApply() { }

    /// <summary>
    /// 
    /// </summary>
    public virtual void AfterApply() { }

    /// <summary>
    /// 
    /// </summary>
    public virtual void ApplyFailed(Exception ex) { }
}
