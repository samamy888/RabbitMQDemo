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
        public async Task Run()
        {

            //var queueName = "[Logistics] E-Can Make Txt and Uploading";
            var queueName = "TestQ";
            var message = "Test";

            for (int i = 0; i <= _config.Times; i++)
            {
                var result = _queueClient.SendQueueAndWaitReply(queueName, message + i.ToString()).FirstAsync().ToTask().Result;
                Console.WriteLine("times = {0}", i);
                Thread.Sleep(_config.Sleep);
            }
        }
    }
}
