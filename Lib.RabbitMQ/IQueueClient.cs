using Microsoft.AspNetCore.Http;
using System;

namespace Lib.RabbitMQ
{
    public interface IQueueClientMembers : IQueueClient { }
    public interface IQueueClientCart : IQueueClient { }
    public interface IQueueClientCheckout : IQueueClient { }
    public interface IQueueClientGoods : IQueueClient { }
    public interface IQueueClientOrder : IQueueClient { }
    public interface IQueueClientPayment : IQueueClient { }
    public interface IQueueClientRegularCustomer : IQueueClient { }
    public interface IQueueClientShipping : IQueueClient { }
    public interface IQueueClientStory : IQueueClient { }
    public interface IQueueClientTrack : IQueueClient { }
    public interface IQueueClientDelivery : IQueueClient { }
    public interface IQueueClientInvoice : IQueueClient { }
    public interface IQueueClientOrderDelivery : IQueueClient { }
    public interface IQueueClientOrderPayment : IQueueClient { }
    public interface IQueueClientKnowledge : IQueueClient { }


    public interface IQueueClient
    {
        /// <summary>
        /// 設定一次可從 Queue 擷取多少訊息
        /// </summary>
        /// <param name="prefetchCount"></param>
        void SetPrefetchCount(ushort prefetchCount);

        /// <summary>
        /// 發送物件訊息給 Queue
        /// </summary>
        /// <param name="queueName"></param>
        /// <param name="message"></param>
        /// <param name="correlationId"></param>
        void SendObjectToQueue(string queueName, object message, string correlationId = "");

        /// <summary>
        /// 發送文字訊息給 Queue
        /// </summary>
        void SendStringToQueue(string queueName, string message, string correlationId = "");

        /// <summary>
        /// 發送 byte 陣列訊息給 Queue
        /// </summary>
        void SendBytesToQueue(string queueName, byte[] message, string correlationId = "");

        /// <summary>
        /// 發送通知給 Queue
        /// </summary>
        void SendNotifyToQueue(string queueName, string correlationId = "");

        /// <summary>
        /// 送出物件訊息，並等待物件回傳
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="queueName"></param>
        /// <param name="message"></param>
        /// <param name="correlationId"></param>
        /// <returns></returns>
        IObservable<QueueMessageHandler<TResult>> SendObjectAndWaitObjectReply<TResult>(
            string queueName, 
            object message,
            string correlationId = "") where TResult : class;

        /// <summary>
        /// 送出字串訊息，並等待物件回傳
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="queueName"></param>
        /// <param name="message"></param>
        /// <param name="correlationId"></param>
        /// <returns></returns>
        IObservable<QueueMessageHandler<TResult>> SendStringAndWaitObjectReply<TResult>(
            string queueName, 
            string message,
            string correlationId = "") where TResult : class;

        /// <summary>
        /// 等待 Queue 回傳物件
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="queueName"></param>
        /// <param name="correlationId"></param>
        /// <returns></returns>
        IObservable<QueueMessageHandler<TResult>> WaitObjectReply<TResult>(
            string queueName,
            string correlationId = "") where TResult: class;

        /// <summary>
        /// 送出 byte[] 訊息，並等待物件回傳
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="queueName"></param>
        /// <param name="message"></param>
        /// <param name="correlationId"></param>
        /// <returns></returns>
        IObservable<QueueMessageHandler<TResult>> SendBytesAndWaitObjectReply<TResult>(
            string queueName,
            byte[] message, 
            string correlationId = "")
            where TResult : class;

        /// <summary>
        /// 送出物件訊息，並等待字串回傳
        /// </summary>
        /// <param name="queueName"></param>
        /// <param name="message"></param>
        /// <param name="correlationId"></param>
        /// <returns></returns>
        IObservable<QueueMessageHandler<string>> SendObjectAndWaitStringReply(
            string queueName, 
            object message,
            string correlationId = "");

        /// <summary>
        /// 送出字串訊息，並等待字串回傳
        /// </summary>
        /// <param name="queueName"></param>
        /// <param name="message"></param>
        /// <param name="correlationId"></param>
        /// <returns></returns>
        IObservable<QueueMessageHandler<string>> SendStringAndWaitStringReply(
            string queueName, 
            string message, 
            string correlationId = "");

