using Newtonsoft.Json;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TruckTest
{
    public class TrunkInfo
    {
        /// <summary>
        /// 采集时间
        /// </summary>
        public string CollectTime { get; set; }
        /// <summary>
        /// 车头排号
        /// </summary>
        public string Name { get; set; }
        public double Lat { get; set; }
        public double Lon { get; set; }
        /// <summary>
        /// 目的地
        /// </summary>
        public string Dest { get; set; }
        /// <summary>
        /// 出勤挂车号
        /// </summary>
        public string TrailerNo { get; set; }
        /// <summary>
        /// 用户ID
        /// </summary>
        public string UserId { get; set; }
        /// <summary>
        /// 出勤前车号
        /// </summary>
        public string FrontNo { get; set; }
        /// <summary>
        /// 进出港类型（0：进港1：出港）
        /// </summary>
        public string InOutType { get; set; }
        /// <summary>
        /// 是否到达目的地
        /// </summary>
        public string IsArrivedDest { get; set; }
        /// <summary>
        /// 预计时间
        /// </summary>
        public string Pta { get; set; }
        /// <summary>
        /// 实际时间
        /// </summary>
        public string Rta { get; set; }
    }
    /// <summary>
    /// 测试发送数据
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("模拟集卡数据........");
            //新建连接工厂
            var factory = new ConnectionFactory
            {
                Port = 5672,
                UserName = "trkj",
                Password = "trkj",
                HostName = "172.27.101.100",
                
            };
            //新建连接和通道
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                Dictionary<string, object> dic = new Dictionary<string, object>();
                dic.Add("x-message-ttl", 5000); 
                channel.ExchangeDeclare("E_TR_TRUNK", "fanout",true);
                channel.QueueDeclare("TR_TRUNK", true, false, false, dic);
                channel.QueueBind("TR_TRUNK", "E_TR_TRUNK", "E_TR_TRUNK.TR_TRUNK");

                while (true)
                {
                    var m = new TrunkInfo()
                    {
                        CollectTime = DateTime.Now.ToString(),
                        Name = "18701852079",
                        Lat = 30.656518,
                        Lon = 122.023278,
                        Dest = "18701852079",
                        TrailerNo = "挂KHBA",
                        UserId = "18701852079",
                        FrontNo = "沪A68725",
                        InOutType ="进港",
                        IsArrivedDest ="false",
                        Pta = DateTime.Now.ToString(),
                        Rta = DateTime.Now.ToString()
                    };
                    List<TrunkInfo> truck = new List<TrunkInfo>();
                    truck.Add(m);
                    var message = JsonConvert.SerializeObject(truck);
                    
                    var body = Encoding.UTF8.GetBytes(message);
                    //发送消息
                    channel.BasicPublish(exchange: "E_TR_TRUNK", routingKey: "E_TR_TRUNK.TR_TRUNK", basicProperties: null, body: body);
                    Thread.Sleep(1000);
                }
                
            }
            
        }
    }
}
