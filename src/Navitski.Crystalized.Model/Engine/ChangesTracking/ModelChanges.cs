using System.Collections;

namespace Navitski.Crystalized.Model.Engine.ChangesTracking;

internal sealed class ModelChanges : IWritableModelChanges
{
    private readonly IList<IChangesFrame> _frames;

    public ModelChanges()
        : this(new List<IChangesFrame>())
    {
    }

    private ModelChanges(IList<IChangesFrame> frames)
    {
        _frames = frames;
    }

    /// <inheritdoc />
    public bool TryGetFrame<T>(out T frame)
        where T : class, IChangesFrame
    {
        frame = _frames.OfType<T>().SingleOrDefault()!;

        return frame is not null;
    }

    /// <inheritdoc />
    public T Register<T>(Func<T> factory)
        where T : class, IWritableChangesFrame
    {
        if (TryGetFrame<T>(out var frame))
        {
            return frame;
        }

        var newFrame = factory();
        _frames.Add(newFrame);
        return newFrame;
    }

    /// <inheritdoc />
    public IModelChanges Invert()
    {
        var frames = _frames.Select(x => x.Invert()).ToArray();

        return new ModelChanges(frames);
    }

    /// <inheritdoc />
    public void Apply(IModel model)
    {
        foreach (var frame in _frames.Cast<IWritableChangesFrame>())
        {
            frame.Apply(model);
        }
    }

    public IWritableModelChanges Merge(IModelChanges changes)
    {
        var changesFrames = changes.ToDictionary(k => k.GetType());
        var result = new List<IChangesFrame>();

        foreach (var frame in _frames.Cast<IWritableChangesFrame>())
        {
            if (changesFrames.TryGetValue(frame.GetType(), out var changesFrame))
            {
                result.Add(frame.Merge(changesFrame));
            }
        }

        return new ModelChanges(result);
    }

    /// <inheritdoc />
    public bool HasChanges()
    {
        return _frames.Any(x => x.HasChanges());
    }

    public IEnumerator<IChangesFrame> GetEnumerator()
    {
        return _frames.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return _frames.GetEnumerator();
    }
}
