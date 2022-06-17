using Navitski.Crystalized.Model.Engine;
using Navitski.Crystalized.Model.Engine.Commands;
using Navitski.Crystalized.Model.Engine.Core;

namespace Navitski.Crystalized.Model.Tests.Infrastructure.Commands;

public class DelegateCommand<T> : ModelCommand<T>
    where T : IDomainModel
{
    private readonly Action<IModel> _executor;

    public DelegateCommand(T model, Action<IModel> executor)
        : base(model)
    {
        _executor = executor;
    }

    protected override void ExecuteInternal(IModel model, CancellationToken token)
    {
        _executor(model);
    }
}
