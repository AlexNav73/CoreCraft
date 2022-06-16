using Navitski.Crystalized.Model.Engine;
using Navitski.Crystalized.Model.Engine.Core;

namespace Navitski.Crystalized.Model.Tests;

public class ViewTests
{
    [Test]
    public void CreateSnapshotCopiesModelShardTest()
    {
        var originalShard = A.Fake<IFakeModelShard>(c => c.Implements<ICopy<IFakeModelShard>>());
        var view = new View(new[] { originalShard });
        var snapshot = view.CreateSnapshot();
        var shardSnapshot = snapshot.Shard<IFakeModelShard>();

        A.CallTo(() => ((ICopy<IFakeModelShard>)originalShard).Copy()).MustHaveHappenedOnceExactly();
    }

    [Test]
    public void CreateSnapshotReturnsCachedModelShardTest()
    {
        var originalShard = A.Fake<IFakeModelShard>(c => c.Implements<ICopy<IFakeModelShard>>());
        var view = new View(new[] { originalShard });
        var snapshot = view.CreateSnapshot();
        var shardSnapshot = snapshot.Shard<IFakeModelShard>();
        var shardSnapshot2 = snapshot.Shard<IFakeModelShard>();

        A.CallTo(() => ((ICopy<IFakeModelShard>)originalShard).Copy()).MustHaveHappenedOnceExactly();
    }

    [Test]
    public void CopyModelReturnsCopiedModelShardTest()
    {
        var originalShard = A.Fake<IFakeModelShard>(c => c.Implements<ICopy<IFakeModelShard>>());
        var view = new View(new[] { originalShard });
        var snapshot = view.CopyModel();

        A.CallTo(() => ((ICopy<IFakeModelShard>)originalShard).Copy()).MustHaveHappenedOnceExactly();
    }
}
