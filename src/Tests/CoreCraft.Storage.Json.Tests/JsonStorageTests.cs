using CoreCraft.ChangesTracking;
using CoreCraft.Persistence;
using CoreCraft.Storage.Json.Model;
using Newtonsoft.Json;

namespace CoreCraft.Storage.Json.Tests;

public class JsonStorageTests
{
    [Test]
    public void CtorTest()
    {
        var modelShardStorage = A.Fake<IModelShardStorage>();

        Assert.DoesNotThrow(() => new JsonStorage(new[] { modelShardStorage }));
    }

    [Test]
    public void UpdateTest()
    {
        var modelShardStorage = A.Fake<IModelShardStorage>();
        var jsonFileHandler = A.Fake<IJsonFileHandler>();
        var storage = new JsonStorage(new[] { modelShardStorage }, jsonFileHandler);

        A.CallTo(() => jsonFileHandler.ReadModelShardsFromFile(A<string>.Ignored, A<JsonSerializerSettings>.Ignored))
            .Returns(new List<ModelShard>());

        storage.Update("", A.Fake<IModelChanges>());

        A.CallTo(() => jsonFileHandler.ReadModelShardsFromFile(A<string>.Ignored, A<JsonSerializerSettings>.Ignored))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => modelShardStorage.Update(A<IRepository>.Ignored, A<IModelChanges>.Ignored))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => jsonFileHandler.WriteModelShardsToFile(A<string>.Ignored, A<IList<ModelShard>>.Ignored, A<JsonSerializerSettings>.Ignored))
            .MustHaveHappenedOnceExactly();
    }

    [Test]
    public void SaveTest()
    {
        var modelShardStorage = A.Fake<IModelShardStorage>();
        var jsonFileHandler = A.Fake<IJsonFileHandler>();
        var storage = new JsonStorage(new[] { modelShardStorage }, jsonFileHandler);

        storage.Save("", A.Fake<IModel>());

        A.CallTo(() => modelShardStorage.Save(A<IRepository>.Ignored, A<IModel>.Ignored))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => jsonFileHandler.WriteModelShardsToFile(A<string>.Ignored, A<IList<ModelShard>>.Ignored, A<JsonSerializerSettings>.Ignored))
            .MustHaveHappenedOnceExactly();
    }

    [Test]
    public void LoadTest()
    {
        var modelShardStorage = A.Fake<IModelShardStorage>();
        var jsonFileHandler = A.Fake<IJsonFileHandler>();
        var storage = new JsonStorage(new[] { modelShardStorage }, jsonFileHandler);

        storage.Load("", A.Fake<IModel>());

        A.CallTo(() => jsonFileHandler.ReadModelShardsFromFile(A<string>.Ignored, A<JsonSerializerSettings>.Ignored))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => modelShardStorage.Load(A<IRepository>.Ignored, A<IModel>.Ignored))
            .MustHaveHappenedOnceExactly();
    }
}
