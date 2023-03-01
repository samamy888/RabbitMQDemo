using System;
using System.Diagnostics;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Microsoft.AspNetCore.Http;
using System.IO;

namespace Lib.RabbitMQ
{
    public class QueueClient : IDisposable,
        IQueueClient,
        IQueueClientMembers,
        IQueueClientCart,
        IQueueClientCheckout,
        IQueueClientGoods,
        IQueueClientOrder,
        IQueueClientPayment,
        IQueueClientRegularCustomer,
        IQueueClientShipping,
        IQueueClientStory,
        IQueueClientTrack,
        IQueueClientDelivery,
        IQueueClientInvoice,
        IQueueClientOrderDelivery,
        IQueueClientOrderPayment,
        IQueueClientKnowledge
    {

        private readonly ILogger<QueueClient> logger;

        /// <summary>
        /// Queue 連線
        /// </summary>
        private ConnectionFactory ConnectionFactory { get; set; }

        /// <summary>
        /// 連線後的物件實體
        /// </summary>
        private IConnection Connection { get; set; }

        /// <summary>
        /// 建立的 channel 實體
        /// </summary>
        public IModel Channel { get; set; }

        public QueueClient()
        {
            Connect("localhost", "", "");
        }

        public QueueClient(string host, string username, string password)
        {
            Connect(host, username, password);
        }

        public QueueClient(IOptions<QueueConnectionSettings> configuration, ILogger<QueueClient> logger)
        {
            this.logger = logger;

            Connect(
                configuration.Value.HostName,
                configuration.Value.UserName,
                configuration.Value.Password,
                configuration.Value.IsOfficial
            );
        }

        private void LogDebug(string message, params object[] args)
        {
            logger?.LogDebug(message, args);
        }

        /// <summary>
        /// 建立 Queue 連線
        /// </summary>
        private void Connect(string hostName = "localhost", string userName = "", string password = "", bool isOfficial = false)
        {
            ConnectionFactory = new ConnectionFactory { HostName = hostName };
            ConnectionFactory.AutomaticRecoveryEnabled = true;
            if (!string.IsNullOrEmpty(userName))
            {
                ConnectionFactory.UserName = userName;
                ConnectionFactory.Password = password;
            }

            if (isOfficial)
            {
                ConnectionFactory.VirtualHost = "official";
            }

            Connection = ConnectionFactory.CreateConnection("API");

            LogDebug("Connection 已建立");

            Connection.ConnectionBlocked += Connection_ConnectionBlocked;
            Connection.CallbackException += Connection_CallbackException;
            Connection.ConnectionShutdown += Connection_ConnectionShutdown;
            Connection.ConnectionUnblocked += Connection_ConnectionUnblocked;

            Channel = Connection.CreateModel();
            Channel.BasicQos(0, 1, false);

            LogDebug("Channel 已建立");

            Channel.CallbackException += Channel_CallbackException;
            Channel.BasicAcks += Channel_BasicAcks;
            Channel.BasicNacks += Channel_BasicNacks;
            Channel.BasicRecoverOk += Channel_BasicRecoverOk;
            Channel.BasicReturn += Channel_BasicReturn;
            Channel.FlowControl += Channel_FlowControl;
            Channel.ModelShutdown += Channel_ModelShutdown;
        }

        private void Connection_ConnectionUnblocked(object sender, EventArgs e)
        {
            LogDebug("{ServerName} ConnectionEvent {ConnectionEvent}", "WebAPI", "ConnectionUnblocked");
        }

        private void Connection_ConnectionShutdown(object sender, ShutdownEventArgs e)
        {
            LogDebug("{ServerName} ConnectionEvent {ConnectionEvent}: {ReplyCode} {ReplyText}", "WebAPI", "ConnectionShutdown", e.ReplyCode, e.ReplyText);
        }

        private void Connection_ConnectionBlocked(object sender, ConnectionBlockedEventArgs e)
        {
            LogDebug("{ServerName} ConnectionEvent {ConnectionEvent}: {Reason}", "WebAPI", "ConnectionBlocked", e.Reason);
        }

        private void Connection_CallbackException(object sender, CallbackExceptionEventArgs e)
        {
            LogDebug("{ServerName} ConnectionEvent {ConnectionEvent}: {ExceptionMessage}", "WebAPI", "CallbackException", e.Exception.Message);
        }

        private void Channel_ModelShutdown(object sender, ShutdownEventArgs e)
        {
            LogDebug("{ServerName} ChannelEvent {ChannelEvent}: {ReasonCode} {ReasonText}", "WebAPI", "ModelShutdown", e.ReplyCode, e.ReplyText);
        }

        private void Channel_FlowControl(object sender, FlowControlEventArgs e)
        {
            LogDebug("{ServerName} ChannelEvent {ChannelEvent}: {Active}", "WebAPI", "FlowControl", e.Active);
        }

        private void Channel_BasicReturn(object sender, BasicReturnEventArgs e)
        {
            LogDebug("{ServerName} ChannelEvent {ChannelEvent}: {ReasonCode} {ReasonText}", "WebAPI", "BasicReturn ", e.ReplyCode, e.ReplyText);
        }

        private void Channel_BasicRecoverOk(object sender, EventArgs e)
        {
            LogDebug("{ServerName} ChannelEvent {ChannelEvent}", "WebAPI", "BasicRecoverOk");
        }

        private void Channel_BasicNacks(object sender, BasicNackEventArgs e)
        {
            LogDebug("{ServerName} ChannelEvent {ChannelEvent}: DeliveryTag = {DeliveryTag}, Multiple = {Multiple} Requeue = {Requeue}",
                "WebAPI",
                "BasicAcks",
                e.DeliveryTag,
                e.Multiple,
                e.Requeue);
        }

        private void Channel_BasicAcks(object sender, BasicAckEventArgs e)
        {
            LogDebug("{ServerName} ChannelEvent {ChannelEvent}: DeliveryTag = {DeliveryTag} , Multiple = {Multiple}",
                "WebAPI",
                "BasicAcks",
                e.DeliveryTag,
                e.Multiple);
        }

        private void Channel_CallbackException(object sender, CallbackExceptionEventArgs e)
        {
            LogDebug("{ServerName} ChannelEvent {ChannelEvent}: Detail = {Detail} , Exception = {Exception}",
                "WebAPI",
                "CallbackException",
                e.Detail,
                e.Exception);
        }

        /// <summary>
        /// 設定一次可從 Queue 擷取多少訊息
        /// </summary>
        /// <param name="prefetchCount"></param>
        public void SetPrefetchCount(ushort prefetchCount)
        {
            Channel.BasicQos(0, prefetchCount, false);
        }

        /// <summary>
        /// 發送物件訊息給 Queue
        /// </summary>
        /// <param name="queueName"></param>
        /// <param name="message"></param>
        /// <param name="correlationId"></param>
        public void SendObjectToQueue(string queueName, object message, string correlationId = "")
        {
            Channel.SendObjectToQueue(queueName, message, correlationId: correlationId);
        }

        /// <summary>
        /// 發送文字訊息給 Queue
        /// </summary>
        public void SendStringToQueue(string queueName, string message, string correlationId = "")
        {
            Channel.SendStringToQueue(queueName, message, correlationId: correlationId);
        }

        /// <summary>
        /// 發送 byte 陣列訊息給 Queue
        /// </summary>
        public void SendBytesToQueue(string queueName, byte[] message, string correlationId = "")
        {
            Channel.SendBytesToQueue(queueName, message, correlationId: correlationId);
        }

        /// <summary>
        /// 發送通知給 Queue
        /// </summary>
        public void SendNotifyToQueue(string queueName, string correlationId = "")
        {
            Channel.SendBytesToQueue(queueName, new byte[] { }, correlationId);
        }

        /// <summary>
        /// 送出物件訊息，並等待物件回傳
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="queueName"></param>
        /// <param name="message"></param>
        /// <param name="correlationId"></param>
        /// <returns></returns>
        public IObservable<QueueMessageHandler<TResult>> SendObjectAndWaitObjectReply<TResult>(
            string queueName,
            object message,
            string correlationId = "") where TResult : class
        {
                return SendStringAndWaitObjectReply<TResult>(
                    queueName,
                    JsonSerializer.Serialize(message),
                    correlationId);
        }

        /// <summary>
        /// 送出字串訊息，並等待物件回傳
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="queueName"></param>
        /// <param name="message"></param>
        /// <param name="correlationId"></param>
        /// <returns></returns>
        public IObservable<QueueMessageHandler<TResult>> SendStringAndWaitObjectReply<TResult>(
            string queueName,
            string message,
            string correlationId = "") where TResult : class
        {
            return SendBytesAndWaitObjectReply<TResult>(
                queueName,
                Encoding.UTF8.GetBytes(message),
                correlationId);
        }

        /// <summary>
        /// 等待 Queue 回傳物件
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="queueName"></param>
        /// <param name="correlationId"></param>
        /// <returns></returns>
        public IObservable<QueueMessageHandler<TResult>> WaitObjectReply<TResult>(
            string queueName,
            string correlationId = "") where TResult : class
        {
            return SendStringAndWaitObjectReply<TResult>(queueName, String.Empty, correlationId);
        }

        /// <summary>
        /// 送出 byte[] 訊息，並等待物件回傳
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="queueName"></param>
        /// <param name="message"></param>
        /// <param name="correlationId"></param>
        /// <returns></returns>
        public IObservable<QueueMessageHandler<TResult>> SendBytesAndWaitObjectReply<TResult>(
            string queueName,
            byte[] message,
            string correlationId = "")
            where TResult : class
        {
            var correlationIdToSend = string.IsNullOrEmpty(correlationId) ? Guid.NewGuid().ToString() : correlationId;
            string replyQueue;
            lock (Channel)
            {
                replyQueue = Channel.QueueDeclare().QueueName;
            }

            var subject = new ReplaySubject<QueueMessageHandler<TResult>>(1);
            ListenObjectFromQueue<TResult>(replyQueue, autoAck: true, onlyOnce: true)
                .MessageReplyToSubject(correlationIdToSend, subject)
                .Subscribe(result =>
                {
                    LogDebug("QueueAction {QueueAction} Start: {ReplyQueue}", "Delete ReplyQueue", replyQueue);
                    Channel.QueueDelete(replyQueue);
                    LogDebug("QueueAction {QueueAction} Done: {ReplyQueue}", "Delete ReplyQueue", replyQueue);
                });

            LogDebug("QueueAction {QueueAction} Start: {QueueName}, ReplyQueue: {ReplyQueue}", "Send Bytes To Queue", queueName, replyQueue, message);
            Channel.SendBytesToQueue(queueName, message, replyQueue, correlationIdToSend);
            LogDebug("QueueAction {QueueAction} Done: {QueueName}, ReplyQueue: {ReplyQueue}", "Send Bytes To Queue", queueName, replyQueue, message);

            return subject.AsObservable();
        }

        /// <summary>
        /// 送出物件訊息，並等待字串回傳
        /// </summary>
        /// <param name="queueName"></param>
        /// <param name="message"></param>
        /// <param name="correlationId"></param>
        /// <returns></returns>
        public IObservable<QueueMessageHandler<string>> SendObjectAndWaitStringReply(
            string queueName,
            object message,
            string correlationId = "")
        {
            return SendStringAndWaitStringReply(queueName, JsonSerializer.Serialize(message), correlationId);
        }

        public IObservable<QueueMessageHandler<string>> SendIFormFileAndWaitStringReply(
           string queueName,
           IFormFile message,
           string correlationId = "")
        {
            using (var ms = new MemoryStream())
            {
                message.CopyTo(ms);
                var fileBytes = ms.ToArray();
                return SendBytesAndWaitStringReply(queueName, fileBytes, correlationId);
            }
        }

        /// <summary>
        /// 送出字串訊息，並等待字串回傳
        /// </summary>
        /// <param name="queueName"></param>
        /// <param name="message"></param>
        /// <param name="correlationId"></param>
        /// <returns></returns>
        public IObservable<QueueMessageHandler<string>> SendStringAndWaitStringReply(
            string queueName,
            string message,
            string correlationId = "")
        {
            return SendBytesAndWaitStringReply(queueName, Encoding.UTF8.GetBytes(message), correlationId);
        }

        /// <summary>
        /// 等待 Queue 回傳字串
        /// </summary>
        /// <param name="queueName"></param>
        /// <returns></returns>
        public IObservable<QueueMessageHandler<string>> WaitStringReply(string queueName)
        {
            return SendStringAndWaitStringReply(queueName, String.Empty);
        }

        /// <summary>
        /// 送出 byte[] 訊息，並等待字串回傳
        /// </summary>
        /// <param name="queueName"></param>
        /// <param name="message"></param>
        /// <param name="correlationId"></param>
        /// <returns></returns>
        public IObservable<QueueMessageHandler<string>> SendBytesAndWaitStringReply(string queueName, byte[] message, string correlationId = "")
        {
            var correlationIdToSend = String.IsNullOrEmpty(correlationId) ? Guid.NewGuid().ToString() : correlationId;

            string replyQueue;
            lock (Channel)
            {
                replyQueue = Channel.QueueDeclare().QueueName;
            }

            var subject = new ReplaySubject<QueueMessageHandler<string>>(1);
            ListenStringFromQueue(replyQueue, autoAck: true, onlyOnce: true)
                .MessageReplyToSubject(correlationIdToSend, subject)
                .Subscribe(result =>
                {
                    LogDebug("QueueAction {QueueAction} Start: {ReplyQueue}", "Delete ReplyQueue", replyQueue);
                    lock (Channel)
                    {
                        Channel.QueueDelete(replyQueue);
                    }

                    LogDebug("QueueAction {QueueAction} Done: {ReplyQueue}", "Delete ReplyQueue", replyQueue);
                });

            LogDebug("QueueAction {QueueAction} Start: {QueueName}, ReplyQueue: {ReplyQueue}", "Send Bytes To Queue", queueName, replyQueue, message);
            Channel.SendBytesToQueue(queueName, message, replyQueue, correlationIdToSend);
            LogDebug("QueueAction {QueueAction} Done: {QueueName}, ReplyQueue: {ReplyQueue}", "Send Bytes To Queue", queueName, replyQueue, message);

            return subject.AsObservable();
        }

        /// <summary>
        /// 送出物件訊息，並等待 byte[] 回傳
        /// </summary>
        /// <param name="queueName"></param>
        /// <param name="message"></param>
        /// <param name="correlationId"></param>
        /// <returns></returns>
        public IObservable<QueueMessageHandler<byte[]>> SendObjectAndWaitBytesReply(string queueName, object message, string correlationId = "")
        {
            return SendStringAndWaitBytesReply(queueName, JsonSerializer.Serialize(message), correlationId);
        }

        /// <summary>
        /// 送出字串訊息，並等待 byte[] 回傳
        /// </summary>
        /// <param name="queueName"></param>
        /// <param name="message"></param>
        /// <param name="correlationId"></param>
        /// <returns></returns>
        public IObservable<QueueMessageHandler<byte[]>> SendStringAndWaitBytesReply(string queueName, string message, string correlationId = "")
        {
            return SendBytesAndWaitBytesReply(queueName, Encoding.UTF8.GetBytes(message), correlationId);
        }

        /// <summary>
        /// 等待 Queue 回傳 byte[]
        /// </summary>
        /// <param name="queueName"></param>
        /// <returns></returns>
        public IObservable<QueueMessageHandler<byte[]>> WaitBytesReply(string queueName)
        {
            return SendStringAndWaitBytesReply(queueName, String.Empty);
        }

        /// <summary>
        /// 送出 byte[] 訊息，並等待 byte[] 回傳
        /// </summary>
        /// <param name="queueName"></param>
        /// <param name="message"></param>
        /// <param name="correlationId"></param>
        /// <returns></returns>
        public IObservable<QueueMessageHandler<byte[]>> SendBytesAndWaitBytesReply(
            string queueName,
            byte[] message,
            string correlationId = "")
        {
            var correlationIdToSend = String.IsNullOrEmpty(correlationId) ? Guid.NewGuid().ToString() : correlationId;
            string replyQueue;
            lock (Channel)
            {
                replyQueue = Channel.QueueDeclare().QueueName;
            }

            var subject = new ReplaySubject<QueueMessageHandler<byte[]>>(1);
            ListenBytesFromQueue(replyQueue, autoAck: true, onlyOnce: true)
                .MessageReplyToSubject(correlationIdToSend, subject)
                .Subscribe(result =>
                {
                    LogDebug("QueueAction {QueueAction} Start: {ReplyQueue}", "Delete ReplyQueue", replyQueue);
                    Channel.QueueDelete(replyQueue);
                    LogDebug("QueueAction {QueueAction} Done: {ReplyQueue}", "Delete ReplyQueue", replyQueue);
                });

            LogDebug("QueueAction {QueueAction} Start: {QueueName}, ReplyQueue: {ReplyQueue}", "Send Bytes To Queue", queueName, replyQueue, message);
            Channel.SendBytesToQueue(queueName, message, replyQueue, correlationIdToSend);
            LogDebug("QueueAction {QueueAction} Done: {QueueName}, ReplyQueue: {ReplyQueue}", "Send Bytes To Queue", queueName, replyQueue, message);

            return subject.AsObservable();
        }

        /// <summary>
        /// 從 Queue 取得物件訊息
        /// </summary>
        /// <typeparam name="TResult">物件類型</typeparam>
        /// <param name="queueName">Queue 名稱</param>
        /// <param name="autoAck">自動 Acknowledge；預設為 false</param>
        /// <param name="onlyOnce">只接收一次訊息，用於接收回覆訊息；預設為 false</param>
        /// <returns>物件訊息 Observable</returns>
        public IObservable<QueueMessageHandler<TResult>> ListenObjectFromQueue<TResult>(
            string queueName,
            bool autoAck = false,
            bool onlyOnce = false)
            where TResult : class
        {
            return ListenStringFromQueue(queueName, autoAck, onlyOnce)
                .ConvertMessageHandler(
                    Channel,
                    message => JsonSerializer.Deserialize<TResult>(message));
        }

        /// <summary>
        /// 從 Queue 取得文字訊息
        /// </summary>
        /// <param name="queueName">Queue 名稱</param>
        /// <param name="autoAck">自動 Acknowledge；預設為 false</param>
        /// <param name="onlyOnce">只接收一次訊息，用於接收回覆訊息；預設為 false</param>
        /// <returns>文字訊息 Observable</returns>
        public IObservable<QueueMessageHandler<string>> ListenStringFromQueue(
            string queueName,
            bool autoAck = false,
            bool onlyOnce = false)
        {
            return ListenBytesFromQueue(queueName, autoAck, onlyOnce)
                .ConvertMessageHandler(
                    Channel,
                    message => Encoding.UTF8.GetString(message));
        }

        /// <summary>
        /// 從 Queue 取得 byte 陣列訊息
        /// </summary>
        /// <param name="queueName">Queue 名稱</param>
        /// <param name="autoAck">自動 Acknowledge；預設為 false</param>
        /// <param name="onlyOnce">只接收一次訊息，用於接收回覆訊息；預設為 false</param>
        /// <returns>byte 陣列 Observable</returns>
        public IObservable<QueueMessageHandler<byte[]>> ListenBytesFromQueue(string queueName, bool autoAck = false, bool onlyOnce = false)
        {
            return Channel.ReceiveFromQueue(queueName, autoAck, onlyOnce);
        }

        public void Dispose()
        {
            Channel?.Close();
            Connection?.Close();

            Connection?.Dispose();
            Channel?.Dispose();
        }
    }
}