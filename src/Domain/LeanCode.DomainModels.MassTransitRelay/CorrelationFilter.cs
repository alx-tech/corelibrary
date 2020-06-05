using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using GreenPipes;
using LeanCode.Correlation;
using MassTransit;
using Serilog.Context;

namespace LeanCode.DomainModels.MassTransitRelay
{
    public class CorrelationFilter : IFilter<ConsumeContext>
    {
        public void Probe(ProbeContext context)
        { }

        public async Task Send(ConsumeContext context, IPipe<ConsumeContext> next)
        {
            var correlationId = context.ConversationId is Guid conversationId ?
                Correlate.Logs(conversationId) :
                null;

            var messageId = GetMessageId(context);
            var consumerType = GetConsumerType(next);

            using (messageId)
            using (correlationId)
            using (consumerType)
            {
                await next.Send(context);
            }
        }

        private static IDisposable? GetMessageId(ConsumeContext ctx)
        {
            return ctx.MessageId is Guid messageId ?
                LogContext.PushProperty("MessageId", messageId) :
                null;
        }

        private static IDisposable? GetConsumerType(IPipe<ConsumeContext> next)
        {
            var type = next.GetConsumerType();
            return type is null ? null : LogContext.PushProperty("ConsumerType", type);
        }
    }

    public class CorrelationFilterPipeSpecification : IPipeSpecification<ConsumeContext>
    {
        public void Apply(IPipeBuilder<ConsumeContext> builder)
        {
            builder.AddFilter(new CorrelationFilter());
        }

        public IEnumerable<ValidationResult> Validate()
        {
            return Enumerable.Empty<ValidationResult>();
        }
    }

    public static class CorrelationFilterExtensions
    {
        public static void UseLogsCorrelation(this IPipeConfigurator<ConsumeContext> config) =>
            config.AddPipeSpecification(new CorrelationFilterPipeSpecification());
    }
}
