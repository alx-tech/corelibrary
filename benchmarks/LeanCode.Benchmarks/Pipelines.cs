using System.Threading.Tasks;
using Autofac;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using LeanCode.Benchmarks.Pipelines;
using LeanCode.Pipelines;
using LeanCode.Pipelines.Autofac;
using Executor = LeanCode.Pipelines.PipelineExecutor<
    LeanCode.Benchmarks.Pipelines.Context,
    LeanCode.Benchmarks.Pipelines.Input,
    LeanCode.Benchmarks.Pipelines.Output
>;

namespace LeanCode.Benchmarks;

[SimpleJob(RuntimeMoniker.NetCoreApp31)]
[MarkdownExporterAttribute.Atlassian]
[MemoryDiagnoser]
public class PipelinesBenchmark
{
    private Executor emptyStatic;
    private Executor singleElementStatic;
    private Executor emptyAutofac;
    private Executor singleElementAutofac;

    [GlobalSetup]
    public void Setup()
    {
        var emptyCfg = Pipeline.Build<Context, Input, Output>().Finalize<Finalizer>();
        var singleCfg = Pipeline.Build<Context, Input, Output>().Use<PassthroughElement>().Finalize<Finalizer>();

        var builder = new ContainerBuilder();
        builder.RegisterType<AutofacPipelineFactory>().AsSelf();
        builder.RegisterType<Finalizer>().AsSelf().SingleInstance();
        builder.RegisterType<PassthroughElement>().AsSelf().SingleInstance();
        var container = builder.Build();

        emptyStatic = new Executor(new StaticFactory(), emptyCfg);
        singleElementStatic = new Executor(new StaticFactory(), singleCfg);
        emptyAutofac = new Executor(container.Resolve<AutofacPipelineFactory>(), emptyCfg);
        singleElementAutofac = new Executor(container.Resolve<AutofacPipelineFactory>(), singleCfg);
    }

    [Benchmark(Baseline = true)]
    public Task<Output> Empty() => emptyStatic.ExecuteAsync(new Context(), new Input());

    [Benchmark]
    public Task<Output> SingleElement() => singleElementStatic.ExecuteAsync(new Context(), new Input());

    [Benchmark]
    public Task<Output> EmptyAutofac() => emptyAutofac.ExecuteAsync(new Context(), new Input());

    [Benchmark]
    public Task<Output> SingleElementAutofac() => singleElementAutofac.ExecuteAsync(new Context(), new Input());
}
