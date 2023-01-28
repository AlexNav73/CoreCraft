using Navitski.Crystalized.Model.Engine.Core;
using Navitski.Crystalized.Model.Tests.Infrastructure;
using Navitski.Crystalized.Model.Tests.Infrastructure.Commands;

namespace Navitski.Crystalized.Model.Tests.MemoryLeaks;

[DotMemoryUnit(FailIfRunWithoutSupport = false)]
public class MemoryUsageTests
{
    [Test]
    public async Task EntitiesAndPropertiesAreCopiedOnlyWhenModifiedTest()
    {
        var model = new FakeModel(new[]
        {
            new FakeModelShard()
        });

        var memoryCheckPoint1 = dotMemory.Check();

        await model.Run(new AddLotOfEntitiesCommand(100));
        await model.Run(new ModifyAllEntitiesCommand());

        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        dotMemory.Check(mem =>
        {
            var diff = mem.GetDifference(memoryCheckPoint1);
            var countOfFirstEntities = diff
                .GetNewObjects(q => q.Type.Is<FirstEntity>())
                .ObjectsCount;
            var countOfFirstEntitiesProps = diff
                .GetNewObjects(q => q.Type.Is<FirstEntityProperties>())
                .ObjectsCount;

            Assert.That(countOfFirstEntities, Is.EqualTo(100), "Wrong number of entities");
            Assert.That(countOfFirstEntitiesProps, Is.EqualTo(200), "Wrong number of entities properties");
        });

        // assertion prevents model from been deleted by GC before dotMemory.Check call
        Assert.That(model.UndoStack.Count, Is.EqualTo(2));
    }

    [Test]
    public async Task CollectionsAndRelationsAreCopiedOnlyWhenModifiedTest()
    {
        var model = new FakeModel(new[]
        {
            new FakeModelShard()
        });

        await model.Run<IMutableFakeModelShard>((shard, _) =>
        {
            var parent = shard.FirstCollection.Add(new());
            var child = shard.SecondCollection.Add(new());

            shard.OneToManyRelation.Add(parent, child);
        });

        var memoryCheckPoint1 = dotMemory.Check();

        await model.Run<IMutableFakeModelShard>((shard, _) =>
        {
            var parent = shard.FirstCollection.First();
            var child = shard.SecondCollection.Add(new());

            shard.OneToManyRelation.Add(parent, child);
        });

        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        dotMemory.Check(mem =>
        {
            var diff = mem.GetDifference(memoryCheckPoint1);
            var countOfFirstCollections = diff
                .GetNewObjects(q => q.Type.Is<Collection<FirstEntity, FirstEntityProperties>>())
                .ObjectsCount;
            var countOfSecondCollections = diff
                .GetNewObjects(q => q.Type.Is<Collection<SecondEntity, SecondEntityProperties>>())
                .ObjectsCount;
            var countOfRelations = diff
                .GetNewObjects(q => q.Type.Is<Relation<FirstEntity, SecondEntity>>())
                .ObjectsCount;

            Assert.That(countOfFirstCollections, Is.EqualTo(0), "Collection object should not be copied if it is not modified");
            Assert.That(countOfSecondCollections, Is.EqualTo(1), "Second collection was modified so it should be copied to contain new properties");
            Assert.That(countOfRelations, Is.EqualTo(1), "Relation was modified so it should be copied to contain relations");
        });

        // assertion prevents model from been deleted by GC before dotMemory.Check call
        Assert.That(model.UndoStack.Count, Is.EqualTo(2));
    }
}
