using Lib.RabbitMQ;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;

namespace RabbitMQReceive
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
            var queueName = "TestQ";

            _queueClient.ReceivingQueue(queueName);
        }
    }
}
