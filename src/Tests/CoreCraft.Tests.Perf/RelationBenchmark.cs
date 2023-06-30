using BenchmarkDotNet.Attributes;

namespace CoreCraft.Tests.Perf;

[MemoryDiagnoser]
[SimpleJob(invocationCount: InvocationCount)]
public class RelationBenchmark
{
    private const int InvocationCount = 2_000_000;
    private const string Value = "test";

    private int _i;
    private List<FirstEntity> _parents = null!;
    private List<SecondEntity> _children = null!;

    private IMutableRelation<FirstEntity, SecondEntity> _relation = null!;

    [IterationSetup(Targets = new[] { nameof(Add) })]
    public void IterationSetupEmpty()
    {
        _relation = new Relation<FirstEntity, SecondEntity>("", new OneToMany<FirstEntity, SecondEntity>(), new OneToMany<SecondEntity, FirstEntity>());
    }

    [IterationSetup(Targets = new[]
    {
        nameof(Remove),
        nameof(Children),
        nameof(Parents),
        nameof(ContainsParent),
        nameof(ContainsChild)
    })]
    public void IterationSetupFilled()
    {
        _relation = new Relation<FirstEntity, SecondEntity>("", new OneToMany<FirstEntity, SecondEntity>(), new OneToMany<SecondEntity, FirstEntity>());
        _i = 0;
        _parents = new List<FirstEntity>();
        _children = new List<SecondEntity>();

        for (var i = 0; i < InvocationCount; i++)
        {
            var parent = new FirstEntity();
            var child = new SecondEntity();

            _parents.Add(parent);
            _children.Add(child);
            _relation.Add(parent, child);
        }
    }

    [Benchmark]
    public void Add()
    {
        _relation.Add(new(), new());
    }

    [Benchmark]
    public void Remove()
    {
        _i += 1;
        _relation.Remove(_parents[_i], _children[_i]);
    }

    [Benchmark]
    public SecondEntity[] Children()
    {
        return _relation.Children(_parents[_i++]).ToArray();
    }

    [Benchmark]
    public FirstEntity[] Parents()
    {
        return _relation.Parents(_children[_i++]).ToArray();
    }

    [Benchmark]
    public bool ContainsParent()
    {
        return _relation.ContainsParent(_parents[_i++]);
    }

    [Benchmark]
    public bool ContainsChild()
    {
        return _relation.ContainsChild(_children[_i++]);
    }
}
