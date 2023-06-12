using System.Collections;
using Navitski.Crystalized.Model.Engine.Core;
using Navitski.Crystalized.Model.Engine.Lazy;

namespace Navitski.Crystalized.Model.Tests.Engine.Lazy;

public class CoWCollectionTests
{
    [Test]
    public void CountWhenNotCopiedTest()
    {
        var inner = A.Fake<ICollection<FirstEntity, FirstEntityProperties>>();
        var collection = new CoWCollection<FirstEntity, FirstEntityProperties>(inner);

        var count = collection.Count;

        A.CallTo(() => inner.Count).MustHaveHappenedOnceExactly();
    }

    [Test]
    public void CountWhenCopiedTest()
    {
        var copy = A.Fake<ICollection<FirstEntity, FirstEntityProperties>>(c => c
            .Implements<IMutableCollection<FirstEntity, FirstEntityProperties>>());
        var inner = A.Fake<ICollection<FirstEntity, FirstEntityProperties>>();
        var collection = new CoWCollection<FirstEntity, FirstEntityProperties>(inner);

        A.CallTo(() => inner.Copy()).Returns(copy);

        collection.Add(new());
        var count = collection.Count;

        A.CallTo(() => inner.Copy()).MustHaveHappenedOnceExactly();
        A.CallTo(() => copy.Count).MustHaveHappenedOnceExactly();
    }

    [Test]
    public void AsReadOnlyWhenNotCopiedTest()
    {
        var inner = A.Fake<ICollection<FirstEntity, FirstEntityProperties>>(c => c
            .Implements<IMutableState<ICollection<FirstEntity, FirstEntityProperties>>>());
        var collection = new CoWCollection<FirstEntity, FirstEntityProperties>(inner);

        collection.AsReadOnly();

        A.CallTo(() => inner.Copy()).MustNotHaveHappened();
        A.CallTo(() => ((IMutableState<ICollection<FirstEntity, FirstEntityProperties>>)inner).AsReadOnly())
            .MustHaveHappenedOnceExactly();
    }

    [Test]
    public void AsReadOnlyWhenCopiedTest()
    {
        var copy = A.Fake<ICollection<FirstEntity, FirstEntityProperties>>(c => c
            .Implements<IMutableCollection<FirstEntity, FirstEntityProperties>>()
            .Implements<IMutableState<ICollection<FirstEntity, FirstEntityProperties>>>());
        var inner = A.Fake<ICollection<FirstEntity, FirstEntityProperties>>(c => c
            .Implements<IMutableState<ICollection<FirstEntity, FirstEntityProperties>>>());
        var collection = new CoWCollection<FirstEntity, FirstEntityProperties>(inner);

        A.CallTo(() => inner.Copy()).Returns(copy);

        collection.Add(new());
        collection.AsReadOnly();

        A.CallTo(() => inner.Copy()).MustHaveHappenedOnceExactly();
        A.CallTo(() => ((IMutableState<ICollection<FirstEntity, FirstEntityProperties>>)inner).AsReadOnly())
            .MustNotHaveHappened();
        A.CallTo(() => ((IMutableState<ICollection<FirstEntity, FirstEntityProperties>>)copy).AsReadOnly())
            .MustHaveHappenedOnceExactly();
    }

    [Test]
    public void AddWhenNotCopiedTest()
    {
        var copy = A.Fake<ICollection<FirstEntity, FirstEntityProperties>>(c => c
            .Implements<IMutableCollection<FirstEntity, FirstEntityProperties>>());
        var inner = A.Fake<ICollection<FirstEntity, FirstEntityProperties>>();
        var collection = new CoWCollection<FirstEntity, FirstEntityProperties>(inner);

        A.CallTo(() => inner.Copy()).Returns(copy);

        collection.Add(new());

        A.CallTo(() => inner.Copy()).MustHaveHappenedOnceExactly();
        A.CallTo(() => ((IMutableCollection<FirstEntity, FirstEntityProperties>)copy).Add(A<FirstEntityProperties>.Ignored))
            .MustHaveHappenedOnceExactly();
    }

