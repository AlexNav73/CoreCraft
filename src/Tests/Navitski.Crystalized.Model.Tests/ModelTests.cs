using Navitski.Crystalized.Model.Engine.Core;
using Navitski.Crystalized.Model.Engine.Exceptions;

namespace Navitski.Crystalized.Model.Tests;

public class ModelTests
{
    [Test]
    public void GetShardByTypeThrowsExceptionTest()
    {
        var model = new Engine.Model(Array.Empty<IModelShard>());

        Assert.Throws<ModelShardNotFoundException>(() => model.Shard<IFakeModelShard>());
    }
}
