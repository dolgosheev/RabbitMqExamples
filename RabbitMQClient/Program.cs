using System.Text;

using RabbitMQ.Client;
using RabbitMQ.Client.Events;

var factory = new ConnectionFactory
{
    HostName = "localhost",
    Port = 5672,
    UserName = "admin",
    Password = "89831143406",
    //VirtualHost = "my-rabbit"
};
using var connection = factory.CreateConnection();
using var channel = connection.CreateModel();

// channel.QueueDeclare("task_queue",
//     true,
//     false,
//     false,
//     null);
//
// channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);

channel.ExchangeDeclare(exchange: "logs2", type: ExchangeType.Direct);
var queueName = channel.QueueDeclare().QueueName;
channel.QueueBind(queue: queueName,
    exchange: "logs2",
    routingKey: "");

Console.WriteLine(" [*] Waiting for messages.");

var consumer = new EventingBasicConsumer(channel);
consumer.Received += (_, ea) =>
{
    var body = ea.Body.ToArray();
    var message = Encoding.UTF8.GetString(body);
    Console.WriteLine("Received {0}", message);
    
    //channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
};
channel.BasicConsume(queue: queueName,
    autoAck: true,
    consumer: consumer);

Console.WriteLine(" Press [enter] to exit.");
Console.ReadLine();