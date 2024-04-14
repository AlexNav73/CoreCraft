using CoreCraft.Core;
using CoreCraft.Exceptions;

namespace CoreCraft.Tests.Core;

public class ModelTests
{
    [Test]
    public void GetShardByTypeThrowsExceptionTest()
    {
        var model = new Model(Array.Empty<IModelShard>());

        Assert.Throws<ModelShardNotFoundException>(() => model.Shard<IFakeModelShard>());
    }
}
