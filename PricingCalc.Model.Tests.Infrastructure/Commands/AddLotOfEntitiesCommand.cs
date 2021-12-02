using PricingCalc.Model.Engine;
using PricingCalc.Model.Engine.Commands;

namespace PricingCalc.Model.Tests.Infrastructure.Commands;

public class AddLotOfEntitiesCommand : ModelCommand<FakeModel>
{
    private readonly int _count;

    public AddLotOfEntitiesCommand(FakeModel model, int count)
        : base(model)
    {
        _count = count;
    }

    protected override void ExecuteInternal(IModel model)
    {
        var modelShard = model.Shard<IFakeModelShard>();

        for (var i = 0; i < _count; i++)
        {
            var first = modelShard.FirstCollection.Add(new()
            {
                NonNullableStringProperty = "test"
            });
            var second = modelShard.SecondCollection.Add(new()
            {
                FloatProperty = 0.5f
            });

            modelShard.ManyToManyRelation.Add(first, second);
            modelShard.ManyToOneRelation.Add(first, second);
            modelShard.OneToManyRelation.Add(first, second);
            modelShard.OneToOneRelation.Add(first, second);
        }
    }
}
