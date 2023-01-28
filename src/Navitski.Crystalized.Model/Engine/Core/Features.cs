namespace Navitski.Crystalized.Model.Engine.Core;

/// <summary>
/// 
/// </summary>
[Flags]
public enum Features
{
    /// <summary>
    /// 
    /// </summary>
    None = 0,
    /// <summary>
    /// 
    /// </summary>
    Copy = 1,
    /// <summary>
    /// 
    /// </summary>
    Track = 2 | Copy
}
