using System.Collections;
using Navitski.Crystalized.Model.Engine.Core;
using Navitski.Crystalized.Model.Engine.Lazy;

namespace Navitski.Crystalized.Model.Tests.Engine.Lazy;

public class CoWRelationTests
{
    [Test]
    public void AsReadOnlyWhenNotCopiedTest()
    {
        var inner = A.Fake<IRelation<FirstEntity, SecondEntity>>(c => c
            .Implements<IMutableState<IRelation<FirstEntity, SecondEntity>>>());
        var relation = new CoWRelation<FirstEntity, SecondEntity>(inner);

        relation.AsReadOnly();

        A.CallTo(() => inner.Copy()).MustNotHaveHappened();
        A.CallTo(() => ((IMutableState<IRelation<FirstEntity, SecondEntity>>)inner).AsReadOnly())
            .MustHaveHappenedOnceExactly();
    }

    [Test]
    public void AsReadOnlyWhenCopiedTest()
    {
        var copy = A.Fake<IRelation<FirstEntity, SecondEntity>>(c => c
            .Implements<IMutableRelation<FirstEntity, SecondEntity>>()
            .Implements<IMutableState<IRelation<FirstEntity, SecondEntity>>>());
        var inner = A.Fake<IRelation<FirstEntity, SecondEntity>>(c => c
            .Implements<IMutableState<IRelation<FirstEntity, SecondEntity>>>());
        var relation = new CoWRelation<FirstEntity, SecondEntity>(inner);

        A.CallTo(() => inner.Copy()).Returns(copy);

        relation.Add(new(), new());
        relation.AsReadOnly();

        A.CallTo(() => inner.Copy()).MustHaveHappenedOnceExactly();
        A.CallTo(() => ((IMutableState<IRelation<FirstEntity, SecondEntity>>)inner).AsReadOnly())
            .MustNotHaveHappened();
        A.CallTo(() => ((IMutableState<IRelation<FirstEntity, SecondEntity>>)copy).AsReadOnly())
            .MustHaveHappenedOnceExactly();
    }

    [Test]
    public void AddWhenNotCopiedTest()
    {
        var copy = A.Fake<IRelation<FirstEntity, SecondEntity>>(c => c
            .Implements<IMutableRelation<FirstEntity, SecondEntity>>());
        var inner = A.Fake<IRelation<FirstEntity, SecondEntity>>(c => c
            .Implements<IMutableRelation<FirstEntity, SecondEntity>>());
        var relation = new CoWRelation<FirstEntity, SecondEntity>(inner);

        A.CallTo(() => inner.Copy()).Returns(copy);

        relation.Add(new(), new());

        A.CallTo(() => inner.Copy()).MustHaveHappenedOnceExactly();
        A.CallTo(() => ((IMutableRelation<FirstEntity, SecondEntity>)inner).Add(A<FirstEntity>.Ignored, A<SecondEntity>.Ignored))
            .MustNotHaveHappened();
        A.CallTo(() => ((IMutableRelation<FirstEntity, SecondEntity>)copy).Add(A<FirstEntity>.Ignored, A<SecondEntity>.Ignored))
            .MustHaveHappenedOnceExactly();
    }

    [Test]
    public void AddWhenCopiedTest()
    {
        var copy = A.Fake<IRelation<FirstEntity, SecondEntity>>(c => c
            .Implements<IMutableRelation<FirstEntity, SecondEntity>>());
        var inner = A.Fake<IRelation<FirstEntity, SecondEntity>>(c => c
            .Implements<IMutableRelation<FirstEntity, SecondEntity>>());
        var relation = new CoWRelation<FirstEntity, SecondEntity>(inner);

        A.CallTo(() => inner.Copy()).Returns(copy);

        relation.Add(new(), new());
        relation.Add(new(), new());

        A.CallTo(() => inner.Copy()).MustHaveHappenedOnceExactly();
        A.CallTo(() => ((IMutableRelation<FirstEntity, SecondEntity>)inner).Add(A<FirstEntity>.Ignored, A<SecondEntity>.Ignored))
            .MustNotHaveHappened();
        A.CallTo(() => ((IMutableRelation<FirstEntity, SecondEntity>)copy).Add(A<FirstEntity>.Ignored, A<SecondEntity>.Ignored))
            .MustHaveHappenedTwiceExactly();
    }

