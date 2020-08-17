using System;
using System.Text.Json.Serialization;
using LeanCode.Components;
using LeanCode.DomainModels.MassTransitRelay.Outbox;
using LeanCode.DomainModels.Model;
using LeanCode.IdentityProvider;
using LeanCode.Time;
using Xunit;

namespace LeanCode.DomainModels.MassTransitRelay.Tests
{
    public abstract class BaseJsonEventsSerializerTests
    {
        protected static readonly TypesCatalog Types = TypesCatalog.Of<BaseJsonEventsSerializerTests>();

        private static readonly Guid EventId = Identity.NewId();
        private static readonly Guid CorrelationId = Identity.NewId();
        private static readonly DateTime DateOccurred = new(2020, 5, 7, 11, 0, 0, 0, DateTimeKind.Utc);

        protected abstract IRaisedEventsSerializer Serializer { get; }

        [Fact]
        public void Serializes_an_event_keeps_metadata()
        {
            var evt = new TestEvent(EventId, DateOccurred, 5);

            var raised = Serializer.WrapEvent(evt);

            Assert.Equal(EventId, raised.Id);
            Assert.Equal(Guid.Empty, raised.CorrelationId); // TODO: fixme
            Assert.Equal(DateOccurred, raised.DateOcurred);
            Assert.Equal(typeof(TestEvent).FullName, raised.EventType);
            Assert.False(raised.WasPublished);
            Assert.Contains(@"""Value"":5", raised.Payload);
        }

        [Fact]
        public void Deserializes_event()
        {
            var raisedEvt = new RaisedEvent(
                EventId,
                CorrelationId,
                DateOccurred,
                false,
                typeof(TestEvent).FullName,
                @"{ ""Value"":5 }");

            var evt = Serializer.ExtractEvent(raisedEvt);

            var typed = evt as TestEvent;
            Assert.NotNull(typed);
            Assert.Equal(5, typed.Value);
        }

        [Fact]
        public void Serializes_and_deserializes_events_correctly()
        {
            var evt = new TestEvent(EventId, DateOccurred, 5);

            var wrapped = Serializer.WrapEvent(evt);
            var unwrapped = Serializer.ExtractEvent(wrapped);

            Assert.Equal(evt, unwrapped);
        }

        [Fact]
        public void Serializes_and_deserializes_events_with_private_setters_and_no_ctor_correctly()
        {
            var evt = TestEventWithPrivateFields.Create("test value");

            var wrapped = Serializer.WrapEvent(evt);
            var unwrapped = Serializer.ExtractEvent(wrapped);

            Assert.Equal(evt, unwrapped);
        }

        public record TestEvent(Guid Id, DateTime DateOccurred, int Value) : IDomainEvent;

        public record TestEventWithPrivateFields : IDomainEvent
        {
            public Guid Id { get; private init; }
            public DateTime DateOccurred { get; private init; }
            public string Value { get; private init; }

            public static TestEventWithPrivateFields Create(string val) => new(val);

            private TestEventWithPrivateFields(string val)
            {
                Id = Identity.NewId();
                DateOccurred = TimeProvider.Now;
                Value = val;
            }

            private TestEventWithPrivateFields()
            {
                Value = string.Empty;
            }
        }
    }

    public sealed class NewtonsoftJsonEventsSerializerTests : BaseJsonEventsSerializerTests
    {
        protected override IRaisedEventsSerializer Serializer { get; } = new NewtonsoftJsonEventsSerializer(Types);
    }

    // public sealed class SystemTextJsonEventsSerializerTests : BaseJsonEventsSerializerTests
    // {
    //     protected override IRaisedEventsSerializer Serializer { get; } = new SystemTextJsonEventsSerializer(Types);
    // }
}
