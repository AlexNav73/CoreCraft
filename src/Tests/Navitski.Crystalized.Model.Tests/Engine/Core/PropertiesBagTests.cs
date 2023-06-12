using System.Collections;
using Navitski.Crystalized.Model.Engine.Core;

namespace Navitski.Crystalized.Model.Tests.Engine.Core;

internal class PropertiesBagTests
{
    [Test]
    public void GetEnumeratorTest()
    {
        var bag = new PropertiesBag();

        Assert.That(((IEnumerable)bag).GetEnumerator(), Is.Not.Null);
    }

    [Test]
    public void ContainsTest()
    {
        var bag = new PropertiesBag();
        var propName = "prop";

        bag.Write(propName, propName);

        Assert.That(bag.ContainsProp(propName), Is.True);
    }

    [Test]
    public void CompareTest()
    {
        var bag1 = new PropertiesBag();
        var bag2 = new PropertiesBag();
        var propName1 = "prop1";
        var propName2 = "prop2";
        var val1 = "value1";
        var val2 = "value2";

        bag1.Write(propName1, val1);
        bag1.Write<string?>(propName2, null);

        bag2.Write(propName1, val2);
        bag2.Write<string?>(propName2, null);

        var result = bag1.Compare(bag2);

        Assert.That(result.ContainsProp(propName1), Is.True);
        Assert.That(result.ContainsProp(propName2), Is.False);
    }
}
