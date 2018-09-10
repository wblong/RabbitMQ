using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RabbitMQ.Client;

namespace _EmitLogTopic
{
    class Program
    {
        static void Main(string[] args)
        {
            var factory = new ConnectionFactory() { HostName="localhost"};
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.ExchangeDeclare(exchange: "topic_logs", type: "topic");

                var routeKey = args.Length > 0 ? args[0] : "anonymous.info";
                var message = args.Length > 1 ? String.Join(" ", args.Skip(1).ToArray()) : "Hello World";

                var body = Encoding.UTF8.GetBytes(message);

                channel.BasicPublish(exchange: "topic_logs", routingKey: routeKey, basicProperties: null, body: body);

                Console.WriteLine("[x] Send {0}:{1}",routeKey,message);
            }
            Console.ReadLine();
        }
    }
}