        IObservable<QueueMessageHandler<string>> SendIFormFileAndWaitStringReply(
            string queueName,
            IFormFile message,
            string correlationId = "");
        /// <summary>
        /// 等待 Queue 回傳字串
        /// </summary>
        /// <param name="queueName"></param>
        /// <returns></returns>
        IObservable<QueueMessageHandler<string>> WaitStringReply(string queueName);

        /// <summary>
        /// 送出 byte[] 訊息，並等待字串回傳
        /// </summary>
        /// <param name="queueName"></param>
        /// <param name="message"></param>
        /// <param name="correlationId"></param>
        /// <returns></returns>
        IObservable<QueueMessageHandler<string>> SendBytesAndWaitStringReply(string queueName, byte[] message, string correlationId = "");

        /// <summary>
        /// 送出物件訊息，並等待 byte[] 回傳
        /// </summary>
        /// <param name="queueName"></param>
        /// <param name="message"></param>
        /// <param name="correlationId"></param>
        /// <returns></returns>
        IObservable<QueueMessageHandler<byte[]>> SendObjectAndWaitBytesReply(string queueName, object message, string correlationId = "");

        /// <summary>
        /// 送出字串訊息，並等待 byte[] 回傳
        /// </summary>
        /// <param name="queueName"></param>
        /// <param name="message"></param>
        /// <param name="correlationId"></param>
        /// <returns></returns>
        IObservable<QueueMessageHandler<byte[]>> SendStringAndWaitBytesReply(string queueName, string message, string correlationId = "");

        /// <summary>
        /// 等待 Queue 回傳 byte[]
        /// </summary>
        /// <param name="queueName"></param>
        /// <returns></returns>
        IObservable<QueueMessageHandler<byte[]>> WaitBytesReply(string queueName);

        /// <summary>
        /// 送出 byte[] 訊息，並等待 byte[] 回傳
        /// </summary>
        /// <param name="queueName"></param>
        /// <param name="message"></param>
        /// <param name="correlationId"></param>
        /// <returns></returns>
        IObservable<QueueMessageHandler<byte[]>> SendBytesAndWaitBytesReply(
            string queueName,
            byte[] message,
            string correlationId = "");

        /// <summary>
        /// 從 Queue 取得物件訊息
        /// </summary>
        /// <typeparam name="TResult">物件類型</typeparam>
        /// <param name="queueName">Queue 名稱</param>
        /// <param name="autoAck">自動 Acknowledge；預設為 false</param>
        /// <param name="onlyOnce">只接收一次訊息，用於接收回覆訊息；預設為 false</param>
        /// <returns>物件訊息 Observable</returns>
        IObservable<QueueMessageHandler<TResult>> ListenObjectFromQueue<TResult>(
            string queueName,
            bool autoAck = false,
            bool onlyOnce = false)
            where TResult : class;

        /// <summary>
        /// 從 Queue 取得文字訊息
        /// </summary>
        /// <param name="queueName">Queue 名稱</param>
        /// <param name="autoAck">自動 Acknowledge；預設為 false</param>
        /// <param name="onlyOnce">只接收一次訊息，用於接收回覆訊息；預設為 false</param>
        /// <returns>文字訊息 Observable</returns>
        IObservable<QueueMessageHandler<string>> ListenStringFromQueue(
            string queueName,
            bool autoAck = false,
            bool onlyOnce = false);

        /// <summary>
        /// 從 Queue 取得 byte 陣列訊息
        /// </summary>
        /// <param name="queueName">Queue 名稱</param>
        /// <param name="autoAck">自動 Acknowledge；預設為 false</param>
        /// <param name="onlyOnce">只接收一次訊息，用於接收回覆訊息；預設為 false</param>
        /// <returns>byte 陣列 Observable</returns>
        IObservable<QueueMessageHandler<byte[]>> ListenBytesFromQueue(string queueName, bool autoAck = false, bool onlyOnce = false);
    }
}