using System.Net;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using RabbitMQ.Producer.ASPNetCore.Extensions.SerilogEnricher;
using RabbitMQ.Producer.ASPNetCore.ServiceInterfaces;
using RabbitMQ.Producer.ASPNetCore.Services;
using RabbitMQ.Producer.ASPNetCore.Services.Daemons;
using Serilog;
using Serilog.OpenTelemetry;

namespace RabbitMQ.Producer.ASPNetCore;

/// <summary>
///     Application configuration
/// </summary>
public static class Startup
{
    public static int SyncFlagClickHouse;

    /// <summary>
    ///     Host configurator
    /// </summary>
    internal static WebApplicationBuilder ConfigureHost(WebApplicationBuilder builder)
    {
        /*
         * Logger config
         */
        builder.Host.UseSerilog((context, lc) => lc
            .Enrich.WithCaller()
            .Enrich.WithResource(
                ("server", Environment.MachineName),
                ("app", AppDomain.CurrentDomain.FriendlyName))
            .WriteTo.Console()
            .ReadFrom.Configuration(context.Configuration)
        );

        /*
         * Kestrel config
         */
        builder.WebHost.ConfigureKestrel((_, opt) =>
        {
            var appHost = builder.Configuration.GetValue<string>("App:Host");
            var http1Port = builder.Configuration.GetValue<int>("App:Ports:Http1");

            opt.Limits.MinRequestBodyDataRate = null;

            opt.Listen(IPAddress.Parse(appHost ?? "0.0.0.0"), http1Port, listenOptions =>
            {
                Log.Information(
                    "The application [{AppName}] is successfully started at [{StartTime}] (UTC) | protocol http1",
                    AppDomain.CurrentDomain.FriendlyName,
                    DateTime.UtcNow.ToString("F"));

                listenOptions.Protocols = HttpProtocols.Http1;
            });

            opt.AllowAlternateSchemes = true;
        });

        /*
         * Service registration
         */
        builder.Services.AddSingleton<IRabbitMq, RabbitMqService>();

        /*
         * Daemons
         */
        builder.Services.AddHostedService<Schedule>();
        
        return builder;
    }

    /// <summary>
    ///     Application configurator
    /// </summary>
    internal static WebApplication ConfigApp(WebApplication app)
    {
        using (var serviceScope = app.Services.GetService<IServiceScopeFactory>()?.CreateScope())
        {
            if (serviceScope != null)
            {
                /* RabbitMQ (1c) */
                var rabbitMq = serviceScope.ServiceProvider.GetRequiredService<IRabbitMq>();
                rabbitMq.Connect();
            }
        }

        Log
            .ForContext("Environment", app.Environment.EnvironmentName)
            .Debug("App activated in [{Environment}] mode", app.Environment.EnvironmentName);

        if (app.Environment.IsProduction())
            app.UseHsts();

        app.UseRouting();

        return app;
    }
}