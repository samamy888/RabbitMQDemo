using Lib.RabbitMQ;
using Microsoft.Extensions.Options;
using System;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;

namespace RabbitMQSend
{
    public class App
    {
        private readonly QueueConnectionSettings _config;
        private readonly QueueClient _queueClient;

        public App(IOptions<QueueConnectionSettings> config, QueueClient queueClient)
        {
            _config = config.Value;
            _queueClient = queueClient;
        }
        public void Run()
        {

            //var queueName = "[Logistics] E-Can Make Txt and Uploading";
            var queueName = "TestQ";
            var message = "bar";
            var queueName2 = "TestQ2";
            var message2 = "foo";

            var result = _queueClient.SendQueueAndWaitReply(queueName, message)
                .FirstAsync()
                .ToTask()
                .Result
                .Message;

            Console.WriteLine(result);

            result = _queueClient.SendQueueAndWaitReply(queueName2, message2)
                .FirstAsync()
                .ToTask()
                .Result
                .Message;

            Console.WriteLine(result);
        }
    }
}
