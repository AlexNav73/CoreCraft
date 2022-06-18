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

        return frame != null;
    }

    /// <inheritdoc />
    public T Register<T>(T changesFrame) where T : class, IWritableChangesFrame
    {
        var frame = _frames.OfType<T>().SingleOrDefault();
        if (frame != null)
        {
            return frame;
        }

        _frames.Add(changesFrame);
        return changesFrame;
    }

    /// <inheritdoc />
    public IWritableModelChanges Invert()
    {
        var frames = _frames.Cast<IWritableChangesFrame>().Select(x => x.Invert()).ToArray();

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

    /// <inheritdoc />
    public bool HasChanges()
    {
        return _frames.Any(x => x.HasChanges());
    }
}
