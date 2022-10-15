using Navitski.Crystalized.Model.Tests.Infrastructure;
using Navitski.Crystalized.Model.Tests.Infrastructure.Commands;

namespace Navitski.Crystalized.Model.Tests.MemoryTests;

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

        dotMemory.Check(mem =>
        {
            var diff = mem.GetDifference(memoryCheckPoint1);
            var countOfFirstEntities = diff
                .GetNewObjects(q => q.Type.Is<FirstEntity>())
                .ObjectsCount;
            var countOfFirstEntitiesProps = diff
                .GetNewObjects(q => q.Type.Is<FirstEntityProperties>())
                .ObjectsCount;

            Assert.That(countOfFirstEntities, Is.EqualTo(200), "Wrong number of entities");
            Assert.That(countOfFirstEntitiesProps, Is.EqualTo(200), "Wrong number of entities properties");
        });

        // assertion prevents model from been deleted by GC before dotMemory.Check call
        Assert.That(model.UndoStack.Count, Is.EqualTo(2));
    }
}
