using Lib.PosMQ;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using System;
using System.Data.Common;
using System.Reactive.Linq;
using System.Runtime.Remoting.Channels;
using System.Text;
using System.Threading.Tasks;

namespace Lib.PosMQ
{
    public class Service
    {
        private readonly QueueConnectionSettings _config;
        private IModel _channel;
        private ConnectionFactory _factory;
        private IConnection _connection;

        public Service(QueueConnectionSettings config)
        {
            _config = config;
            Connect();
        }
        public void Run()
        {
            var queueName = "TestQ";
            var message = "bar";
            SendQueue(queueName, message);
        }
        public void SendQueue(string queueName, string message, string replyTo = "")
        {

            QueueDeclare(queueName);

            var body = Encoding.UTF8.GetBytes(message);
            var properties = _channel.CreateBasicProperties();
            properties.ReplyTo = replyTo;
            properties.CorrelationId = Guid.NewGuid().ToString();
            _channel.BasicPublish(
                exchange: string.Empty,
                routingKey: queueName,
                mandatory: true,
                basicProperties: properties,
                body: body);

            Console.WriteLine("QueueName : {0} Sent {1}", queueName, message);

        }

        private void Connect()
        {
            _factory = new ConnectionFactory()
            {
                HostName = _config.HostName,
                UserName = _config.UserName,
                Password = _config.Password,
                VirtualHost = _config.VirtualHost,
            };
            _connection = _factory.CreateConnection();
            _channel = _connection.CreateModel();
            _channel.BasicQos(0, 1, false);
        }

        private void QueueDeclare(string queueName)
        {
            // 由 RabbitMQ 建立的匿名 Queue 不需要確認
            if (queueName.StartsWith("amq."))
            {
                return;
            }
            _channel.QueueDeclare(
               queue: queueName,
               durable: true,
               exclusive: false,
               autoDelete: false,
               null
           );
        }
    }
}
