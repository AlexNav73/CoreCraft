namespace Navitski.Crystalized.Model.Engine.ChangesTracking;

/// <inheritdoc cref="IWritableModelChanges"/>
internal sealed class WritableModelChanges : ModelChanges, IWritableModelChanges
{
    public WritableModelChanges()
    {
    }

    private WritableModelChanges(IList<IChangesFrame> frames)
        : base(frames)
    {
    }

    /// <inheritdoc />
    public T Register<T>(T changesFrame) where T : class, IWritableChangesFrame
    {
        var frame = Frames.OfType<T>().SingleOrDefault();
        if (frame != null)
        {
            return frame;
        }

        Frames.Add(changesFrame);
        return changesFrame;
    }

    /// <inheritdoc />
    public IWritableModelChanges Invert()
    {
        var frames = Frames.Cast<IWritableChangesFrame>().Select(x => x.Invert()).ToArray();

        return new WritableModelChanges(frames);
    }

    /// <inheritdoc />
    public void Apply(IModel model)
    {
        foreach (var frame in Frames.Cast<IWritableChangesFrame>())
        {
            frame.Apply(model);
        }
    }
}
