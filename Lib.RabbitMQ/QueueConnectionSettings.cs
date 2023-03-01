using System;
using System.Collections.Generic;
using System.Text;

namespace Lib.RabbitMQ
{
    public class QueueConnectionSettings
    {
        public string HostName { get; set; }

        public string UserName { get; set; }

        public string Password { get; set; }

        public bool IsOfficial { get; set; }
    }
}
