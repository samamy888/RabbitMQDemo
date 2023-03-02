using Lib.RabbitMQ;
using Microsoft.Extensions.Options;
using System;
using System.Reactive.Linq;
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
            Console.WriteLine("等候消息中");

            _queueClient.ReceivingQueue("TestQ")
                .Subscribe(handler =>
                {
                    // do something....
                    Console.WriteLine($"Message : {handler.Message}");
                    handler.Ack();
                    handler.Reply("OK!!");
                });

            _queueClient.ReceivingQueue("TestQ2")
                .Subscribe(handler =>
                {
                    // do something....
                    Console.WriteLine($"Message : {handler.Message}");
                    handler.Ack();
                    handler.Reply("Error!!");
                });

            while (true)
            {
                await Task.Delay(100);
            }
        }
    }
}
