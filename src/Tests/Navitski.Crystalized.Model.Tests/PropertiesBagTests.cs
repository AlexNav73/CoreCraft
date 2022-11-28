using System.Collections;
using Navitski.Crystalized.Model.Engine.Core;

namespace Navitski.Crystalized.Model.Tests;

internal class PropertiesBagTests
{
    [Test]
    public void GetEnumeratorTest()
    {
        var bag = new PropertiesBag();

        Assert.That(((IEnumerable)bag).GetEnumerator(), Is.Not.Null);
    }
}
