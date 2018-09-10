using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace _ReceiveLogsTopic
{
    class Program
    {
        static void Main(string[] args)
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.ExchangeDeclare(exchange: "topic_logs", type: "topic");
                var queueName = channel.QueueDeclare().QueueName;

                if (args.Length < 1)
                {
                    Console.WriteLine("Args Error :Usage [0] [binding keys....]",Environment.GetCommandLineArgs()[0]);
                    Console.WriteLine("Press [enter] to exit!");
                    Console.ReadLine();
                    Environment.ExitCode = 1;
                    return;

                }

                foreach(var bindingkey in args)
                {
                    channel.QueueBind(queue: queueName,exchange:"topic_logs",routingKey: bindingkey);

                }
                var consume = new EventingBasicConsumer(channel);

                consume.Received += (model, ea) => {
                    var body = ea.Body;
                    var message = Encoding.UTF8.GetString(body);
                    var routingKey = ea.RoutingKey;
                    Console.WriteLine("[x] send {0} :{1}",routingKey,message);

                };
                channel.BasicConsume(queue: queueName, autoAck: true, consumer: consume);
                Console.WriteLine("Press [enter] to exit");
                Console.ReadLine();
            }
        }
    }
}
