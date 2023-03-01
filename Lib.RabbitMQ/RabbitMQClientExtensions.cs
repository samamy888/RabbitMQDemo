using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Lib.RabbitMQ
{
    public static class RabbitMQClientExtensions
    {
        /// <summary>
        /// 將訊息送到 Queue
        /// </summary>
        /// <param name="channel">Channel 實體</param>
        /// <param name="queueName">Queue 名稱</param>
        /// <param name="message">物件訊息</param>
        /// <param name="replyTo">回覆 Queue 名稱</param>
        /// <param name="correlationId"></param>
        public static void SendObjectToQueue(
            this IModel channel,
            string queueName,
            object message,
            string replyTo = "",
            string correlationId = "")
        {
            var body = JsonSerializer.Serialize(message);
            channel.SendStringToQueue(queueName, body, replyTo, correlationId);
        }

        /// <summary>
        /// 將訊息送到 Queue
        /// </summary>
        /// <param name="channel">Channel 實體</param>
        /// <param name="queueName">Queue 名稱</param>
        /// <param name="message">字串訊息</param>
        /// <param name="replyTo">回覆 Queue 名稱</param>
        /// <param name="correlationId"></param>
        public static void SendStringToQueue(
            this IModel channel,
            string queueName,
            string message,
            string replyTo = "",
            string correlationId = "")
        {
            var body = Encoding.UTF8.GetBytes(message);
            channel.SendBytesToQueue(queueName, body, replyTo, correlationId);
        }

        /// <summary>
        /// 將訊息送到 Queue
        /// </summary>
        /// <param name="channel">Channel 實體</param>
        /// <param name="queueName">Queue 名稱</param>
        /// <param name="message">訊息 bytes</param>
        /// <param name="replyTo">回覆 Queue 名稱</param>
        /// <param name="correlationId"></param>
        public static void SendBytesToQueue(
            this IModel channel,
            string queueName,
            byte[] message,
            string replyTo = "",
            string correlationId = "")
        {
            IBasicProperties properties;
            lock (channel)
            {
                properties = channel.CreateBasicProperties();
            }

            properties.ReplyTo = replyTo;
            properties.CorrelationId = correlationId;

            if (String.IsNullOrEmpty(properties.ReplyTo))
            {
                properties.Persistent = true;
            }

            channel.MakeSureQueueCreated(queueName);

            lock (channel)
            {
                channel.BasicPublish(String.Empty, queueName, true, properties, message);
            }
        }

        /// <summary>
        /// 從 Queue 接收訊息
        /// </summary>
        /// <param name="channel">Channel 實體</param>
        /// <param name="queueName">Queue 名稱</param>
        /// <param name="autoAck">自動 Acknowledge；預設為 false</param>
        /// <param name="onlyOnce">只接收一次訊息，用於接收回覆訊息；預設為 false</param>
        /// <returns></returns>
        public static IObservable<QueueMessageHandler<byte[]>> ReceiveFromQueue(
            this IModel channel,
            string queueName,
            bool autoAck = false,
            bool onlyOnce = false)
        {
            channel.MakeSureQueueCreated(queueName);

            var subject = new ReplaySubject<QueueMessageHandler<byte[]>>();

            var consumer = new EventingBasicConsumer(channel);

            consumer.Received += (model, eventArgs) =>
            {
                var body = eventArgs.Body.ToArray();

                try
                {
                    var handler = new QueueMessageHandler<byte[]>(channel, eventArgs, body);
                    subject.OnNext(handler);
                }
                catch
                {
                    var handler = new QueueMessageHandler<byte[]>(channel, eventArgs, body);
                    subject.OnNext(handler);
                }

                if (onlyOnce)
                {
                    subject.OnCompleted();
                }
            };

            lock (channel)
            {
                channel.BasicConsume(
                    queueName,
                    autoAck: autoAck,
                    consumer: consumer);
            }

            return subject.AsObservable();
        }

        /// <summary>
        /// 確保 Queue 被建立
        /// </summary>
        /// <param name="channel">Channel 實體</param>
        /// <param name="queueName">Queue 名稱</param>
        public static void MakeSureQueueCreated(this IModel channel, string queueName)
        {
            // 由 RabbitMQ 建立的匿名 Queue 不需要確認
            if (queueName.StartsWith("amq."))
            {
                return;
            }
            lock (channel)
            {
                channel.QueueDeclare(
                    queue: queueName,
                    durable: true,
                    exclusive: false,
                    autoDelete: false);
            }
        }

        /// <summary>
        /// 將收到的訊息放入指定的 Subject
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="source"></param>
        /// <param name="correlationId"></param>
        /// <param name="subject"></param>
        /// <returns></returns>
        public static IObservable<QueueMessageHandler<TResult>> MessageReplyToSubject<TResult>(
            this IObservable<QueueMessageHandler<TResult>> source,
            string correlationId,
            ReplaySubject<QueueMessageHandler<TResult>> subject)
        {
            source
                .Where(result => result.BasicDeliverEventArgs.BasicProperties.CorrelationId == correlationId)
                .Take(1)
                .Subscribe(subject);
            return source;
        }

        /// <summary>
        /// 轉換 QueueMessageHandler 內的訊息資訊 (通常用於轉型)
        /// </summary>
        /// <typeparam name="TSource">轉換前的資料型別</typeparam>
        /// <typeparam name="TResult">轉換後的資料型別</typeparam>
        /// <param name="source"></param>
        /// <param name="channel"></param>
        /// <param name="func">轉換 function</param>
        /// <returns></returns>
        public static IObservable<QueueMessageHandler<TResult>> ConvertMessageHandler<TSource, TResult>(
            this IObservable<QueueMessageHandler<TSource>> source,
            IModel channel,
            Func<TSource, TResult> func
        )
        {
            return source
                .Select(handler =>
                    new QueueMessageHandler<TResult>(
                        channel,
                        handler.BasicDeliverEventArgs,
                        func(handler.Message))
                );
        }
    }
}
