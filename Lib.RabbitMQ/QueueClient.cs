using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
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
            Connect();
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

            Console.WriteLine("{0} Sent {1}", queueName, message);

        }

        public IObservable<string> ReceivingQueue(string queueName, bool listener = false)
        {
            QueueDeclare(queueName);

            var consumer = new EventingBasicConsumer(_channel);
            var subject = new ReplaySubject<string>();
            var replyQueue = _channel.QueueDeclare().QueueName;
            consumer.Received += (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);

                if (listener)
                {
                    var replyProps = _channel.CreateBasicProperties();
                    replyProps.CorrelationId = ea.BasicProperties.CorrelationId;
                    _channel.BasicPublish(
                        exchange: string.Empty,
                        routingKey: ea.BasicProperties.ReplyTo,
                        mandatory: true,
                        basicProperties: replyProps,
                        body: body
                    );
                }

                subject.OnNext(message);
                Console.WriteLine("Received {0} ， Time = {1}", message, DateTime.Now.ToString("mm:ss fff"));
            };

            _channel.BasicConsume(
                queue: queueName,
                autoAck: true,
                consumer: consumer
            );

            return subject.AsObservable();
        }

        public IObservable<string> SendQueueAndWaitReply(string queueName, string message)
        {
            var subject = new ReplaySubject<string>();
            var replyQueue = _channel.QueueDeclare().QueueName;
            SendQueue(queueName, message, replyQueue);
            ReceivingQueue(replyQueue).Subscribe(x =>
                subject.OnNext(x)
            );
            return subject.AsObservable();
        }
        private void Connect()
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
            // 由 RabbitMQ 建立的匿名 Queue 不需要確認
            if (queueName.StartsWith("amq."))
            {
                return;
            }
            _channel.QueueDeclare(
                queue: queueName,
                durable: true,
                exclusive: false,
                autoDelete: false
            );
        }
    }
}