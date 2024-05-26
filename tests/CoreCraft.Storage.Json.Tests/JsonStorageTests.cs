using CoreCraft.ChangesTracking;
using CoreCraft.Persistence;
using CoreCraft.Persistence.History;
using CoreCraft.Persistence.Lazy;
using CoreCraft.Persistence.Operations;
using Newtonsoft.Json;

namespace CoreCraft.Storage.Json.Tests;

public class JsonStorageTests
{
    [Test]
    public void CtorTest()
    {
        Assert.DoesNotThrow(() => new JsonStorage("test.json"));
    }

    [Test]
    public void UpdateTest()
    {
        var change = A.Fake<IChangesFrameEx>();
        var jsonFileHandler = A.Fake<IJsonFileHandler>();
        var storage = new JsonStorage("test.json", jsonFileHandler);

        A.CallTo(() => jsonFileHandler.ReadModelFromFile(A<string>.Ignored, A<JsonSerializerSettings>.Ignored))
            .Returns(new Model.Model());

        storage.Update([change]);

        A.CallTo(() => jsonFileHandler.ReadModelFromFile(A<string>.Ignored, A<JsonSerializerSettings>.Ignored))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => change.Do(A<UpdateChangesFrameOperation>.Ignored))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => jsonFileHandler.WriteModelToFile(A<string>.Ignored, A<Model.Model>.Ignored, A<JsonSerializerSettings>.Ignored))
            .MustHaveHappenedOnceExactly();
    }

    [Test]
    public void SaveTest()
    {
        var shard = A.Fake<IModelShard>();
        var jsonFileHandler = A.Fake<IJsonFileHandler>();
        var storage = new JsonStorage("test.json", jsonFileHandler);

        storage.Save([shard]);

        A.CallTo(() => shard.Save(A<IRepository>.Ignored))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => jsonFileHandler.WriteModelToFile(A<string>.Ignored, A<Model.Model>.Ignored, A<JsonSerializerSettings>.Ignored))
            .MustHaveHappenedOnceExactly();
    }

    [Test]
    public void SaveHistoryTest()
    {
        var change = A.Fake<IModelChanges>();
        var jsonFileHandler = A.Fake<IJsonFileHandler>();
        var storage = new JsonStorage("test.json", jsonFileHandler);

        storage.Save([change]);

        A.CallTo(() => change.Save(A<IHistoryRepository>.Ignored))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => jsonFileHandler.WriteModelToFile(A<string>.Ignored, A<Model.Model>.Ignored, A<JsonSerializerSettings>.Ignored))
            .MustHaveHappenedOnceExactly();
    }

    [Test]
    public void LoadTest()
    {
        var shard = A.Fake<IMutableModelShard>();
        var jsonFileHandler = A.Fake<IJsonFileHandler>();
        var storage = new JsonStorage("test.json", jsonFileHandler);

        storage.Load([shard]);

        A.CallTo(() => jsonFileHandler.ReadModelFromFile(A<string>.Ignored, A<JsonSerializerSettings>.Ignored))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => shard.Load(A<IRepository>.Ignored, A<bool>.Ignored))
            .MustHaveHappenedOnceExactly();
    }

    [Test]
    public void LoadHistoryTest()
    {
        var shard = A.Fake<IModelShard>(c => c.Implements<IFrameFactory>());
        var frame = A.Fake<IChangesFrame>(c => c.Implements<IChangesFrameEx>());
        var jsonFileHandler = A.Fake<IJsonFileHandler>();
        var storage = new JsonStorage("test.json", jsonFileHandler);

        var modelChanges = new Model.History.ModelChanges(0);
        var model = new Model.Model();
        model.ChangesHistory.Add(modelChanges);

        A.CallTo(() => frame.HasChanges()).Returns(true);
        A.CallTo(() => ((IFrameFactory)shard).Create())
            .Returns(frame);
        A.CallTo(() => jsonFileHandler.ReadModelFromFile(A<string>.Ignored, A<JsonSerializerSettings>.Ignored))
            .Returns(model);

        var changes = storage.Load([shard]).ToList();

        A.CallTo(() => jsonFileHandler.ReadModelFromFile(A<string>.Ignored, A<JsonSerializerSettings>.Ignored))
            .MustHaveHappenedOnceExactly();

        Assert.That(changes.Count, Is.EqualTo(1));
        Assert.That(ReferenceEquals(changes.Single().Single(), frame), Is.True);
    }

    [Test]
    public void LazyLoadTest()
    {
        var jsonFileHandler = A.Fake<IJsonFileHandler>();
        var storage = new JsonStorage("test.json", jsonFileHandler);
        var loader = A.Fake<ILazyLoader>();

        storage.Load(loader);

        A.CallTo(() => jsonFileHandler.ReadModelFromFile(A<string>.Ignored, A<JsonSerializerSettings>.Ignored))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => loader.Load(A<IRepository>.Ignored))
            .MustHaveHappenedOnceExactly();
    }
}
