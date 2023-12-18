using GithubSarifCollector.Model;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace GithubSarifCollector.Csl;

class Program
{
    static void Main(string[] args)
    {
        var host = new HostBuilder()
            .UseContentRoot(Directory.GetCurrentDirectory())
            .UseConsoleLifetime()
            .ConfigureServices(ModelDI.RegisterServices)
            .ConfigureLogging((hostingContext, builder) =>
            {
                builder.AddConsole();
            })
            .Build();

        var model = host.Services.GetRequiredService<GithubSarifCollectorModel>();
        model.CollectSarifResults(args);
    }
}
