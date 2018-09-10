using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace _Receive
{
    class Program
    {
        static void Main(string[] args)
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare("hello", false, false, false, null);
                Console.WriteLine("[*] Waiting for message.");
                var consume = new EventingBasicConsumer(channel);
                consume.Received += (model, ea) => {
                    var body = ea.Body;
                    var message = Encoding.UTF8.GetString(body);
                    Console.WriteLine($"Received a message:{message}");
                };
                channel.BasicConsume(queue: "hello", autoAck: true, consumer: consume);
                Console.WriteLine("Press [enter] to exit!");
                Console.ReadLine();
            }
        }
    }
}
