
using RabbitMQ.Producer.ASPNetCore.ServiceInterfaces;

namespace RabbitMQ.Producer.ASPNetCore.Services.Daemons;

/// <summary>
///     Фоновый процесс получения
///     - Агентов
///     и заполнения ими БД
/// </summary>
public class Schedule : BackgroundService
{
    private readonly ILogger<Schedule> _logger;

    public Schedule(IServiceProvider services, ILogger<Schedule> logger)
    {
        Services = services;
        _logger = logger;
    }

    private IServiceProvider Services { get; }

    protected override async Task ExecuteAsync(CancellationToken token)
    {
        _logger.LogInformation("Scheduler started at {DateTime} UTC", DateTime.UtcNow);
        await StartSchedule(token);
    }

    public override async Task StopAsync(CancellationToken token)
    {
        _logger.LogInformation("Scheduler stopped at {DateTime} UTC", DateTime.UtcNow);
        await base.StopAsync(token);
    }

    /// <summary>
    ///     Циклическое выполнение фоновых заданий
    /// </summary>
    /// <param name="token">Токен отмены</param>
    private async Task StartSchedule(CancellationToken token)
    {

        using var scope = Services.CreateScope();
        var rabbitMq = scope.ServiceProvider.GetRequiredService<IRabbitMq>();
        
        var taskList = new List<Task>
        {
            /*
             * HealthCheck механизма отправки в rmq
             * раз в 1 минуту
             */
            rabbitMq.Ping(TimeSpan.FromSeconds(5), token),
        };
        
        await Task.WhenAll(taskList);
    }
}