namespace RabbitMQ.Producer.ASPNetCore.ServiceInterfaces;

public interface IRabbitMq
{
    void Connect();
    Task Ping(TimeSpan taskRepeatInterval, CancellationToken token);
    bool PublishMessage(byte[] data);
}