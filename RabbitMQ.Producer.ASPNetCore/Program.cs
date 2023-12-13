using RabbitMQ.Producer.ASPNetCore;

Startup.ConfigApp(
    Startup.ConfigureHost(
        WebApplication.CreateBuilder(new WebApplicationOptions { Args = args })
    ).Build()
).Run();