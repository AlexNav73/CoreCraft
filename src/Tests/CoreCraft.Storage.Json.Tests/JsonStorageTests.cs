﻿using CoreCraft.Persistence;
using CoreCraft.Storage.Json.Model;
using Newtonsoft.Json;

namespace CoreCraft.Storage.Json.Tests;

public class JsonStorageTests
{
    [Test]
    public void CtorTest()
    {
        Assert.DoesNotThrow(() => new JsonStorage());
    }

    [Test]
    public void UpdateTest()
    {
        var change = A.Fake<ICanBeSaved>();
        var jsonFileHandler = A.Fake<IJsonFileHandler>();
        var storage = new JsonStorage(jsonFileHandler);

        A.CallTo(() => jsonFileHandler.ReadModelShardsFromFile(A<string>.Ignored, A<JsonSerializerSettings>.Ignored))
            .Returns(new List<ModelShard>());

        storage.Update("", new[] { change });

        A.CallTo(() => jsonFileHandler.ReadModelShardsFromFile(A<string>.Ignored, A<JsonSerializerSettings>.Ignored))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => change.Save(A<IRepository>.Ignored))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => jsonFileHandler.WriteModelShardsToFile(A<string>.Ignored, A<IList<ModelShard>>.Ignored, A<JsonSerializerSettings>.Ignored))
            .MustHaveHappenedOnceExactly();
    }

    [Test]
    public void SaveTest()
    {
        var change = A.Fake<ICanBeSaved>();
        var jsonFileHandler = A.Fake<IJsonFileHandler>();
        var storage = new JsonStorage(jsonFileHandler);

        storage.Save("", new[] { change });

        A.CallTo(() => change.Save(A<IRepository>.Ignored))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => jsonFileHandler.WriteModelShardsToFile(A<string>.Ignored, A<IList<ModelShard>>.Ignored, A<JsonSerializerSettings>.Ignored))
            .MustHaveHappenedOnceExactly();
    }

    [Test]
    public void LoadTest()
    {
        var loadable = A.Fake<ICanBeLoaded>();
        var jsonFileHandler = A.Fake<IJsonFileHandler>();
        var storage = new JsonStorage(jsonFileHandler);

        storage.Load("", new[] { loadable });

        A.CallTo(() => jsonFileHandler.ReadModelShardsFromFile(A<string>.Ignored, A<JsonSerializerSettings>.Ignored))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => loadable.Load(A<IRepository>.Ignored))
            .MustHaveHappenedOnceExactly();
    }
}
