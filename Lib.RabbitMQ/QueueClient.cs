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
        private QueueConnectionSettings _config;
        private ConnectionFactory _factory;
        private IConnection _connection;
        private IModel _channel;
        public QueueClient(IOptions<QueueConnectionSettings> config)
        {
            _config = config.Value;
            Connect(_config);
        }

        public void SendQueue(string queueName, string message, string exchange = "")
        {

            QueueDeclare(queueName);

            var body = Encoding.UTF8.GetBytes(message);
            var properties = _channel.CreateBasicProperties();
            properties.Persistent = true;
            _channel.BasicPublish(
                exchange: string.Empty,
                routingKey: queueName,
                mandatory: true,
                basicProperties: properties,
                body: body);

            Console.WriteLine(" [x] Sent {0}", message);

        }

        public async Task ReceivingQueue(string queueName, string exchange = "")
        {
            QueueDeclare(queueName);

            var consumer = new EventingBasicConsumer(_channel);
            Console.WriteLine("等候消息中");
            consumer.Received += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                Console.WriteLine("Received {0} ， Time = {1}", message, DateTime.Now.ToString("mm:ss fff"));
                await Task.Delay(1000);
            };
            while (true)
            {
                _channel.BasicConsume(queue: queueName,
                                 autoAck: true,
                                 consumer: consumer);
                await Task.Delay(100);
            }

        }

        private void Connect(QueueConnectionSettings settings)
        {
            _factory = new ConnectionFactory()
            {
                HostName = _config.HostName,
                UserName = _config.UserName,
                Password = _config.Password
            };
            _connection = _factory.CreateConnection();
            _channel = _connection.CreateModel();
            _channel.BasicQos(0, 1, false);
        }
        private void QueueDeclare(string queueName)
        {
            _channel.QueueDeclare(
                queue: queueName,
                durable: true,
                exclusive: false,
                autoDelete: false
            );
        }
    }
}