namespace Navitski.Crystalized.Model.Engine.ChangesTracking;

internal class ModelChanges : IModelChanges
{
    protected readonly IDictionary<Type, IChangesFrame> Frames;

    public ModelChanges()
        : this(new Dictionary<Type, IChangesFrame>())
    {
    }

    protected ModelChanges(IDictionary<Type, IChangesFrame> frames)
    {
        Frames = frames;
    }

    public bool TryGetFrame<T>(out T? frame)
        where T : class, IChangesFrame
    {
        if (Frames.TryGetValue(typeof(T), out var f))
        {
            frame = (T)f;
            return true;
        }

        frame = null;
        return false;
    }

    public bool HasChanges()
    {
        return Frames.Values.Any(x => x.HasChanges());
    }
}
