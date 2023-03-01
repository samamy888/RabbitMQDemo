using System;
using System.Collections.Generic;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Lib.RabbitMQ
{
    public class QueueMessageHandler<T>
    {
        /// <summary>
        /// Queue 訊息
        /// </summary>
        public T Message { get; }

        public string CorrelationId => BasicDeliverEventArgs.BasicProperties.CorrelationId;

        internal readonly BasicDeliverEventArgs BasicDeliverEventArgs;

        internal readonly IModel Channel;

        private bool hasAcked = false;


        public QueueMessageHandler(IModel channel, BasicDeliverEventArgs eventArgs, T message)
        {
            Channel = channel;
            BasicDeliverEventArgs = eventArgs;
            Message = message;
        }

        /// <summary>
        /// 通知 Queue server 已收到並處理訊息
        /// </summary>
        public void Ack()
        {
            if (!hasAcked)
            {
                Channel.BasicAck(BasicDeliverEventArgs.DeliveryTag, false);
            }
            hasAcked = true;
        }

        /// <summary>
        /// 通知 Queue server 拒絕此訊息，不處理
        /// </summary>
        /// <param name="requeue">是否要將拒絕的訊息重新加入佇列</param>
        public void Reject(bool requeue = false)
        {
            if (!hasAcked)
            {
                Channel.BasicReject(BasicDeliverEventArgs.DeliveryTag, requeue);
            }
            hasAcked = true;
        }

        /// <summary>
        /// 回覆物件
        /// </summary>
        /// <param name="message"></param>
        public void Reply(object message)
        {
            // 沒有指定 ReplyTo 則不處理
            if (String.IsNullOrEmpty(BasicDeliverEventArgs.BasicProperties.ReplyTo))
            {
                return;
            }

            Channel.SendObjectToQueue(
                queueName: BasicDeliverEventArgs.BasicProperties.ReplyTo, 
                message: message,
                correlationId:BasicDeliverEventArgs.BasicProperties.CorrelationId);
        }

        /// <summary>
        /// 回覆字串
        /// </summary>
        /// <param name="message"></param>
        public void Reply(string message)
        {
            // 沒有指定 ReplyTo 則不處理
            if (String.IsNullOrEmpty(BasicDeliverEventArgs.BasicProperties.ReplyTo))
            {
                return;
            }

            Channel.SendStringToQueue(
                queueName: BasicDeliverEventArgs.BasicProperties.ReplyTo,
                message: message,
                correlationId: BasicDeliverEventArgs.BasicProperties.CorrelationId);
        }

        /// <summary>
        /// 回覆 byte[]
        /// </summary>
        /// <param name="message"></param>
        public void Reply(byte[] message)
        {
            // 沒有指定 ReplyTo 則不處理
            if (String.IsNullOrEmpty(BasicDeliverEventArgs.BasicProperties.ReplyTo))
            {
                return;
            }

            Channel.SendBytesToQueue(
                queueName: BasicDeliverEventArgs.BasicProperties.ReplyTo,
                message: message,
                correlationId: BasicDeliverEventArgs.BasicProperties.CorrelationId);
        }
    }
}
