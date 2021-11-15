using PricingCalc.Model.Tests.Infrastructure;
using PricingCalc.Model.Tests.Infrastructure.Commands;

namespace PricingCalc.Model.Tests.MemoryTests;

[DotMemoryUnit(FailIfRunWithoutSupport = false)]
public class MemoryUsageTests
{
    [Test]
    public void TotalMemoryConsumptionAfterCommandExecutionTest()
    {
        var model = new FakeModel(new[]
        {
            new FakeModelShard()
        },
        new SyncJobService());

        var memoryCheckPoint1 = dotMemory.Check();

        var addCommand = new AddLotOfEntitiesCommand(model);
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

            Assert.That(countOfFirstEntities, Is.EqualTo(200));
            Assert.That(countOfFirstEntitiesProps, Is.EqualTo(200));

            Assert.That(model.History.UndoStack.Count, Is.EqualTo(2));
        });
    }
}
