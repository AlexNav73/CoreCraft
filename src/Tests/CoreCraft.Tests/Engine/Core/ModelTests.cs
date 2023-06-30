using CoreCraft.Engine.Core;
using CoreCraft.Engine.Exceptions;

namespace CoreCraft.Tests.Engine.Core;

public class ModelTests
{
    [Test]
    public void GetShardByTypeThrowsExceptionTest()
    {
        var model = new Model(Array.Empty<IModelShard>());

        Assert.Throws<ModelShardNotFoundException>(() => model.Shard<IFakeModelShard>());
    }
}
