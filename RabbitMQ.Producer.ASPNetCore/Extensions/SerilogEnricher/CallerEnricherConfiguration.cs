using Serilog;
using Serilog.Configuration;

namespace RabbitMQ.Producer.ASPNetCore.Extensions.SerilogEnricher;

public static class CallerEnrichmentConfiguration
{
    public static LoggerConfiguration WithCaller(this LoggerEnrichmentConfiguration enrichmentConfiguration)
    {
        return enrichmentConfiguration.With<CallerEnricher>();
    }
}