    [Test]
    public void RemoveWhenNotCopiedTest()
    {
        var copy = A.Fake<IRelation<FirstEntity, SecondEntity>>(c => c
            .Implements<IMutableRelation<FirstEntity, SecondEntity>>());
        var inner = A.Fake<IRelation<FirstEntity, SecondEntity>>(c => c
            .Implements<IMutableRelation<FirstEntity, SecondEntity>>());
        var relation = new CoWRelation<FirstEntity, SecondEntity>(inner);

        A.CallTo(() => inner.Copy()).Returns(copy);

        relation.Remove(new(), new());

        A.CallTo(() => inner.Copy()).MustHaveHappenedOnceExactly();
        A.CallTo(() => ((IMutableRelation<FirstEntity, SecondEntity>)inner).Remove(A<FirstEntity>.Ignored, A<SecondEntity>.Ignored))
            .MustNotHaveHappened();
        A.CallTo(() => ((IMutableRelation<FirstEntity, SecondEntity>)copy).Remove(A<FirstEntity>.Ignored, A<SecondEntity>.Ignored))
            .MustHaveHappenedOnceExactly();
    }

    [Test]
    public void RemoveWhenCopiedTest()
    {
        var copy = A.Fake<IRelation<FirstEntity, SecondEntity>>(c => c
            .Implements<IMutableRelation<FirstEntity, SecondEntity>>());
        var inner = A.Fake<IRelation<FirstEntity, SecondEntity>>(c => c
            .Implements<IMutableRelation<FirstEntity, SecondEntity>>());
        var relation = new CoWRelation<FirstEntity, SecondEntity>(inner);

        A.CallTo(() => inner.Copy()).Returns(copy);

        relation.Remove(new(), new());
        relation.Remove(new(), new());

        A.CallTo(() => inner.Copy()).MustHaveHappenedOnceExactly();
        A.CallTo(() => ((IMutableRelation<FirstEntity, SecondEntity>)inner).Remove(A<FirstEntity>.Ignored, A<SecondEntity>.Ignored))
            .MustNotHaveHappened();
        A.CallTo(() => ((IMutableRelation<FirstEntity, SecondEntity>)copy).Remove(A<FirstEntity>.Ignored, A<SecondEntity>.Ignored))
            .MustHaveHappenedTwiceExactly();
    }

