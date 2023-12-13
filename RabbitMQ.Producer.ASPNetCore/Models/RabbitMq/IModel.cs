namespace RabbitMQ.Producer.ASPNetCore.Models.RabbitMq;

public interface IModel
{
    string Subject { get; }
    Guid Identify { get; }
}