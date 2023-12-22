using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace GithubDotnetResultsExporter.Model;

public static class ModelDI
{
    public static void RegisterServices(IServiceCollection services)
    {
        services
            .AddTransient<FileProvider>()
            .AddTransient<SarifLogProvider>()
            .AddTransient<TrxProvider>()
            .AddSingleton<IEnvironment, DefaultEnvironment>()
            .AddSingleton<GithubDotnetResultsExporterModel>();
    }
}
