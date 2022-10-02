using Navitski.Crystalized.Model.Engine;
using Navitski.Crystalized.Model.Engine.Commands;
using Navitski.Crystalized.Model.Engine.Core;
using System.Threading;
using WpfDemoApp.Model;

namespace WpfDemoApp.ModelCommands;

internal class AddItemModelCommand : ModelCommand<IDomainModel>
{
    public AddItemModelCommand(IDomainModel model)
        : base(model)
    {
        Name = Parameter<string>(nameof(Name));
    }

    public ICommandParameter<string> Name { get; }

    protected override void ExecuteInternal(IModel model, CancellationToken token)
    {
        var shard = model.Shard<IMutableToDoModelShard>();

        shard.Items.Add(new() { Name = Name.Value });
    }
}
