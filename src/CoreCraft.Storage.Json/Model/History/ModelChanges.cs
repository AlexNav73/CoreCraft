namespace CoreCraft.Storage.Json.Model.History;

internal sealed class ModelChanges
{
    public ModelChanges()
    {
        Frames = new List<ChangesFrame>();
    }

    public long Timestamp { get; set; }

    public IList<ChangesFrame> Frames { get; set; }
}
