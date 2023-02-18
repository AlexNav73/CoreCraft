using Navitski.Crystalized.Model.Engine.ChangesTracking;
using Navitski.Crystalized.Model.Engine.Core;
using Navitski.Crystalized.Model.Engine.Feature;

namespace Navitski.Crystalized.Model.Tests;

public class ViewTests
{
    [Test]
    public void SnapshotCallsAsMutableAndAsReadOnlyModelShardTest()
    {
        var originalShard = A.Fake<IFakeModelShard>(c => c.Implements<ICanBeMutable<IMutableFakeModelShard>>());
        var mutableShard = A.Fake<IMutableFakeModelShard>(c => c.Implements<ICanBeReadOnly<IFakeModelShard>>());

        A.CallTo(() => ((ICanBeMutable<IMutableFakeModelShard>)originalShard).AsMutable(A<IEnumerable<IFeature>>.Ignored))
            .Returns(mutableShard);
        A.CallTo(() => ((ICanBeReadOnly<IFakeModelShard>)mutableShard).AsReadOnly())
            .Returns(A.Fake<IFakeModelShard>());

        var view = new View(new[] { originalShard });
        var snapshot = view.CreateSnapshot(new IFeature[] { new CoWFeature() });
        var mutableShardSnapshot = snapshot.Shard<IMutableFakeModelShard>();
        var model = snapshot.ToModel();

        A.CallTo(() => ((ICanBeMutable<IMutableFakeModelShard>)originalShard).AsMutable(A<IEnumerable<IFeature>>.Ignored))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => ((ICanBeReadOnly<IFakeModelShard>)mutableShard).AsReadOnly())
            .MustHaveHappenedOnceExactly();
    }

    [Test]
    public void CreateSnapshotReturnsCachedModelShardTest()
    {
        var originalShard = A.Fake<IFakeModelShard>(c => c.Implements<ICanBeMutable<IMutableFakeModelShard>>());
        var mutableShard = A.Fake<IMutableFakeModelShard>(c => c.Implements<ICanBeReadOnly<IFakeModelShard>>());

        A.CallTo(() => ((ICanBeMutable<IMutableFakeModelShard>)originalShard).AsMutable(A<IEnumerable<IFeature>>.Ignored))
            .Returns(mutableShard);
        A.CallTo(() => ((ICanBeReadOnly<IFakeModelShard>)mutableShard).AsReadOnly())
            .Returns(A.Fake<IFakeModelShard>());

        var view = new View(new[] { originalShard });
        var snapshot = view.CreateSnapshot(new IFeature[] { new CoWFeature() });
        var mutableShardSnapshot = snapshot.Shard<IMutableFakeModelShard>();
        var mutableShardSnapshot2 = snapshot.Shard<IMutableFakeModelShard>();
        var model = snapshot.ToModel();

        A.CallTo(() => ((ICanBeMutable<IMutableFakeModelShard>)originalShard).AsMutable(A<IEnumerable<IFeature>>.Ignored))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => ((ICanBeReadOnly<IFakeModelShard>)mutableShard).AsReadOnly())
            .MustHaveHappenedOnceExactly();

        Assert.That(ReferenceEquals(mutableShardSnapshot, mutableShardSnapshot2), Is.True);
    }
}
