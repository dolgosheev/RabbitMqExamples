
using System.Collections.Concurrent;
using RabbitMQ.Client;
using RabbitMQ.Producer.ASPNetCore.Extensions;
using RabbitMQ.Producer.ASPNetCore.Models.RabbitMq.Ping;
using RabbitMQ.Producer.ASPNetCore.ServiceInterfaces;

namespace RabbitMQ.Producer.ASPNetCore.Services;

// https://www.rabbitmq.com/tutorials/tutorial-seven-dotnet.html
public class RabbitMqService : IRabbitMq
{
    private readonly string? _exchange;
    private readonly string? _host;
    private readonly ILogger<RabbitMqService> _logger;

    private readonly string? _password;
    private readonly int _port;
    private readonly string? _user;
    private readonly string? _virtualHost;
    private IModel? _channel;
    private IConnection? _connection;
    private ConcurrentDictionary<ulong, RabbitMQ.Producer.ASPNetCore.Models.RabbitMq.IModel> _dictionary = new();
    
    public RabbitMqService(ILogger<RabbitMqService> logger, IConfiguration configuration)
    {
        _logger = logger;
        _host = configuration.GetValue<string>("RabbitMQ:Host");
        _virtualHost = configuration.GetValue<string>("RabbitMQ:VirtualHost");
        _port = configuration.GetValue<int>("RabbitMQ:Port");
        _user = configuration.GetValue<string>("RabbitMQ:User");
        _password = configuration.GetValue<string>("RabbitMQ:Password");
        _exchange = configuration.GetValue<string>("RabbitMQ:Exchange");
    }

    public void Connect()
    {
        #region Connect/Observe/Reconnect

        var factory = new ConnectionFactory
        {
            HostName = _host,
            Port = _port,
            UserName = _user,
            Password = _password,
            //VirtualHost = _virtualHost,
            // Ssl = new SslOption
            // {
            //     Enabled = true,
            //     Version = SslProtocols.Tls12,
            //     AcceptablePolicyErrors = SslPolicyErrors.RemoteCertificateNameMismatch |
            //                              SslPolicyErrors.RemoteCertificateChainErrors
            // }
        };

        try
        {
            _connection?.Dispose();
            _channel?.Dispose();
            _connection = factory.CreateConnection();
        }
        catch (Exception)
        {
            _logger.LogError("[RabbitMQ] reconnection attempt at {Datetime} (UTC)", DateTime.UtcNow.ToString("F"));
            Thread.Sleep(2000);
            Connect();
            return;
        }

        // Observer connection (Reconnect)
        _connection.ConnectionShutdown += (_, _) =>
        {
            _logger.LogWarning("[RabbitMQ] connection lost at {Datetime} (UTC)", DateTime.UtcNow.ToString("F"));
            _connection?.Dispose();
            _channel?.Dispose();
            Connect();
        };

        // Established
        _logger.LogInformation("[RabbitMQ] connection established | {Datetime} (UTC)", DateTime.UtcNow.ToString("F"));

        #endregion

        if (!OpenChannels()) OpenChannels();
    }

    public async Task Ping(TimeSpan taskRepeatInterval, CancellationToken token)
    {
        while (!token.IsCancellationRequested)
            try
            {
                if (_channel is null)
                {
                    _logger.LogWarning("[Rabbit] Ping was not send at {PingTime} (UTC)", DateTime.UtcNow.ToString("F"));
                    continue;
                }

                if (!_channel.IsOpen)
                {
                    _logger.LogWarning("[Rabbit] Ping was not send at {PingTime} (UTC)", DateTime.UtcNow.ToString("F"));
                    continue;
                }

                var model = new PingModel();
                var convertedMessage = model.ModelToBytes();

                if (convertedMessage is null)
                {
                    _logger.LogWarning("[Rabbit] Ping was not send at {PingTime} (UTC)", DateTime.UtcNow.ToString("F"));
                    continue;
                }

                var sequenceNumber = _channel.NextPublishSeqNo;
                _dictionary.TryAdd(sequenceNumber, model);
                _channel.BasicPublish(_exchange,
                    string.Empty,
                    null,
                    convertedMessage);
                
                _logger.LogInformation("[Rabbit] Ping ({SeqNumber}) was send at {PingTime} (UTC)", sequenceNumber,DateTime.UtcNow.ToString("F"));
            }
            catch (Exception e)
            {
                _logger.LogWarning("[Rabbit] Ping was not send at {PingTime} (UTC) | Exception {Exception}",
                    DateTime.UtcNow.ToString("F"), e.Message);
            }
            finally
            {
                await Task.Delay(taskRepeatInterval, token);
            }
    }

    public bool PublishMessage(byte[] data)
    {
        try
        {
            if (_channel is null)
            {
                _logger.LogError("Channel is null");
                return false;
            }

            if (!_channel.IsOpen)
            {
                _logger.LogWarning("[Rabbit] Message was published at {PingTime} (UTC)", DateTime.UtcNow.ToString("F"));
                return false;
            }

            _channel.BasicPublish(_exchange,
                string.Empty,
                null,
                data);

            _logger.LogInformation("[Rabbit] Message was published at {PingTime} (UTC)", DateTime.UtcNow.ToString("F"));
        }
        catch (Exception e)
        {
            _logger.LogWarning("[Rabbit] Message was not published at {DateTime} (UTC) | Exception {Exception}",
                DateTime.UtcNow.ToString("F"), e.Message);
            return false;
        }

        return true;
    }

    private bool OpenChannels()
    {
        _logger.LogInformation("[RabbitMQ] trying subscribe to channel {Channel} at {Datetime} (UTC)", _exchange,
            DateTime.UtcNow.ToString("F"));

        _channel = _connection?.CreateModel();
        

        if (_channel is null)
        {
            _logger.LogWarning("[RabbitMQ] channel {Channel} unsubscribed at {Datetime} (UTC)", _exchange,
                DateTime.UtcNow.ToString("F"));
            Thread.Sleep(2000);
            OpenChannels();
            return false;
        }

        _channel.ConfirmSelect();
        _channel.BasicAcks += (sender, ea) =>
        {
            _dictionary.TryGetValue(ea.DeliveryTag, out var message);
            _logger.LogInformation("Delivered at {DateTime} (UTC) | seq number {SeqNumber} | Identify [{Guid}]",DateTime.UtcNow.ToString("yyyy-MM-dd"),ea.DeliveryTag,message?.Identify);
            _dictionary.TryRemove(ea.DeliveryTag, out var _);
        };
        _channel.BasicNacks += (sender, ea) =>
        {
            _dictionary.TryGetValue(ea.DeliveryTag, out var message);
            _logger.LogInformation("Not delivered at {DateTime} (UTC) | seq number {SeqNumber} | Identify [{Guid}]",DateTime.UtcNow.ToString("yyyy-MM-dd"),ea.DeliveryTag,message?.Identify);
            _dictionary.TryRemove(ea.DeliveryTag, out var _);
        };
        
        _logger.LogInformation("[RabbitMQ] channel {Channel} subscribed at {Datetime} (UTC)", _exchange,
            DateTime.UtcNow.ToString("F"));

        return true;
    }
}