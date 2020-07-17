using System;
using System.Threading.Tasks;
using LeanCode.DomainModels.MassTransitRelay.Middleware;
using LeanCode.Pipelines;

namespace LeanCode.DomainModels.MassTransitRelay.Simple
{
    public sealed class SimpleEventsExecutor
    {
        private readonly PipelineExecutor<SimplePipelineContext, Func<Task>, ValueTuple> exec;

        private static ConfiguredPipeline<SimplePipelineContext, Func<Task>, ValueTuple> Classic() => Pipeline
            .Build<SimplePipelineContext, Func<Task>, ValueTuple>()
            .Use<EventsPublisherElement<SimplePipelineContext, Func<Task>, ValueTuple>>()
            .Finalize<SimpleFinalizer>();

        private static ConfiguredPipeline<SimplePipelineContext, Func<Task>, ValueTuple> Outboxed() => Pipeline
            .Build<SimplePipelineContext, Func<Task>, ValueTuple>()
            .Use<StoreAndPublishEventsElement<SimplePipelineContext, Func<Task>, ValueTuple>>()
            .Finalize<SimpleFinalizer>();

        public SimpleEventsExecutor(IPipelineFactory factory, bool useOutbox)
        {
            var config = useOutbox ? Outboxed() : Classic();
            exec = PipelineExecutor.Create(factory, config);
        }

        public Task HandleEventsOf(Func<Task> action, Guid? correlationId = null) =>
            exec.ExecuteAsync(
                new SimplePipelineContext
                {
                    ExecutionId = correlationId ?? Guid.NewGuid(),
                    CorrelationId = Guid.NewGuid(),
                }, action);
    }
}
