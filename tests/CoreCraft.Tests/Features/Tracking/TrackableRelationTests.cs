﻿using CoreCraft.ChangesTracking;
using CoreCraft.Core;
using CoreCraft.Features.Tracking;
using CoreCraft.Persistence;
using System.Collections;

namespace CoreCraft.Tests.Features.Tracking;

public class TrackableRelationTests
{
    private IMutableRelation<FirstEntity, SecondEntity>? _relation;
    private IRelationChangeSet<FirstEntity, SecondEntity>? _changes;
    private IMutableRelation<FirstEntity, SecondEntity>? _trackable;

    [SetUp]
    public void Setup()
    {
        _relation = A.Fake<IMutableRelation<FirstEntity, SecondEntity>>();

        _changes = new RelationChangeSet<FirstEntity, SecondEntity>(FakeModelShardInfo.OneToOneRelationInfo);
        _trackable = new TrackableRelation<FirstEntity, SecondEntity>(_changes, _relation);
    }

    [Test]
    public void TrackableRelationInfoTest()
    {
        var info = _trackable!.Info;

        A.CallTo(() => _relation!.Info).MustHaveHappenedOnceExactly();
    }

    [Test]
    public void TrackableAddToRelationTest()
    {
        var first = new FirstEntity();
        var second = new SecondEntity();

        _trackable!.Add(first, second);

        Assert.That(_changes!.HasChanges(), Is.True);
        Assert.That(_changes.Single().Action, Is.EqualTo(RelationAction.Linked));
        Assert.That(_changes.Single().Parent, Is.EqualTo(first));
        Assert.That(_changes.Single().Child, Is.EqualTo(second));
    }

    [Test]
    public void TrackableRemoveFromRelationTest()
    {
        var first = new FirstEntity();
        var second = new SecondEntity();

        _trackable!.Remove(first, second);

        Assert.That(_changes!.HasChanges(), Is.True);
        Assert.That(_changes.Single().Action, Is.EqualTo(RelationAction.Unlinked));
        Assert.That(_changes.Single().Parent, Is.EqualTo(first));
        Assert.That(_changes.Single().Child, Is.EqualTo(second));
    }

    [Test]
    public void TrackableRemoveParentFromRelationTest()
    {
        var first = new FirstEntity();
        var second = new SecondEntity();
        var third = new SecondEntity();

        A.CallTo(() => _relation.Children(first)).Returns([second, third]);

        _trackable!.Remove(first);

        Assert.That(_changes!.HasChanges(), Is.True);
        Assert.That(_changes.First().Action, Is.EqualTo(RelationAction.Unlinked));
        Assert.That(_changes.First().Parent, Is.EqualTo(first));
        Assert.That(_changes.First().Child, Is.EqualTo(second));

        Assert.That(_changes!.HasChanges(), Is.True);
        Assert.That(_changes.Last().Action, Is.EqualTo(RelationAction.Unlinked));
        Assert.That(_changes.Last().Parent, Is.EqualTo(first));
        Assert.That(_changes.Last().Child, Is.EqualTo(third));
    }

    [Test]
    public void TrackableRelationContainsParentTest()
    {
        var first = new FirstEntity();

        _trackable!.ContainsParent(first);

        A.CallTo(() => _relation!.ContainsParent(A<FirstEntity>.Ignored)).MustHaveHappened();
    }

    [Test]
    public void TrackableRelationContainsChildTest()
    {
        var second = new SecondEntity();

        _trackable!.ContainsChild(second);

        A.CallTo(() => _relation!.ContainsChild(A<SecondEntity>.Ignored)).MustHaveHappened();
    }

    [Test]
    public void TrackableRelationAreLinkedTest()
    {
        var first = new FirstEntity();
        var second = new SecondEntity();

        _trackable!.Contains(first, second);

        A.CallTo(() => _relation!.Contains(A<FirstEntity>.Ignored, A<SecondEntity>.Ignored)).MustHaveHappened();
    }

    [Test]
    public void TrackableRelationParentsTest()
    {
        var second = new SecondEntity();

        var parents = _trackable!.Parents(second);

        A.CallTo(() => _relation!.Parents(A<SecondEntity>.Ignored)).MustHaveHappened();
    }

    [Test]
    public void TrackableRelationChildrenTest()
    {
        var entity = new FirstEntity();

        var parents = _trackable!.Children(entity);

        A.CallTo(() => _relation!.Children(A<FirstEntity>.Ignored)).MustHaveHappened();
    }

    [Test]
    public void TrackableRelationCopyTest()
    {
        Assert.Throws<InvalidOperationException>(() => _trackable!.Copy());
    }

    [Test]
    public void TrackableRelationGetEnumeratorTest()
    {
        var enumerator = _trackable!.GetEnumerator();

        A.CallTo(() => _relation!.GetEnumerator()).MustHaveHappened();
    }

    [Test]
    public void TrackableRelationGetEnumerator2Test()
    {
        var enumerator = ((IEnumerable)_trackable!).GetEnumerator();

        A.CallTo(() => _relation!.GetEnumerator()).MustHaveHappened();
    }

    [Test]
    public void SaveShouldCallRepositoryTest()
    {
        var repo = A.Fake<IRepository>();

        _trackable!.Save(repo);

        A.CallTo(() => repo.Update(A<IRelationChangeSet<FirstEntity, SecondEntity>>.Ignored))
            .Invokes(c =>
            {
                var innerRelation = c.Arguments[0];

                Assert.That(ReferenceEquals(_relation, innerRelation), Is.True);
            });
    }
}
