using Navitski.Crystalized.Model.Engine;
using Navitski.Crystalized.Model.Engine.Commands;
using Navitski.Crystalized.Model.Engine.Core;
using System.Threading;
using WpfDemoApp.Model;
using WpfDemoApp.Model.Entities;

namespace WpfDemoApp.ModelCommands;

internal class RemoveItemModelCommand : ModelCommand<IDomainModel>
{
    public RemoveItemModelCommand(IDomainModel model)
        : base(model)
    {
        Entity = Parameter<ToDoItem>(nameof(Entity));
    }

    public ICommandParameter<ToDoItem> Entity { get; }

    protected override void ExecuteInternal(IModel model, CancellationToken token)
    {
        var shard = model.Shard<IMutableToDoModelShard>();

        shard.Items.Remove(Entity.Value);
    }
}