    [Test]
    public void AddWhenCopiedTest()
    {
        var copy = A.Fake<ICollection<FirstEntity, FirstEntityProperties>>(c => c
            .Implements<IMutableCollection<FirstEntity, FirstEntityProperties>>());
        var inner = A.Fake<ICollection<FirstEntity, FirstEntityProperties>>();
        var collection = new CoWCollection<FirstEntity, FirstEntityProperties>(inner);

        A.CallTo(() => inner.Copy()).Returns(copy);

        collection.Add(new());
        collection.Add(new());

        A.CallTo(() => inner.Copy()).MustHaveHappenedOnceExactly();
        A.CallTo(() => ((IMutableCollection<FirstEntity, FirstEntityProperties>)copy).Add(A<FirstEntityProperties>.Ignored))
            .MustHaveHappenedTwiceExactly();
    }

    [Test]
    public void AddIdFuncWhenNotCopiedTest()
    {
        var copy = A.Fake<ICollection<FirstEntity, FirstEntityProperties>>(c => c
            .Implements<IMutableCollection<FirstEntity, FirstEntityProperties>>());
        var inner = A.Fake<ICollection<FirstEntity, FirstEntityProperties>>();
        var collection = new CoWCollection<FirstEntity, FirstEntityProperties>(inner);

        A.CallTo(() => inner.Copy()).Returns(copy);

        collection.Add(Guid.NewGuid(), p => new());

        A.CallTo(() => inner.Copy()).MustHaveHappenedOnceExactly();
        A.CallTo(() => ((IMutableCollection<FirstEntity, FirstEntityProperties>)copy).Add(A<Guid>.Ignored, A<Func<FirstEntityProperties, FirstEntityProperties>>.Ignored))
            .MustHaveHappenedOnceExactly();
    }

    [Test]
    public void AddIdFuncWhenCopiedTest()
    {
        var copy = A.Fake<ICollection<FirstEntity, FirstEntityProperties>>(c => c
            .Implements<IMutableCollection<FirstEntity, FirstEntityProperties>>());
        var inner = A.Fake<ICollection<FirstEntity, FirstEntityProperties>>();
        var collection = new CoWCollection<FirstEntity, FirstEntityProperties>(inner);

        A.CallTo(() => inner.Copy()).Returns(copy);

        collection.Add(Guid.NewGuid(), p => new());
        collection.Add(Guid.NewGuid(), p => new());

        A.CallTo(() => inner.Copy()).MustHaveHappenedOnceExactly();
        A.CallTo(() => ((IMutableCollection<FirstEntity, FirstEntityProperties>)copy).Add(A<Guid>.Ignored, A<Func<FirstEntityProperties, FirstEntityProperties>>.Ignored))
            .MustHaveHappenedTwiceExactly();
    }

    [Test]
    public void AddEntityPropWhenNotCopiedTest()
    {
        var copy = A.Fake<ICollection<FirstEntity, FirstEntityProperties>>(c => c
            .Implements<IMutableCollection<FirstEntity, FirstEntityProperties>>());
        var inner = A.Fake<ICollection<FirstEntity, FirstEntityProperties>>();
        var collection = new CoWCollection<FirstEntity, FirstEntityProperties>(inner);

        A.CallTo(() => inner.Copy()).Returns(copy);

        collection.Add(new FirstEntity(), new());

        A.CallTo(() => inner.Copy()).MustHaveHappenedOnceExactly();
        A.CallTo(() => ((IMutableCollection<FirstEntity, FirstEntityProperties>)copy).Add(A<FirstEntity>.Ignored, A<FirstEntityProperties>.Ignored))
            .MustHaveHappenedOnceExactly();
    }

    [Test]
    public void AddEntityPropWhenCopiedTest()
    {
        var copy = A.Fake<ICollection<FirstEntity, FirstEntityProperties>>(c => c
            .Implements<IMutableCollection<FirstEntity, FirstEntityProperties>>());
        var inner = A.Fake<ICollection<FirstEntity, FirstEntityProperties>>();
        var collection = new CoWCollection<FirstEntity, FirstEntityProperties>(inner);

        A.CallTo(() => inner.Copy()).Returns(copy);

        collection.Add(new FirstEntity(), new());
        collection.Add(new FirstEntity(), new());

        A.CallTo(() => inner.Copy()).MustHaveHappenedOnceExactly();
        A.CallTo(() => ((IMutableCollection<FirstEntity, FirstEntityProperties>)copy).Add(A<FirstEntity>.Ignored, A<FirstEntityProperties>.Ignored))
            .MustHaveHappenedTwiceExactly();
    }

