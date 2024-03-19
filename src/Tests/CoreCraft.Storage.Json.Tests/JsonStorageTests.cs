using CoreCraft.ChangesTracking;
using CoreCraft.Persistence;
using CoreCraft.Persistence.Lazy;
using CoreCraft.Persistence.Operations;
using CoreCraft.Storage.Json.Model;
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

        A.CallTo(() => jsonFileHandler.ReadModelShardsFromFile(A<string>.Ignored, A<JsonSerializerSettings>.Ignored))
            .Returns(new List<ModelShard>());

        storage.Update([change]);

        A.CallTo(() => jsonFileHandler.ReadModelShardsFromFile(A<string>.Ignored, A<JsonSerializerSettings>.Ignored))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => change.Do(A<UpdateChangesFrameOperation>.Ignored))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => jsonFileHandler.WriteModelShardsToFile(A<string>.Ignored, A<IList<ModelShard>>.Ignored, A<JsonSerializerSettings>.Ignored))
            .MustHaveHappenedOnceExactly();
    }

    [Test]
    public void SaveTest()
    {
        var change = A.Fake<IModelShard>();
        var jsonFileHandler = A.Fake<IJsonFileHandler>();
        var storage = new JsonStorage("test.json", jsonFileHandler);

        storage.Save([change]);

        A.CallTo(() => change.Save(A<IRepository>.Ignored))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => jsonFileHandler.WriteModelShardsToFile(A<string>.Ignored, A<IList<ModelShard>>.Ignored, A<JsonSerializerSettings>.Ignored))
            .MustHaveHappenedOnceExactly();
    }

    [Test]
    public void LoadTest()
    {
        var loadable = A.Fake<IMutableModelShard>();
        var jsonFileHandler = A.Fake<IJsonFileHandler>();
        var storage = new JsonStorage("test.json", jsonFileHandler);

        storage.Load([loadable]);

        A.CallTo(() => jsonFileHandler.ReadModelShardsFromFile(A<string>.Ignored, A<JsonSerializerSettings>.Ignored))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => loadable.Load(A<IRepository>.Ignored, A<bool>.Ignored))
            .MustHaveHappenedOnceExactly();
    }

    [Test]
    public void LazyLoadTest()
    {
        var jsonFileHandler = A.Fake<IJsonFileHandler>();
        var storage = new JsonStorage("test.json", jsonFileHandler);
        var loader = A.Fake<ILazyLoader>();

        storage.Load(loader);

        A.CallTo(() => jsonFileHandler.ReadModelShardsFromFile(A<string>.Ignored, A<JsonSerializerSettings>.Ignored))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => loader.Load(A<IRepository>.Ignored))
            .MustHaveHappenedOnceExactly();
    }
}
