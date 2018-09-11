using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace _RPCClient
{
    /// <summary>
    /// Rpc客户端
    /// </summary>
    public class RpcClient
    {
        private readonly IConnection connection ;
        private readonly IModel channel;
        private readonly string replyQueueName;
        private readonly EventingBasicConsumer consume;
        private readonly BlockingCollection<string> responQueue = new BlockingCollection<string>();
        private readonly IBasicProperties props;

        public RpcClient()
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            connection = factory.CreateConnection();
            channel = connection.CreateModel();
            replyQueueName = channel.QueueDeclare().QueueName;
            consume = new EventingBasicConsumer(channel);

            props = channel.CreateBasicProperties();
            string correlationId = Guid.NewGuid().ToString();
            props.CorrelationId = correlationId;
            props.ReplyTo = replyQueueName;

            consume.Received += (model, ea) => {
                var body = ea.Body;
                var respose = Encoding.UTF8.GetString(body);
                if (ea.BasicProperties.CorrelationId == correlationId)
                {
                    responQueue.Add(respose);
                }
            };

        }
        public string Call(string message)
        {
            var body = Encoding.UTF8.GetBytes(message);
            channel.BasicPublish(exchange: "", routingKey: "rpc_queue", basicProperties: props, body: body);
            channel.BasicConsume(queue: replyQueueName, autoAck: true, consumer: consume);

            return responQueue.Take();
        }
        public void Close()
        {
            connection.Close();
        }
    }
    /// <summary>
    /// Rpc主程序
    /// </summary>
    class Rpc
    {
        static void Main(string[] args)
        {
            var rpcClient = new RpcClient();
            Console.WriteLine("[x] Requesting fib(30)");
            var response = rpcClient.Call("30");
            Console.WriteLine("[.] Got '{0}", response);
            rpcClient.Close();
        }
    }
}