    [Test]
    public void ContainsWhenNotCopiedTest()
    {
        var copy = A.Fake<ICollection<FirstEntity, FirstEntityProperties>>();
        var inner = A.Fake<ICollection<FirstEntity, FirstEntityProperties>>();
        var collection = new CoWCollection<FirstEntity, FirstEntityProperties>(inner);

        A.CallTo(() => inner.Copy()).Returns(copy);

        collection.Contains(new FirstEntity());

        A.CallTo(() => inner.Copy()).MustNotHaveHappened();
        A.CallTo(() => inner.Contains(A<FirstEntity>.Ignored)).MustHaveHappenedOnceExactly();
        A.CallTo(() => copy.Contains(A<FirstEntity>.Ignored)).MustNotHaveHappened();
    }

    [Test]
    public void ContainsWhenCopiedTest()
    {
        var copy = A.Fake<ICollection<FirstEntity, FirstEntityProperties>>(c => c
            .Implements<IMutableCollection<FirstEntity, FirstEntityProperties>>());
        var inner = A.Fake<ICollection<FirstEntity, FirstEntityProperties>>();
        var collection = new CoWCollection<FirstEntity, FirstEntityProperties>(inner);

        A.CallTo(() => inner.Copy()).Returns(copy);

        collection.Add(new FirstEntity(), new());
        collection.Contains(new FirstEntity());

        A.CallTo(() => inner.Copy()).MustHaveHappenedOnceExactly();
        A.CallTo(() => inner.Contains(A<FirstEntity>.Ignored)).MustNotHaveHappened();
        A.CallTo(() => copy.Contains(A<FirstEntity>.Ignored)).MustHaveHappenedOnceExactly();
    }

    [Test]
    public void GetWhenNotCopiedTest()
    {
        var copy = A.Fake<ICollection<FirstEntity, FirstEntityProperties>>();
        var inner = A.Fake<ICollection<FirstEntity, FirstEntityProperties>>();
        var collection = new CoWCollection<FirstEntity, FirstEntityProperties>(inner);

        A.CallTo(() => inner.Copy()).Returns(copy);

        collection.Get(new FirstEntity());

        A.CallTo(() => inner.Copy()).MustNotHaveHappened();
        A.CallTo(() => inner.Get(A<FirstEntity>.Ignored)).MustHaveHappenedOnceExactly();
        A.CallTo(() => copy.Get(A<FirstEntity>.Ignored)).MustNotHaveHappened();
    }

    [Test]
    public void GetWhenCopiedTest()
    {
        var copy = A.Fake<ICollection<FirstEntity, FirstEntityProperties>>(c => c
            .Implements<IMutableCollection<FirstEntity, FirstEntityProperties>>());
        var inner = A.Fake<ICollection<FirstEntity, FirstEntityProperties>>();
        var collection = new CoWCollection<FirstEntity, FirstEntityProperties>(inner);

        A.CallTo(() => inner.Copy()).Returns(copy);

        collection.Add(new FirstEntity(), new());
        collection.Get(new FirstEntity());

        A.CallTo(() => inner.Copy()).MustHaveHappenedOnceExactly();
        A.CallTo(() => inner.Get(A<FirstEntity>.Ignored)).MustNotHaveHappened();
        A.CallTo(() => copy.Get(A<FirstEntity>.Ignored)).MustHaveHappenedOnceExactly();
    }

