using Navitski.Crystalized.Model.Engine;
using Navitski.Crystalized.Model.Engine.Commands;
using Navitski.Crystalized.Model.Engine.Core;
using System.Threading;
using WpfDemoApp.Model;
using WpfDemoApp.Model.Entities;

namespace WpfDemoApp.ModelCommands;

internal class ModifyItemModelCommand : ModelCommand<IDomainModel>
{
    public ModifyItemModelCommand(IDomainModel model)
        : base(model)
    {
        Entity = Parameter<ToDoItem>(nameof(Entity));
        NewName = Parameter<string>(nameof(NewName));
    }

    public ICommandParameter<ToDoItem> Entity { get; }

    public ICommandParameter<string> NewName { get; }

    protected override void ExecuteInternal(IModel model, CancellationToken token)
    {
        var shard = model.Shard<IMutableToDoModelShard>();

        shard.Items.Modify(Entity.Value, p => p with { Name = NewName.Value });
    }
}
