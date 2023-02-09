using Lib.RabbitMQ;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;

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
            var queueName = "TestQ";

            for(int i = 0; i <= _config.Times; i++)
            {
                _queueClient.SendQueue(queueName, i.ToString());
                Console.WriteLine("times = {0}", i);
                Thread.Sleep(_config.Sleep);
            }

        }
    }
}
