using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TAOTest
{  /// <summary>
/// 测试接收数据
/// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            //新建连接工厂
            var factory = new ConnectionFactory
            {
                Port = 5672,
                UserName = "trkj",
                Password = "trkj",
                HostName = "192.168.5.62",

            };
            //新建连接和通道
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                Dictionary<string, object> dic = new Dictionary<string, object>();
                dic.Add("x-message-ttl", 5000);//队列上消息过期时间，应小于队列过期时间  
                //Exchange，Queue
                channel.ExchangeDeclare("EX.TAO", "topic", true);
                channel.QueueDeclare("Q1", true, false, false, dic);
                channel.QueueBind("Q1", "EX.TAO", "RK.FLIGHT_DYN");
                Console.WriteLine("Waiting for message.");
                //新建消息回调机制
                var consume = new EventingBasicConsumer(channel);
                consume.Received += (model, ea) =>
                {
                    var body = ea.Body;
                    var message = Encoding.UTF8.GetString(body);
                    Console.WriteLine("[x] Received {0}", message);
                };
                channel.BasicConsume(queue: "Q1", noAck: true, consumer: consume);
                Console.WriteLine("Press [enter] to exit!");
                Console.ReadLine();

            }
        }
    }
}
