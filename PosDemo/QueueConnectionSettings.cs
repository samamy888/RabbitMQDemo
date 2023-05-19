using System.Configuration;

public class QueueConnectionSettings
{
    public QueueConnectionSettings(string hostName, string userName, string password, string virtualHost)
    {
        this.HostName = hostName;
        this.UserName = userName;
        this.Password = password;
        this.VirtualHost = virtualHost;
    }
    public string HostName { get; set; }
    public string UserName { get; set; }
    public string Password { get; set; }
    public string VirtualHost { get; set; }
}