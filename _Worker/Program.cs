using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace _Worker

{
    /// <summary>
    /// 队列对应多个用户，每一个用户都去分担全部消息
    /// A producer is a user application that sends messages.
    /// A queue is a buffer that stores messages.
    /// A consumer is a user application that receives messages.
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                //task 端防止中断
                channel.QueueDeclare(queue: "task_queue", durable: true, exclusive: false, autoDelete: false, arguments: null);
                var consumer = new EventingBasicConsumer(channel);
                //空闲确认
                channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);

                consumer.Received += (model, ea) =>
                {
                    var body = ea.Body;
                    var message = Encoding.UTF8.GetString(body);
                    Console.WriteLine(" [x] Received {0}", message);

                    int dots = message.Split('.').Length - 1;
                    Thread.Sleep(dots * 1000);

                    Console.WriteLine(" [x] Done");
                    //  ctrl+c work 端防止中断
                    channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
                };
                channel.BasicConsume(queue: "task_queue", autoAck: false, consumer: consumer);
                Console.WriteLine(" Press [enter] to exit.");
                Console.ReadLine();
            }
            
        }
    }
}
