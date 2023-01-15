using Navitski.Crystalized.Model.Storage.Json.Model;

namespace Navitski.Crystalized.Model.Storage.Json.Tests;

public class JsonFileHandlerTests
{
    [Test]
    public async Task WriteModelShardsToFileTest()
    {
        var shards = CreateModel();
        var jsonFileHandler = new JsonFileHandler();
        var file = "test1.json";

        jsonFileHandler.WriteModelShardsToFile(file, shards);

        var json = File.ReadAllText(file);
        File.Delete(file);

        await Verify(json).UseDirectory("./VerifiedFiles");
    }

    [Test]
    public void ReadModelShardsFromFileTest()
    {
        var shards = CreateModel();
        var jsonFileHandler = new JsonFileHandler();
        var file = "test2.json";

        jsonFileHandler.WriteModelShardsToFile(file, shards);
        var json1 = File.ReadAllText(file);

        var redShards = jsonFileHandler.ReadModelShardsFromFile(file);
        jsonFileHandler.WriteModelShardsToFile(file, redShards);

        var json2 = File.ReadAllText(file);
        File.Delete(file);

        Assert.That(json1, Is.EqualTo(json2));
    }

    [Test]
    public void ReadModelShardsFromFileReturnsEmptyResultIfFileNotFoundTest()
    {
        var jsonFileHandler = new JsonFileHandler();
        var file = "test3.json";

        var redShards = jsonFileHandler.ReadModelShardsFromFile(file);

        Assert.That(redShards.Count, Is.EqualTo(0));
    }

    private static IList<ModelShard> CreateModel()
    {
        var entity1Id = Guid.Parse("4338C5BD-62A5-4881-9496-DE733CBC32E9");
        var entity2Id = Guid.Parse("51A44E3E-3A5E-4F08-B0FA-04779C95530F");
        var entity3Id = Guid.Parse("484591A6-0382-4C9D-9F2C-8A12D3227570");
        var entity4Id = Guid.Parse("B5165342-E8B9-456B-AA1E-1A363693E50E");

        return new List<ModelShard>()
        {
            new ModelShard("Test")
            {
                Collections = new List<ICollection>()
                {
                    new Collection<FirstEntityProperties>("First")
                    {
                        Items = new List<Item<FirstEntityProperties>>()
                        {
                            new Item<FirstEntityProperties>(entity1Id, new() { NullableStringProperty = "value1" }),
                            new Item<FirstEntityProperties>(entity2Id, new() { NullableStringProperty = "value2" })
                        }
                    },
                    new Collection<SecondEntityProperties>("Second")
                    {
                        Items = new List<Item<SecondEntityProperties>>()
                        {
                            new Item<SecondEntityProperties>(entity3Id, new() { DoubleProperty = 1.1 }),
                            new Item<SecondEntityProperties>(entity4Id, new() { DoubleProperty = 2.2 })
                        }
                    },
                },
                Relations = new List<Relation>()
                {
                    new Relation("OneToOne")
                    {
                        Pairs = new List<Pair>()
                        {
                            new Pair(entity1Id, entity3Id),
                            new Pair(entity2Id, entity4Id),
                        }
                    }
                }
            }
        };
    }
}
