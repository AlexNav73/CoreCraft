using CoreCraft.Core;
using CoreCraft.Features.CoW;

namespace CoreCraft.Tests.Features.CoW;

public class CoWFeatureTests
{
    [Test]
    public void DecorateTest()
    {
        var feature = new CoWFeature();

        var collection = feature.Decorate(A.Fake<IFrameFactory>(), A.Fake<IMutableCollection<FirstEntity, FirstEntityProperties>>());
        var relation = feature.Decorate(A.Fake<IFrameFactory>(), A.Fake<IMutableRelation<FirstEntity, SecondEntity>>());

        Assert.That(collection, Is.Not.Null);
        Assert.That(collection, Is.TypeOf<CoWCollection<FirstEntity, FirstEntityProperties>>());
        Assert.That(relation, Is.Not.Null);
        Assert.That(relation, Is.TypeOf<CoWRelation<FirstEntity, SecondEntity>>());
    }
}