    [Test]
    public void ModifyWhenNotCopiedTest()
    {
        var copy = A.Fake<ICollection<FirstEntity, FirstEntityProperties>>(c => c
            .Implements<IMutableCollection<FirstEntity, FirstEntityProperties>>());
        var inner = A.Fake<ICollection<FirstEntity, FirstEntityProperties>>();
        var collection = new CoWCollection<FirstEntity, FirstEntityProperties>(inner);

        A.CallTo(() => inner.Copy()).Returns(copy);

        collection.Modify(new FirstEntity(), p => p);

        A.CallTo(() => inner.Copy()).MustHaveHappenedOnceExactly();
        A.CallTo(() => ((IMutableCollection<FirstEntity, FirstEntityProperties>)copy).Modify(A<FirstEntity>.Ignored, A<Func<FirstEntityProperties, FirstEntityProperties>>.Ignored))
            .MustHaveHappenedOnceExactly();
    }

    [Test]
    public void ModifyWhenCopiedTest()
    {
        var copy = A.Fake<ICollection<FirstEntity, FirstEntityProperties>>(c => c
            .Implements<IMutableCollection<FirstEntity, FirstEntityProperties>>());
        var inner = A.Fake<ICollection<FirstEntity, FirstEntityProperties>>();
        var collection = new CoWCollection<FirstEntity, FirstEntityProperties>(inner);

        A.CallTo(() => inner.Copy()).Returns(copy);

        collection.Modify(new FirstEntity(), p => p);
        collection.Modify(new FirstEntity(), p => p);

        A.CallTo(() => inner.Copy()).MustHaveHappenedOnceExactly();
        A.CallTo(() => ((IMutableCollection<FirstEntity, FirstEntityProperties>)copy).Modify(A<FirstEntity>.Ignored, A<Func<FirstEntityProperties, FirstEntityProperties>>.Ignored))
            .MustHaveHappenedTwiceExactly();
    }

    [Test]
    public void RemoveWhenNotCopiedTest()
    {
        var copy = A.Fake<ICollection<FirstEntity, FirstEntityProperties>>(c => c
            .Implements<IMutableCollection<FirstEntity, FirstEntityProperties>>());
        var inner = A.Fake<ICollection<FirstEntity, FirstEntityProperties>>();
        var collection = new CoWCollection<FirstEntity, FirstEntityProperties>(inner);

        A.CallTo(() => inner.Copy()).Returns(copy);

        collection.Remove(new FirstEntity());

        A.CallTo(() => inner.Copy()).MustHaveHappenedOnceExactly();
        A.CallTo(() => ((IMutableCollection<FirstEntity, FirstEntityProperties>)copy).Remove(A<FirstEntity>.Ignored))
            .MustHaveHappenedOnceExactly();
    }

    [Test]
    public void RemoveWhenCopiedTest()
    {
        var copy = A.Fake<ICollection<FirstEntity, FirstEntityProperties>>(c => c
            .Implements<IMutableCollection<FirstEntity, FirstEntityProperties>>());
        var inner = A.Fake<ICollection<FirstEntity, FirstEntityProperties>>();
        var collection = new CoWCollection<FirstEntity, FirstEntityProperties>(inner);

        A.CallTo(() => inner.Copy()).Returns(copy);

        collection.Remove(new FirstEntity());
        collection.Remove(new FirstEntity());

        A.CallTo(() => inner.Copy()).MustHaveHappenedOnceExactly();
        A.CallTo(() => ((IMutableCollection<FirstEntity, FirstEntityProperties>)copy).Remove(A<FirstEntity>.Ignored))
            .MustHaveHappenedTwiceExactly();
    }

    [Test]
    public void PairsWhenNotCopiedTest()
    {
        var copy = A.Fake<ICollection<FirstEntity, FirstEntityProperties>>();
        var inner = A.Fake<ICollection<FirstEntity, FirstEntityProperties>>();
        var collection = new CoWCollection<FirstEntity, FirstEntityProperties>(inner);

        A.CallTo(() => inner.Copy()).Returns(copy);

        collection.Pairs();

        A.CallTo(() => inner.Copy()).MustNotHaveHappened();
        A.CallTo(() => inner.Pairs()).MustHaveHappenedOnceExactly();
        A.CallTo(() => copy.Pairs()).MustNotHaveHappened();
    }

