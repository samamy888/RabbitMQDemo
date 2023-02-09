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
                //channel.QueueDeclare(queue: queueName,
                //                     durable: false,
                //                     exclusive: false,
                //                     autoDelete: false,
                //                     arguments: null);

                var body = Encoding.UTF8.GetBytes(message);

                channel.BasicPublish(String.Empty,
                                     queueName,
                                     true,
                                     channel.CreateBasicProperties(),
                                     body);
                Console.WriteLine(" [x] Sent {0}", message);
            }
        }

        public void ReceivingQueue(string queueName, string exchange = "")
        {
            using (var connection = _factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {

                channel.QueueDeclare(queue: queueName,
                                     durable: false,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: null);

                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += (model, ea) =>
                {
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);
                    Console.WriteLine(" [x] Received {0}", message);
                };
                while (true)
                {
                    channel.BasicConsume(queue: queueName,
                                     autoAck: true,
                                     consumer: consumer);
                    Thread.Sleep(100);
                }

            }
            Console.WriteLine(" Press [enter] to exit.");
            Console.ReadLine();
        }
    }
}