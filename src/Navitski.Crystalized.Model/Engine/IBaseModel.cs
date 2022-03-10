namespace Navitski.Crystalized.Model.Engine;

public interface IBaseModel : IModelShardAccessor
{
    IDisposable Subscribe(Action<ModelChangedEventArgs> onModelChanges);
}
