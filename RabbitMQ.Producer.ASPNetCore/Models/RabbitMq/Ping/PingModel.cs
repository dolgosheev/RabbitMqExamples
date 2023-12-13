using System.Text.Json.Serialization;

namespace RabbitMQ.Producer.ASPNetCore.Models.RabbitMq.Ping;

public class PingModel : IModel
{
    [JsonPropertyName("Subject")] public string Subject { get; } = "healthcheck";
    
    [JsonIgnore]
    public Guid Identify { get; } = Guid.NewGuid();
}