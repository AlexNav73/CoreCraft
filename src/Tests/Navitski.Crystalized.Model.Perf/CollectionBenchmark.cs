using BenchmarkDotNet.Attributes;
using Navitski.Crystalized.Model.Engine.Core;

namespace Navitski.Crystalized.Model.Perf;

[MemoryDiagnoser]
[SimpleJob(invocationCount: InvocationCount)]
public class CollectionBenchmark
{
    private const int InvocationCount = 2_000_000;
    private const string Value = "test";

    private int _i;
    private List<FirstEntity> _entities = null!;

    private IMutableCollection<FirstEntity, FirstEntityProperties> _collection = null!;

    [IterationSetup(Targets = new[] { nameof(Add) })]
    public void IterationSetupEmpty()
    {
        _collection = new Collection<FirstEntity, FirstEntityProperties>("", id => new FirstEntity(id), () => new());
    }

    [IterationSetup(Targets = new[] { nameof(Remove), nameof(Modify), nameof(Get), nameof(Contains) })]
    public void IterationSetupFilled()
    {
        _collection = new Collection<FirstEntity, FirstEntityProperties>("", id => new FirstEntity(id), () => new());
        _i = 0;
        _entities = new List<FirstEntity>();

        for (int i = 0; i < InvocationCount; i++)
        {
            _entities.Add(_collection.Add(new()));
        }
    }

    [Benchmark]
    public void Add()
    {
        _collection.Add(new());
    }

    [Benchmark]
    public void Remove()
    {
        _collection.Remove(_entities[_i++]);
    }

    [Benchmark]
    public void Modify()
    {
        _collection.Modify(_entities[_i++], p => p with { NullableStringProperty = Value });
    }

    [Benchmark]
    public FirstEntityProperties Get()
    {
        return _collection.Get(_entities[_i++]);
    }

    [Benchmark]
    public bool Contains()
    {
        return _collection.Contains(_entities[_i++]);
    }
}
