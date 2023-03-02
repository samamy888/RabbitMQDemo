using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lib.RabbitMQ
{
    public class QueueHandler
    {
        internal readonly IModel _channel;
        internal readonly BasicDeliverEventArgs _ea;
        public readonly string Message;

        public QueueHandler(IModel channel, BasicDeliverEventArgs ea,string message)
        {
            _channel = channel;
            _ea = ea;
            Message = message;
        }
        public void Ack()
        {
            _channel.BasicAck(_ea.DeliveryTag, false);
        }
        public void Reply(string message)
        {
            // 沒有指定 ReplyTo 則不處理
            if (String.IsNullOrEmpty(_ea.BasicProperties.ReplyTo))
            {
                return;
            }

            var replyProps = _channel.CreateBasicProperties();
            replyProps.CorrelationId = _ea.BasicProperties.CorrelationId;
            var body = Encoding.UTF8.GetBytes(message);

            _channel.BasicPublish(
                exchange: string.Empty,
                routingKey: _ea.BasicProperties.ReplyTo,
                mandatory: true,
                basicProperties: replyProps,
                body: body
            );
        }
    }
}
