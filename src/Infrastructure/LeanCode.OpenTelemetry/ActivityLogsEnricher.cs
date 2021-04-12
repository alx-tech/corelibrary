using System;
using System.Diagnostics;
using LeanCode.OpenTelemetry.Datadog;
using Serilog;
using Serilog.Configuration;
using Serilog.Core;
using Serilog.Events;

namespace LeanCode.OpenTelemetry
{
    public class ActivityLogsEnricher : ILogEventEnricher
    {
        private readonly string traceIdKey;
        private readonly string spanIdKey;
        private readonly bool useDatadogFormat;

        public ActivityLogsEnricher(string traceIdKey, string spanIdKey, bool useDatadogFormat)
        {
            this.spanIdKey = spanIdKey;
            this.useDatadogFormat = useDatadogFormat;
            this.traceIdKey = traceIdKey;
        }

        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            var activity = Activity.Current;
            if (activity != null)
            {
                LogEventProperty spanId;
                LogEventProperty traceId;

                if (useDatadogFormat)
                {
                    traceId = propertyFactory.CreateProperty(traceIdKey, activity.TraceId.ToDatadogFormat());
                    spanId = propertyFactory.CreateProperty(traceIdKey, activity.SpanId.ToDatadogFormat());
                }
                else
                {
                    traceId = propertyFactory.CreateProperty(traceIdKey, activity.TraceId.ToString());
                    spanId = propertyFactory.CreateProperty(spanIdKey, activity.SpanId.ToString());
                }

                logEvent.AddOrUpdateProperty(spanId);
                logEvent.AddOrUpdateProperty(traceId);
            }
        }
    }

    public static class LoggerEnrichmentConfigurationExtensions
    {
        /// <summary>
        /// Enrich log events with <c>SpanId</c> and <c>TraceId</c> from current activity (<see cref="Activity.Current" />)
        /// </summary>
        public static LoggerConfiguration FromCurrentActivity(
            this LoggerEnrichmentConfiguration config,
            string spanIdKey = "dd.spanId",
            string traceIdKey = "dd.traceId",
            bool useDatadogFormat = true)
        {
            return config.With(new ActivityLogsEnricher(spanIdKey, traceIdKey, useDatadogFormat));
        }
    }
}
