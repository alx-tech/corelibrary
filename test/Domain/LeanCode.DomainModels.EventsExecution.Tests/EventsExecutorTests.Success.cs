using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using LeanCode.DomainModels.Model;
using LeanCode.Pipelines;
using NSubstitute;
using Xunit;

namespace LeanCode.DomainModels.EventsExecution.Tests
{
    [Collection("EventsInterceptor")]
    public class EventsExecutorTests__Success
    {
        private static readonly RetryPolicies Policies = new RetryPolicies();
        private static readonly AsyncEventsInterceptor Interceptor = new AsyncEventsInterceptor();

        public EventsExecutorTests__Success()
        {
            Interceptor.Configure();
        }

        [Fact]
        public async Task Calls_the_action_method()
        {
            bool called = false;
            var ee = Prepare();

            await ee.HandleEventsOf(() =>
            {
                called = true;
                return 0;
            });

            Assert.True(called, "The action has not been called.");
        }

        [Fact]
        public async Task Sets_the_storage_before_execution()
        {
            ConcurrentQueue<IDomainEvent> queue = null;
            await Prepare().HandleEventsOf(() =>
            {
                queue = Interceptor.PeekQueue();
                return 0;
            });
            Assert.NotNull(queue);
        }

        [Fact]
        public async Task Resets_storage_after_execution()
        {
            await Prepare().HandleEventsOf(() => 0);

            Assert.Null(Interceptor.PeekQueue());
        }

        [Fact]
        public async Task Executes_handler()
        {
            var h = Substitute.For<IDomainEventHandler<Event1>>();
            var e1 = new Event1();
            await Prepare(h).HandleEventsOf(Publish(e1));

            _ = h.Received(1).HandleAsync(e1);
        }

        [Fact]
        public async Task Executes_all_handler()
        {
            var h1 = Substitute.For<IDomainEventHandler<Event1>>();
            var h2 = Substitute.For<IDomainEventHandler<Event1>>();
            var e1 = new Event1();

            await Prepare(h1, h2).HandleEventsOf(Publish(e1));

            _ = h1.Received(1).HandleAsync(e1);
            _ = h2.Received(1).HandleAsync(e1);
        }

        [Fact]
        public async Task Executes_all_handler_for_all_events()
        {
            var h1 = Substitute.For<IDomainEventHandler<Event1>>();
            var h2 = Substitute.For<IDomainEventHandler<Event1>>();
            var h3 = Substitute.For<IDomainEventHandler<Event2>>();
            var h4 = Substitute.For<IDomainEventHandler<Event2>>();

            var e1 = new Event1();
            var e2 = new Event2();

            await Prepare(h1, h2, h3, h4).HandleEventsOf(Publish(e1, e2));

            _ = h1.Received(1).HandleAsync(e1);
            _ = h2.Received(1).HandleAsync(e1);
            _ = h3.Received(1).HandleAsync(e2);
            _ = h4.Received(1).HandleAsync(e2);
        }

        [Fact]
        public async Task Executes_handlers_for_events_raised_in_handler()
        {
            var e1 = new Event1();
            var e2 = new Event2();

            var h1 = new PublishingHandler<Event1>(e2);
            var h2 = Substitute.For<IDomainEventHandler<Event2>>();

            await Prepare(h1, h2).HandleEventsOf(Publish(e1));

            _ = h2.Received(1).HandleAsync(e2);
        }

        [Fact]
        public async Task Executes_all_handlers_for_events_raised_in_handler()
        {
            var e1 = new Event1();
            var e2 = new Event2();

            var h1 = new PublishingHandler<Event1>(e2);
            var h2 = Substitute.For<IDomainEventHandler<Event2>>();
            var h3 = Substitute.For<IDomainEventHandler<Event2>>();

            await Prepare(h1, h2, h3).HandleEventsOf(Publish(e1));

            _ = h2.Received(1).HandleAsync(e2);
            _ = h3.Received(1).HandleAsync(e2);
        }

        [Fact]
        public async Task Executes_all_events_even_if_they_are_the_same()
        {
            var e1 = new Event1();
            var e2 = new Event1();

            var h1 = Substitute.For<IDomainEventHandler<Event1>>();
            var h2 = Substitute.For<IDomainEventHandler<Event1>>();

            await Prepare(h1, h2).HandleEventsOf(Publish(e1, e2));

            _ = h1.Received(1).HandleAsync(e1);
            _ = h2.Received(1).HandleAsync(e1);
            _ = h1.Received(1).HandleAsync(e2);
            _ = h2.Received(1).HandleAsync(e2);
        }

        [Fact]
        public async Task Executes_all_events_raised_in_the_event_handler_even_if_they_are_of_the_same_type()
        {
            var e1 = new Event2();
            var e2 = new Event2();

            var pub = new PublishingHandler<Event1>(1, e1, e2);
            var h1 = Substitute.For<IDomainEventHandler<Event2>>();
            var h2 = Substitute.For<IDomainEventHandler<Event2>>();

            await Prepare(pub, h1, h2).HandleEventsOf(Publish(new Event1()));

            _ = h1.Received(1).HandleAsync(e1);
            _ = h2.Received(1).HandleAsync(e1);
            _ = h1.Received(1).HandleAsync(e2);
            _ = h2.Received(1).HandleAsync(e2);
        }

        private static PipelineExecutor<Context, Func<int>, int> Prepare(params object[] handlers)
        {
            var resolver = new HandlerResolver(handlers);
            var interp = new EventsInterceptorElement<Context, Func<int>, int>(Interceptor);
            var exec = new EventsExecutorElement<Context, Func<int>, int>(Policies, Interceptor, resolver);

            var scope = Substitute.For<IPipelineScope>();
            scope.ResolveElement<Context, Func<int>, int>(interp.GetType()).Returns(interp);
            scope.ResolveElement<Context, Func<int>, int>(exec.GetType()).Returns(exec);
            scope.ResolveFinalizer<Context, Func<int>, int>(null).ReturnsForAnyArgs(new ExecFinalizer());

            var factory = Substitute.For<IPipelineFactory>();
            factory.BeginScope().Returns(scope);

            var cfg = Pipeline.Build<Context, Func<int>, int>()
                .Use<EventsExecutorElement<Context, Func<int>, int>>()
                .Use<EventsInterceptorElement<Context, Func<int>, int>>()
                .Finalize<ExecFinalizer>();
            return PipelineExecutor.Create(factory, cfg);
        }

        private static Func<int> Publish(
            params IDomainEvent[] events)
        {
            return () =>
            {
                foreach (var e in events)
                {
                    DomainEvents.Raise(e);
                }

                return 1;
            };
        }
    }
}
