using GithubDotnetResultsExporter.Model;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace GithubDotnetResultsExporter.Csl;

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

        var model = serviceProvider.GetRequiredService<GithubDotnetResultsExporterModel>();
        model.CollectSarifResults(args);
    }
}
