using Navitski.Crystalized.Model.Engine.Commands;

namespace Navitski.Crystalized.Model.Tests;

public class CommandParameterTests
{
    [Test]
    public void CreateNotInitializedValueTypeParameterTest()
    {
        var name = "param";
        var parameter = new CommandParameter<int>(name);

        Assert.That(parameter.Name, Is.EqualTo(name));
        Assert.That(parameter.IsInitialized, Is.False);
        Assert.That(parameter.Value, Is.EqualTo(0));
    }

    [Test]
    public void CreateNotInitializedRefTypeParameterTest()
    {
        var name = "param";
        var parameter = new CommandParameter<string>(name);
        string? newValue = null;

        Assert.That(parameter.Name, Is.EqualTo(name));
        Assert.That(parameter.IsInitialized, Is.False);
        Assert.Throws<InvalidOperationException>(() => newValue = parameter.Value);
    }

    [Test]
    public void SetParameterValueTest()
    {
        var name = "param";
        var value = "test";
        var parameter = new CommandParameter<string>(name);

        parameter.Set(value);

        Assert.That(parameter.Name, Is.EqualTo(name));
        Assert.That(parameter.IsInitialized, Is.True);
        Assert.That(parameter.Value, Is.EqualTo(value));
    }

    [Test]
    public void ParameterToStringImplementationTest()
    {
        var name = "param";
        var value = "test";
        var parameter = new CommandParameter<string>(name);

        parameter.Set(value);

        Assert.That(parameter.ToString(), Is.EqualTo($"{name} = '{value}'"));;
    }
}
