namespace CoreCraft.Storage.Json.Model.History;

internal sealed class ModelChanges
{
    public ModelChanges()
    {
        Frames = new List<ChangesFrame>();
    }

    public ModelChanges(long timestamp) : this()
    {
        Timestamp = timestamp;
    }

    public long Timestamp { get; set; }

    public IList<ChangesFrame> Frames { get; set; }

    public ChangesFrame Create(string name)
    {
        var frame = new ChangesFrame(name);
        Frames.Add(frame);
        return frame;
    }
}
