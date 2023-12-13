using System.Text;
using System.Text.Json;
using RabbitMQ.Producer.ASPNetCore.Models.RabbitMq;
using ILogger = Serilog.ILogger;

namespace RabbitMQ.Producer.ASPNetCore.Extensions;

public static class RabbitMqHelper
{
    private static readonly ILogger Log = Serilog.Log.ForContext(typeof(RabbitMqHelper));

    public static byte[]? ModelToBytes<T>(this T model) where T : IModel
    {
        try
        {
            return Encoding
                .Default
                .GetBytes(JsonSerializer.Serialize(model));
        }
        catch (Exception e)
        {
            Log.Warning("An error was occured | Exception {Exception}", e.Message);
        }

        return null;
    }
}