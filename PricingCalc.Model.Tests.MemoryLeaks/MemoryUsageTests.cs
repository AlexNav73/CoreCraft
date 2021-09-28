using JetBrains.dotMemoryUnit;
using NUnit.Framework;
using PricingCalc.Model.Tests.Infrastructure;
using PricingCalc.Model.Tests.Infrastructure.Model;
using PricingCalc.Model.Tests.Infrastructure.Model.Entities;
using PricingCalc.Model.Tests.MemoryTests.Commands;

namespace PricingCalc.Model.Tests.MemoryTests
{
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

                Assert.That(countOfFirstEntities, Is.EqualTo(600));
                Assert.That(countOfFirstEntitiesProps, Is.EqualTo(400));
            });
        }
    }
}