    [Test]
    public void PairsWhenCopiedTest()
    {
        var copy = A.Fake<ICollection<FirstEntity, FirstEntityProperties>>(c => c
            .Implements<IMutableCollection<FirstEntity, FirstEntityProperties>>());
        var inner = A.Fake<ICollection<FirstEntity, FirstEntityProperties>>();
        var collection = new CoWCollection<FirstEntity, FirstEntityProperties>(inner);

        A.CallTo(() => inner.Copy()).Returns(copy);

        collection.Remove(new FirstEntity());
        collection.Pairs();

        A.CallTo(() => inner.Copy()).MustHaveHappenedOnceExactly();
        A.CallTo(() => inner.Pairs()).MustNotHaveHappened();
        A.CallTo(() => copy.Pairs()).MustHaveHappenedOnceExactly();
    }

    [Test]
    public void CopyTest()
    {
        var inner = A.Fake<ICollection<FirstEntity, FirstEntityProperties>>();
        var collection = new CoWCollection<FirstEntity, FirstEntityProperties>(inner);

        Assert.Throws<InvalidOperationException>(() => collection.Copy());
    }

    [Test]
    public void GetEnumeratorWhenNotCopiedTest()
    {
        var copy = A.Fake<ICollection<FirstEntity, FirstEntityProperties>>();
        var inner = A.Fake<ICollection<FirstEntity, FirstEntityProperties>>();
        var collection = new CoWCollection<FirstEntity, FirstEntityProperties>(inner);

        A.CallTo(() => inner.Copy()).Returns(copy);

        collection.GetEnumerator();

        A.CallTo(() => inner.Copy()).MustNotHaveHappened();
        A.CallTo(() => inner.GetEnumerator()).MustHaveHappenedOnceExactly();
        A.CallTo(() => copy.GetEnumerator()).MustNotHaveHappened();
    }

    [Test]
    public void GetEnumeratorWhenCopiedTest()
    {
        var copy = A.Fake<ICollection<FirstEntity, FirstEntityProperties>>(c => c
            .Implements<IMutableCollection<FirstEntity, FirstEntityProperties>>());
        var inner = A.Fake<ICollection<FirstEntity, FirstEntityProperties>>();
        var collection = new CoWCollection<FirstEntity, FirstEntityProperties>(inner);

        A.CallTo(() => inner.Copy()).Returns(copy);

        collection.Remove(new FirstEntity());
        collection.GetEnumerator();

        A.CallTo(() => inner.Copy()).MustHaveHappenedOnceExactly();
        A.CallTo(() => inner.GetEnumerator()).MustNotHaveHappened();
        A.CallTo(() => copy.GetEnumerator()).MustHaveHappenedOnceExactly();
    }


    [Test]
    public void GetEnumerator2WhenNotCopiedTest()
    {
        var copy = A.Fake<ICollection<FirstEntity, FirstEntityProperties>>();
        var inner = A.Fake<ICollection<FirstEntity, FirstEntityProperties>>();
        var collection = new CoWCollection<FirstEntity, FirstEntityProperties>(inner);

        A.CallTo(() => inner.Copy()).Returns(copy);

        ((IEnumerable)collection).GetEnumerator();

        A.CallTo(() => inner.Copy()).MustNotHaveHappened();
        A.CallTo(() => inner.GetEnumerator()).MustHaveHappenedOnceExactly();
        A.CallTo(() => copy.GetEnumerator()).MustNotHaveHappened();
    }

    [Test]
    public void GetEnumerator2WhenCopiedTest()
    {
        var copy = A.Fake<ICollection<FirstEntity, FirstEntityProperties>>(c => c
            .Implements<IMutableCollection<FirstEntity, FirstEntityProperties>>());
        var inner = A.Fake<ICollection<FirstEntity, FirstEntityProperties>>();
        var collection = new CoWCollection<FirstEntity, FirstEntityProperties>(inner);

        A.CallTo(() => inner.Copy()).Returns(copy);

        collection.Remove(new FirstEntity());
        ((IEnumerable)collection).GetEnumerator();

        A.CallTo(() => inner.Copy()).MustHaveHappenedOnceExactly();
        A.CallTo(() => inner.GetEnumerator()).MustNotHaveHappened();
        A.CallTo(() => copy.GetEnumerator()).MustHaveHappenedOnceExactly();
    }
}
