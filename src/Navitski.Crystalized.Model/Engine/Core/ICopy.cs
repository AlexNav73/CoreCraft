namespace Navitski.Crystalized.Model.Engine.Core;

public interface ICopy<out T>
{
    T Copy();
}
