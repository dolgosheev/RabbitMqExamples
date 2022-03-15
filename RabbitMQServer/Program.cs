using System;
using System.Text;
using System.Threading.Tasks;

using RabbitMQ.Client;

var factory = new ConnectionFactory
{
    HostName = "localhost",
    Port = 5672,
    UserName = "admin",
    Password = "89831143406"
    //VirtualHost = "my-rabbit"
};
using (var connection = factory.CreateConnection())
using (var channel = connection.CreateModel())
{
    channel.QueueDeclare("task_queue",
        true,
        false,
        false,
        null);
    
    var iterator = 0;
    
    var properties = channel.CreateBasicProperties();
    properties.Persistent = true;
    
    while (channel.IsOpen)
    {
        var message = $"Message {iterator} | Current data is {DateTime.Now:F}";
        var body = Encoding.UTF8.GetBytes(message);

        channel.BasicPublish(exchange: "",
            routingKey: "task_queue",
            basicProperties: properties,
            body: body);
        
        Console.WriteLine($"Sent [{iterator++}] | Message [{message}]");
        Task.Delay(1000).Wait();
    }
}

Console.ReadLine();