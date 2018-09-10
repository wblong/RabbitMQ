using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace _ReceiveLogs
{
    class Program
    {
        static void Main(string[] args)
        {
            var factory = new ConnectionFactory();
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                //exchange iden
                channel.ExchangeDeclare(exchange: "logs", type: "fanout");

                var queueName = channel.QueueDeclare().QueueName;
                //bind
                channel.QueueBind(queueName, "logs", "");
                Console.WriteLine("Waiting for message.");
                var consume = new EventingBasicConsumer(channel);

                consume.Received += (model, ea) =>
                {
                    var body = ea.Body;
                    var message = Encoding.UTF8.GetString(body);
                    Console.WriteLine("[x] Received {0}", message);
                };
                channel.BasicConsume(queue: queueName, autoAck: true, consumer: consume);
                Console.WriteLine("Press [enter] to exit!");
                Console.ReadLine();
            }
            
        }
    }
}
