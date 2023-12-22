using GithubDotnetResultsExporter.Model;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace GithubDotnetResultsExporter.Csl;

internal class Program
{
    static void Main(string[] args) => ExportResults(args);

    internal static void ExportResults(string[] args, Action<IServiceCollection>? registerAdditionalServices = null)
    {
        var serviceCollection = new ServiceCollection();
        ModelDI.RegisterServices(serviceCollection);

        if (registerAdditionalServices != null)
            registerAdditionalServices(serviceCollection);

        serviceCollection.AddLogging(builder =>
        {
            builder.AddConsole();
        });

        using var serviceProvider = serviceCollection.BuildServiceProvider();

        var model = serviceProvider.GetRequiredService<GithubDotnetResultsExporterModel>();
        model.ExportResults(args);
    }
}
