using GithubSarifCollector.Model;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace GithubSarifCollector.Csl;

class Program
{
    static void Main(string[] args)
    {
        var serviceCollection = new ServiceCollection();
        ModelDI.RegisterServices(serviceCollection);
        serviceCollection.AddLogging(builder =>
        {
            builder.AddConsole();
        });
        using var serviceProvider = serviceCollection.BuildServiceProvider();

        var model = serviceProvider.GetRequiredService<GithubSarifCollectorModel>();
        model.CollectSarifResults(args);
    }
}
