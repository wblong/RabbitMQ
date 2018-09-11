using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace _RPCServer
{
    /// <summary>
    /// rpc sever to call 
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                //声明队列
                channel.QueueDeclare(queue: "rpc_queue", durable: false, exclusive: false, autoDelete: false, arguments: null);
                //客户端按需分配
                channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);
                //创建消费者
                var consume = new EventingBasicConsumer(channel);
                //消息确认
                channel.BasicConsume(queue: "rpc_queue", autoAck: false, consumer: consume);

                Console.WriteLine("[x] Awaiting RPC request.");
                // received from client

                consume.Received += (model, ea) =>
                {
                    string response = null;
                    var body = ea.Body;
                    var props = ea.BasicProperties;
                    //回复属性设置
                    var replyProps = channel.CreateBasicProperties();
                    replyProps.CorrelationId = props.CorrelationId;
                    //
                    try
                    {
                        var message = Encoding.UTF8.GetString(body);
                        var n = int.Parse(message);
                        Console.WriteLine("[.] fib({0})", message);
                        response = fib(n).ToString();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("[.] " + e.Message);
                        response = "";
                    }
                    finally
                    {
                        //答复结果
                        var responesBytes = Encoding.UTF8.GetBytes(response);
                        //publish to client
                        channel.BasicPublish(exchange: "", routingKey: props.ReplyTo, basicProperties: replyProps, body: responesBytes);
                        //？
                        channel.BasicAck(deliveryTag:ea.DeliveryTag, multiple: false);
                    }
                };

                Console.WriteLine("Press [enter] to exit.");
                Console.ReadLine();
            }
        }
        /// <summary>
        /// RPC call function
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        private static int fib(int n)
        {
            if (n == 0 || n == 1)
            {
                return n;
            }
            return fib(n - 1) + fib(n - 2);
        }
    }
}
