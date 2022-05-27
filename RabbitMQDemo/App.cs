using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;

namespace RabbitMQDemo
{
    public class App
    {
        private readonly QueueConnectionSettings _config;
        private readonly Services _services;

        public App(IOptions<QueueConnectionSettings> config, Services services)
        {
            _config = config.Value;
            _services = services;
        }
        public async Task Run()
        {
            var queueName = "Hello";
            var message = "Hello world";
#if DEBUG
            _services.SendQueue(queueName, message);
#else
            _services.ReceivingQueue(queueName);
#endif
        }
    }
}
