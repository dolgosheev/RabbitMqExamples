using RabbitMQ.Client;
using RabbitMQ.Client.Events;

using Serializer;

var factory = new ConnectionFactory
{
    HostName = "localhost",
    Port = 5672,
    UserName = "admin",
    Password = "89831143406"
    //VirtualHost = "my-rabbit"
};
using var connection = factory.CreateConnection();
using var channel = connection.CreateModel();
channel.QueueDeclare("task_queue",
    true,
    false,
    false,
    null);

channel.BasicQos(0, 1, false);
Console.WriteLine(" [*] Waiting for messages.");

var consumer = new EventingBasicConsumer(channel);
consumer.Received += (_, ea) =>
{
    var body = ea.Body.ToArray();
    //var message = Encoding.UTF8.GetString(body);
    //Console.WriteLine("Received {0}", message);

    var evtDeserialize = Data.DeserializeFromStream<ExtendData>(body);
    Console.WriteLine(
        $"Id {evtDeserialize?.Id} | Time {evtDeserialize?.TimeStamp} | Message [{evtDeserialize?.Message}]");

    channel.BasicAck(ea.DeliveryTag, false);
};
channel.BasicConsume("task_queue",
    false,
    consumer);

Console.WriteLine(" Press [enter] to exit.");
Console.ReadLine();