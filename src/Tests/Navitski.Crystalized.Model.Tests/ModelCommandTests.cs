using Navitski.Crystalized.Model.Engine;
using Navitski.Crystalized.Model.Engine.Commands;

namespace Navitski.Crystalized.Model.Tests;

public class ModelCommandTests
{
    [Test]
    public void CtorTest()
    {
        Assert.DoesNotThrow(() => new TestModelCommand(CreateModel()));
    }

    [Test]
    public void NotInitializedParameterTest()
    {
        var command = new TestModelCommand(CreateModel());

        Assert.Throws<ArgumentException>(() => command.Execute());
    }

    [Test]
    public void InitializedParameterTest()
    {
        var model = CreateModel();
        var command = new TestModelCommand(model);

        command.Name.Set("value");

        Assert.DoesNotThrow(() => command.Execute());
        A.CallTo(() => ((ICommandRunner)model).Enqueue(A<IRunnable>.Ignored, A<CancellationToken>.Ignored))
            .MustHaveHappened();
    }

    [Test]
    public void CallExecuteInternalTest()
    {
        var model = CreateModel();
        var command = new TestModelCommand(model);

        command.Name.Set("value");

        A.CallTo(() => ((ICommandRunner)model).Enqueue(A<IRunnable>.Ignored, A<CancellationToken>.Ignored))
            .Invokes(c => ((IRunnable)command).Run(A.Fake<IModel>(), CancellationToken.None));

        Assert.Throws<NotImplementedException>(() => command.Execute());
    }

    private IDomainModel CreateModel()
    {
        return A.Fake<IDomainModel>(c => c.Implements<ICommandRunner>());
    }
}

class TestModelCommand : ModelCommand<IDomainModel>
{
    public TestModelCommand(IDomainModel model)
        : base(model)
    {
        Name = Parameter<string>(nameof(Name));
    }

    public ICommandParameter<string> Name { get; }

    protected override void ExecuteInternal(IModel model, CancellationToken token)
    {
        throw new NotImplementedException();
    }
}

