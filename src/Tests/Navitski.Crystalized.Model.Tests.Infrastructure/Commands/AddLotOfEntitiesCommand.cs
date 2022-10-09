﻿using Navitski.Crystalized.Model.Engine.Commands;
using Navitski.Crystalized.Model.Engine.Core;

namespace Navitski.Crystalized.Model.Tests.Infrastructure.Commands;

public class AddLotOfEntitiesCommand : ICommand
{
    private readonly int _count;

    public AddLotOfEntitiesCommand(int count)
    {
        _count = count;
    }

    public void Execute(IModel model, CancellationToken token)
    {
        var modelShard = model.Shard<IMutableFakeModelShard>();

        for (var i = 0; i < _count; i++)
        {
            token.ThrowIfCancellationRequested();

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
