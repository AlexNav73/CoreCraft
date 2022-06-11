using Navitski.Crystalized.Model.Engine.Exceptions;

namespace Navitski.Crystalized.Model.Engine.ChangesTracking;

/// <inheritdoc cref="IWritableModelChanges"/>
internal sealed class WritableModelChanges : ModelChanges, IWritableModelChanges
{
    public WritableModelChanges()
    {
    }

    private WritableModelChanges(IDictionary<Type, IChangesFrame> frames)
        : base(frames)
    {
    }

    /// <inheritdoc />
    public T Register<T>(T changesFrame) where T : class, IWritableChangesFrame
    {
        if (Frames.ContainsKey(typeof(T)))
        {
            throw new ChangesFrameRegistrationException($"The changes frame [{changesFrame.GetType().Name}] already registered");
        }

        Frames.Add(typeof(T), changesFrame);
        return changesFrame;
    }

    /// <inheritdoc />
    public IWritableModelChanges Invert()
    {
        var frames = Frames
            .Select(x => (type: x.Key, frame: ((IWritableChangesFrame)x.Value).Invert()))
            .ToDictionary(k => k.type, v => (IChangesFrame)v.frame);

        return new WritableModelChanges(frames);
    }

    /// <inheritdoc />
    public void Apply(IModel model)
    {
        foreach (var frame in Frames.Values.Cast<IWritableChangesFrame>())
        {
            frame.Apply(model);
        }
    }
}
