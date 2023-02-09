using Lib.RabbitMQ;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RabbitMQReceive;
using System.Threading.Tasks;

static void ConfigureServices(IServiceCollection services)
{
    // configure logging
    services.AddLogging(builder =>
    {
        builder.AddConsole();
        builder.AddDebug();
    });

    // build config
    IConfiguration builder = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .AddEnvironmentVariables()
    .Build();

    // add app
    services.AddScoped<QueueClient>();
    services.Configure<QueueConnectionSettings>(builder.GetSection("QueueConnectionSettings"));
    services.AddTransient<App>();
}

var services = new ServiceCollection();
ConfigureServices(services);

using var serviceProvider = services.BuildServiceProvider();

// entry to run app
await serviceProvider.GetService<App>().Run();