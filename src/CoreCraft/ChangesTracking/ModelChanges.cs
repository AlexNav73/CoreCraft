// Ignore Spelling: timestamp

using System.Collections;
using CoreCraft.Persistence.History;
using CoreCraft.Persistence.Operations;

namespace CoreCraft.ChangesTracking;

/// <inheritdoc cref="IModelChanges" />
public sealed class ModelChanges : IMutableModelChanges
{
    private readonly HashSet<IChangesFrameEx> _frames;

    private readonly long _timestamp;

    /// <summary>
    ///     Ctor
    /// </summary>
    public ModelChanges(long timestamp)
        : this(timestamp, new HashSet<IChangesFrameEx>(ChangesFrameComparer.Instance))
    {
    }

    private ModelChanges(long timestamp, HashSet<IChangesFrameEx> frames)
    {
        _timestamp = timestamp;
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
    public TFrame AddOrGet<TFrame>(TFrame frame)
        where TFrame : IChangesFrame
    {
        if (frame is IChangesFrameEx ext && _frames.Add(ext))
        {
            return frame;
        }

        return _frames.OfType<TFrame>().Single();
    }

    /// <inheritdoc />
    public IModelChanges Invert()
    {
        var frames = _frames.Select(x => x.Invert()).Cast<IChangesFrameEx>();

        return new ModelChanges(_timestamp, new HashSet<IChangesFrameEx>(frames, ChangesFrameComparer.Instance));
    }

    /// <inheritdoc />
    public void Apply(IModel model)
    {
        foreach (var frame in _frames)
        {
            frame.Apply(model);
        }
    }

    /// <inheritdoc />
    public IModelChanges Merge(IModelChanges changes)
    {
        var changesFrames = changes.ToDictionary(k => k.GetType());
        var result = new HashSet<IChangesFrameEx>(ChangesFrameComparer.Instance);

        foreach (var frame in _frames)
        {
            if (changesFrames.TryGetValue(frame.GetType(), out var changesFrame))
            {
                result.Add((IChangesFrameEx)frame.Merge(changesFrame));
            }
        }

        return new ModelChanges(_timestamp, result);
    }

    /// <inheritdoc />
    public void Save(IHistoryRepository repository)
    {
        foreach (var frame in _frames)
        {
            frame.Do(new SaveChangesFrameOperation(_timestamp, repository));
        }
    }

    /// <inheritdoc />
    public bool HasChanges()
    {
        return _frames.Any(x => x.HasChanges());
    }

    /// <inheritdoc />
    public IEnumerator<IChangesFrame> GetEnumerator()
    {
        return _frames.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return _frames.GetEnumerator();
    }

    private sealed class ChangesFrameComparer : IEqualityComparer<IChangesFrame>
    {
        public static readonly ChangesFrameComparer Instance = new();

        public bool Equals(IChangesFrame? x, IChangesFrame? y)
        {
            return x != null && y != null && x.GetType() == y.GetType();
        }

        public int GetHashCode(IChangesFrame obj)
        {
            return obj.GetType().GetHashCode();
        }
    }
}