    [Test]
    public void ContainsParentWhenNotCopiedTest()
    {
        var copy = A.Fake<IRelation<FirstEntity, SecondEntity>>();
        var inner = A.Fake<IRelation<FirstEntity, SecondEntity>>();
        var relation = new CoWRelation<FirstEntity, SecondEntity>(inner);

        A.CallTo(() => inner.Copy()).Returns(copy);

        relation.ContainsParent(new());

        A.CallTo(() => inner.Copy()).MustNotHaveHappened();
        A.CallTo(() => inner.ContainsParent(A<FirstEntity>.Ignored))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => copy.ContainsParent(A<FirstEntity>.Ignored))
            .MustNotHaveHappened();
    }

    [Test]
    public void ContainsParentWhenCopiedTest()
    {
        var copy = A.Fake<IRelation<FirstEntity, SecondEntity>>(c => c
            .Implements<IMutableRelation<FirstEntity, SecondEntity>>());
        var inner = A.Fake<IRelation<FirstEntity, SecondEntity>>();
        var relation = new CoWRelation<FirstEntity, SecondEntity>(inner);

        A.CallTo(() => inner.Copy()).Returns(copy);

        relation.Remove(new(), new());
        relation.ContainsParent(new());

        A.CallTo(() => inner.Copy()).MustHaveHappenedOnceExactly();
        A.CallTo(() => inner.ContainsParent(A<FirstEntity>.Ignored))
            .MustNotHaveHappened();
        A.CallTo(() => copy.ContainsParent(A<FirstEntity>.Ignored))
            .MustHaveHappenedOnceExactly();
    }

    [Test]
    public void ContainsChildWhenNotCopiedTest()
    {
        var copy = A.Fake<IRelation<FirstEntity, SecondEntity>>();
        var inner = A.Fake<IRelation<FirstEntity, SecondEntity>>();
        var relation = new CoWRelation<FirstEntity, SecondEntity>(inner);

        A.CallTo(() => inner.Copy()).Returns(copy);

        relation.ContainsChild(new());

        A.CallTo(() => inner.Copy()).MustNotHaveHappened();
        A.CallTo(() => inner.ContainsChild(A<SecondEntity>.Ignored))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => copy.ContainsChild(A<SecondEntity>.Ignored))
            .MustNotHaveHappened();
    }

    [Test]
    public void ContainsChildWhenCopiedTest()
    {
        var copy = A.Fake<IRelation<FirstEntity, SecondEntity>>(c => c
            .Implements<IMutableRelation<FirstEntity, SecondEntity>>());
        var inner = A.Fake<IRelation<FirstEntity, SecondEntity>>();
        var relation = new CoWRelation<FirstEntity, SecondEntity>(inner);

        A.CallTo(() => inner.Copy()).Returns(copy);

        relation.Remove(new(), new());
        relation.ContainsChild(new());

        A.CallTo(() => inner.Copy()).MustHaveHappenedOnceExactly();
        A.CallTo(() => inner.ContainsChild(A<SecondEntity>.Ignored))
            .MustNotHaveHappened();
        A.CallTo(() => copy.ContainsChild(A<SecondEntity>.Ignored))
            .MustHaveHappenedOnceExactly();
    }

    [Test]
    public void ChildrenChildWhenNotCopiedTest()
    {
        var copy = A.Fake<IRelation<FirstEntity, SecondEntity>>();
        var inner = A.Fake<IRelation<FirstEntity, SecondEntity>>();
        var relation = new CoWRelation<FirstEntity, SecondEntity>(inner);

        A.CallTo(() => inner.Copy()).Returns(copy);

        relation.Children(new());

        A.CallTo(() => inner.Copy()).MustNotHaveHappened();
        A.CallTo(() => inner.Children(A<FirstEntity>.Ignored))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => copy.Children(A<FirstEntity>.Ignored))
            .MustNotHaveHappened();
    }

    [Test]
    public void ChildrenWhenCopiedTest()
    {
        var copy = A.Fake<IRelation<FirstEntity, SecondEntity>>(c => c
            .Implements<IMutableRelation<FirstEntity, SecondEntity>>());
        var inner = A.Fake<IRelation<FirstEntity, SecondEntity>>();
        var relation = new CoWRelation<FirstEntity, SecondEntity>(inner);

        A.CallTo(() => inner.Copy()).Returns(copy);

        relation.Remove(new(), new());
        relation.Children(new());

        A.CallTo(() => inner.Copy()).MustHaveHappenedOnceExactly();
        A.CallTo(() => inner.Children(A<FirstEntity>.Ignored))
            .MustNotHaveHappened();
        A.CallTo(() => copy.Children(A<FirstEntity>.Ignored))
            .MustHaveHappenedOnceExactly();
    }

    [Test]
    public void ParentsChildWhenNotCopiedTest()
    {
        var copy = A.Fake<IRelation<FirstEntity, SecondEntity>>();
        var inner = A.Fake<IRelation<FirstEntity, SecondEntity>>();
        var relation = new CoWRelation<FirstEntity, SecondEntity>(inner);

        A.CallTo(() => inner.Copy()).Returns(copy);

        relation.Parents(new());

        A.CallTo(() => inner.Copy()).MustNotHaveHappened();
        A.CallTo(() => inner.Parents(A<SecondEntity>.Ignored))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => copy.Parents(A<SecondEntity>.Ignored))
            .MustNotHaveHappened();
    }

    [Test]
    public void ParentsWhenCopiedTest()
    {
        var copy = A.Fake<IRelation<FirstEntity, SecondEntity>>(c => c
            .Implements<IMutableRelation<FirstEntity, SecondEntity>>());
        var inner = A.Fake<IRelation<FirstEntity, SecondEntity>>();
        var relation = new CoWRelation<FirstEntity, SecondEntity>(inner);

        A.CallTo(() => inner.Copy()).Returns(copy);

        relation.Remove(new(), new());
        relation.Parents(new());

        A.CallTo(() => inner.Copy()).MustHaveHappenedOnceExactly();
        A.CallTo(() => inner.Parents(A<SecondEntity>.Ignored))
            .MustNotHaveHappened();
        A.CallTo(() => copy.Parents(A<SecondEntity>.Ignored))
            .MustHaveHappenedOnceExactly();
    }

    [Test]
    public void CopyTest()
    {
        var inner = A.Fake<IRelation<FirstEntity, SecondEntity>>();
        var relation = new CoWRelation<FirstEntity, SecondEntity>(inner);

        Assert.Throws<InvalidOperationException>(() => relation.Copy());
    }

    [Test]
    public void GetEnumeratorWhenNotCopiedTest()
    {
        var copy = A.Fake<IRelation<FirstEntity, SecondEntity>>(c => c
            .Implements<IMutableRelation<FirstEntity, SecondEntity>>());
        var inner = A.Fake<IRelation<FirstEntity, SecondEntity>>();
        var relation = new CoWRelation<FirstEntity, SecondEntity>(inner);

        A.CallTo(() => inner.Copy()).Returns(copy);

        relation.GetEnumerator();

        A.CallTo(() => inner.Copy()).MustNotHaveHappened();
        A.CallTo(() => inner.GetEnumerator()).MustHaveHappenedOnceExactly();
        A.CallTo(() => copy.GetEnumerator()).MustNotHaveHappened();
    }

    [Test]
    public void GetEnumeratorWhenCopiedTest()
    {
        var copy = A.Fake<IRelation<FirstEntity, SecondEntity>>(c => c
            .Implements<IMutableRelation<FirstEntity, SecondEntity>>());
        var inner = A.Fake<IRelation<FirstEntity, SecondEntity>>();
        var relation = new CoWRelation<FirstEntity, SecondEntity>(inner);

        A.CallTo(() => inner.Copy()).Returns(copy);

        relation.Remove(new(), new());
        relation.GetEnumerator();

        A.CallTo(() => inner.Copy()).MustHaveHappenedOnceExactly();
        A.CallTo(() => inner.GetEnumerator()).MustNotHaveHappened();
        A.CallTo(() => copy.GetEnumerator()).MustHaveHappenedOnceExactly();
    }


    [Test]
    public void GetEnumerator2WhenNotCopiedTest()
    {
        var copy = A.Fake<IRelation<FirstEntity, SecondEntity>>(c => c
            .Implements<IMutableRelation<FirstEntity, SecondEntity>>());
        var inner = A.Fake<IRelation<FirstEntity, SecondEntity>>();
        var relation = new CoWRelation<FirstEntity, SecondEntity>(inner);

        A.CallTo(() => inner.Copy()).Returns(copy);

        ((IEnumerable)relation).GetEnumerator();

        A.CallTo(() => inner.Copy()).MustNotHaveHappened();
        A.CallTo(() => inner.GetEnumerator()).MustHaveHappenedOnceExactly();
        A.CallTo(() => copy.GetEnumerator()).MustNotHaveHappened();
    }

    [Test]
    public void GetEnumerator2WhenCopiedTest()
    {
        var copy = A.Fake<IRelation<FirstEntity, SecondEntity>>(c => c
            .Implements<IMutableRelation<FirstEntity, SecondEntity>>());
        var inner = A.Fake<IRelation<FirstEntity, SecondEntity>>();
        var relation = new CoWRelation<FirstEntity, SecondEntity>(inner);

        A.CallTo(() => inner.Copy()).Returns(copy);

        relation.Remove(new(), new());
        ((IEnumerable)relation).GetEnumerator();

        A.CallTo(() => inner.Copy()).MustHaveHappenedOnceExactly();
        A.CallTo(() => inner.GetEnumerator()).MustNotHaveHappened();
        A.CallTo(() => copy.GetEnumerator()).MustHaveHappenedOnceExactly();
    }
}
