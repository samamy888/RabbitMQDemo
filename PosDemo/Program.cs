using System;
using System.Collections;
using System.Configuration;
namespace PosDemo
{
    public class Program
    {
        static void Main(string[] args)
        {
            //讀取AppConfig
            var dic = ConfigurationManager.GetSection("queueConnectionSettings") as IDictionary;
            var config  = new QueueConnectionSettings(
                dic["HostName"].ToString(), 
                dic["UserName"].ToString(),
                dic["Password"].ToString(), 
                dic["VirtualHost"].ToString()
                );


            Console.ReadLine();
        }
    }

}
