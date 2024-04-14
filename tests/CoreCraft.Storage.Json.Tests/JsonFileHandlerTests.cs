using CoreCraft.Storage.Json.Model;
using Newtonsoft.Json;

namespace CoreCraft.Storage.Json.Tests;

public class JsonFileHandlerTests
{
    [Test]
    public async Task WriteModelShardsToFileTest()
    {
        var model = CreateModel();
        var jsonFileHandler = new JsonFileHandler();
        var setting = new JsonSerializerSettings();
        var file = "test1.json";

        jsonFileHandler.WriteModelToFile(file, model, setting);

        var json = File.ReadAllText(file);
        File.Delete(file);

        await Verify(json).UseDirectory("./VerifiedFiles");
    }

    [Test]
    public async Task WriteModelShardsToFilePreserveJsonSettingsTest()
    {
        var model = CreateModel();
        var jsonFileHandler = new JsonFileHandler();
        var setting = new JsonSerializerSettings()
        {
            Formatting = Formatting.Indented
        };
        var file = "test1.json";

        jsonFileHandler.WriteModelToFile(file, model, setting);

        var json = File.ReadAllText(file);
        File.Delete(file);

        await Verify(json).UseDirectory("./VerifiedFiles");
    }

    [Test]
    [TestCase(TypeNameHandling.All)]
    [TestCase(TypeNameHandling.None)]
    [TestCase(TypeNameHandling.Objects)]
    [TestCase(TypeNameHandling.Arrays)]
    [TestCase(TypeNameHandling.Auto)]
    public async Task WriteModelShardsToFileAlwaysEnableTypeNameHandlingTest(TypeNameHandling typeNameHandling)
    {
        var model = CreateModel();
        var jsonFileHandler = new JsonFileHandler();
        var setting = new JsonSerializerSettings()
        {
            Formatting = Formatting.Indented,
            TypeNameHandling = typeNameHandling
        };
        var file = "test1.json";

        jsonFileHandler.WriteModelToFile(file, model, setting);

        var json = File.ReadAllText(file);

        var readModel = jsonFileHandler.ReadModelFromFile(file, setting);

        File.Delete(file);

        Assert.That(readModel.Shards.Count, Is.EqualTo(1));

        var fileName = $"{nameof(JsonFileHandlerTests)}_{nameof(WriteModelShardsToFileAlwaysEnableTypeNameHandlingTest)}_{typeNameHandling}";
        await Verify(json).UseDirectory("./VerifiedFiles").UseFileName(fileName);
    }

    [Test]
    public void WriteModelShardsToFileThrowsOnInvalidTypeNameHandlingTest()
    {
        var model = CreateModel();
        var jsonFileHandler = new JsonFileHandler();
        var setting = new JsonSerializerSettings()
        {
            Formatting = Formatting.Indented,
            TypeNameHandling = (TypeNameHandling)55
        };
        var file = "test1.json";

        Assert.Throws<NotSupportedException>(() => jsonFileHandler.WriteModelToFile(file, model, setting));
    }

    [Test]
    public void ReadModelShardsFromFileTest()
    {
        var model = CreateModel();
        var jsonFileHandler = new JsonFileHandler();
        var setting = new JsonSerializerSettings();
        var file = "test2.json";

        jsonFileHandler.WriteModelToFile(file, model, setting);
        var json1 = File.ReadAllText(file);

        var readModel = jsonFileHandler.ReadModelFromFile(file, setting);
        jsonFileHandler.WriteModelToFile(file, readModel, setting);

        var json2 = File.ReadAllText(file);
        File.Delete(file);

        Assert.That(json1, Is.EqualTo(json2));
    }

    [Test]
    public void ReadModelShardsFromFileReturnsEmptyResultIfFileNotFoundTest()
    {
        var jsonFileHandler = new JsonFileHandler();
        var setting = new JsonSerializerSettings();
        var file = "test3.json";

        var model = jsonFileHandler.ReadModelFromFile(file, setting);

        Assert.That(model.Shards.Count, Is.EqualTo(0));
    }

    private static Model.Model CreateModel()
    {
        var entity1Id = Guid.Parse("4338C5BD-62A5-4881-9496-DE733CBC32E9");
        var entity2Id = Guid.Parse("51A44E3E-3A5E-4F08-B0FA-04779C95530F");
        var entity3Id = Guid.Parse("484591A6-0382-4C9D-9F2C-8A12D3227570");
        var entity4Id = Guid.Parse("B5165342-E8B9-456B-AA1E-1A363693E50E");

        return new Model.Model()
        {
            Shards =
            [
                new("Test")
                {
                    Collections =
                    [
                        new Collection<FirstEntityProperties>("First")
                        {
                            Items =
                            [
                                new(entity1Id, new() { NullableStringProperty = "value1" }),
                                new(entity2Id, new() { NullableStringProperty = "value2" })
                            ]
                        },
                        new Collection<SecondEntityProperties>("Second")
                        {
                            Items =
                            [
                                new(entity3Id, new() { DoubleProperty = 1.1 }),
                                new(entity4Id, new() { DoubleProperty = 2.2 })
                            ]
                        },
                    ],
                    Relations =
                    [
                        new("OneToOne")
                        {
                            Pairs =
                            [
                                new(entity1Id, entity3Id),
                                new(entity2Id, entity4Id),
                            ]
                        }
                    ]
                }
            ]
        };
    }
}
