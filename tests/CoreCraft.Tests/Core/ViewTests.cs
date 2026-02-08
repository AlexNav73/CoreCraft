using CoreCraft.Core;

namespace CoreCraft.Tests.Core;

public class ViewTests
{
    [Test]
    public void SnapshotCallsAsMutableAndAsReadOnlyModelShardTest()
    {
        var originalShard = A.Fake<IFakeModelShard>(c => c.Implements<IReadOnlyState<IMutableFakeModelShard>>());
        var mutableShard = A.Fake<IMutableFakeModelShard>(c => c.Implements<IMutableState<IFakeModelShard>>());

        A.CallTo(() => ((IReadOnlyState<IMutableFakeModelShard>)originalShard).AsApplyModel())
            .Returns(mutableShard);
        A.CallTo(() => ((IMutableState<IFakeModelShard>)mutableShard).AsReadOnly())
            .Returns(A.Fake<IFakeModelShard>());

        var view = new ModelView(new[] { originalShard });
        var snapshot = new Snapshot(view.UnsafeModel, s => s.AsApplyModel());
        var mutableShardSnapshot = ((IMutableModel)snapshot).Shard<IMutableFakeModelShard>();
        var model = snapshot.ToModel();

        A.CallTo(() => ((IReadOnlyState<IMutableFakeModelShard>)originalShard).AsApplyModel())
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => ((IMutableState<IFakeModelShard>)mutableShard).AsReadOnly())
            .MustHaveHappenedOnceExactly();
    }

    [Test]
    public void CreateSnapshotReturnsCachedModelShardTest()
    {
        var originalShard = A.Fake<IFakeModelShard>(c => c.Implements<IReadOnlyState<IMutableFakeModelShard>>());
        var mutableShard = A.Fake<IMutableFakeModelShard>(c => c.Implements<IMutableState<IFakeModelShard>>());

        A.CallTo(() => ((IReadOnlyState<IMutableFakeModelShard>)originalShard).AsApplyModel())
            .Returns(mutableShard);
        A.CallTo(() => ((IMutableState<IFakeModelShard>)mutableShard).AsReadOnly())
            .Returns(A.Fake<IFakeModelShard>());

        var view = new ModelView(new[] { originalShard });
        var snapshot = new Snapshot(view.UnsafeModel, s => s.AsApplyModel());
        var mutableShardSnapshot = ((IMutableModel)snapshot).Shard<IMutableFakeModelShard>();
        var mutableShardSnapshot2 = ((IMutableModel)snapshot).Shard<IMutableFakeModelShard>();
        var model = snapshot.ToModel();

        A.CallTo(() => ((IReadOnlyState<IMutableFakeModelShard>)originalShard).AsApplyModel())
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => ((IMutableState<IFakeModelShard>)mutableShard).AsReadOnly())
            .MustHaveHappenedOnceExactly();

        Assert.That(ReferenceEquals(mutableShardSnapshot, mutableShardSnapshot2), Is.True);
    }
}
