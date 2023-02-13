using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Lib.RabbitMQ
{
    public class QueueClient
    {
        private readonly QueueConnectionSettings _config;
        private readonly ConnectionFactory _factory;
        public QueueClient(IOptions<QueueConnectionSettings> config)
        {
            _config = config.Value;
            _factory = new ConnectionFactory()
            {
                HostName = _config.HostName,
                UserName = _config.UserName,
                Password = _config.Password
            };
        }
        public void SendQueue(string queueName, string message, string exchange = "")
        {

            using (var connection = _factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                if (!string.IsNullOrEmpty(exchange))
                {
                    channel.ExchangeDeclare(exchange: exchange,
                                   type: "topic");
                }
                channel.QueueDeclare(queue: queueName,
                                     durable: false,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: null);

                var body = Encoding.UTF8.GetBytes(message);

                channel.BasicPublish(String.Empty,
                                     queueName,
                                     true,
                                     channel.CreateBasicProperties(),
                                     body);
                Console.WriteLine(" [x] Sent {0}", message);
            }
        }

        public async Task ReceivingQueue(string queueName, string exchange = "")
        {
            using (var connection = _factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {

                channel.QueueDeclare(queue: queueName,
                                     durable: false,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: null);
                channel.BasicQos(0, 1, false);

                var consumer = new EventingBasicConsumer(channel);
                Console.WriteLine("等候消息中");
                consumer.Received += async(model, ea) =>
                {
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);
                    Console.WriteLine("Received {0} ， Time = {1}", message, DateTime.Now.ToString("mm:ss fff"));
                    await Task.Delay(1000);
                    //Console.WriteLine("Wait {0} Done， Time = {1}", message, DateTime.Now.ToString("mm:ss fff"));
                };
                while (true)
                {
                    channel.BasicConsume(queue: queueName,
                                     autoAck: true,
                                     consumer: consumer);
                    await Task.Delay(100);
                }
            }
        }
    }
}