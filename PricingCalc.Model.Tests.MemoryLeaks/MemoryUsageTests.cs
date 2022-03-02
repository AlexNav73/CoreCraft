using PricingCalc.Model.Tests.Infrastructure;
using PricingCalc.Model.Tests.Infrastructure.Commands;

namespace PricingCalc.Model.Tests.MemoryTests;

[DotMemoryUnit(FailIfRunWithoutSupport = false)]
public class MemoryUsageTests
{
    [Test]
    public void EntitiesAndPropertiesAreCopiedOnlyWhenModifiedTest()
    {
        var model = new FakeModel(new[]
        {
            new FakeModelShard()
        });

        var memoryCheckPoint1 = dotMemory.Check();

        var addCommand = new AddLotOfEntitiesCommand(model, 100);
        addCommand.Execute();
        var modifyCommand = new ModifyAllEntitiesCommand(model);
        modifyCommand.Execute();

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

        Assert.That(model.History.UndoStack.Count, Is.EqualTo(2));
    }
}
