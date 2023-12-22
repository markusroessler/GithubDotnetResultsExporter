using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Sarif;
using Microsoft.Extensions.Logging;

namespace GithubDotnetResultsExporter.Model;

internal sealed class SarifLogProvider
{
    private readonly ILogger _logger;

    public SarifLogProvider(ILogger<SarifLogProvider> logger)
    {
        _logger = logger;
    }

    public IEnumerable<SarifLog> LoadSarifLogs(IEnumerable<string> files)
    {
        foreach (var file in files)
        {
            _logger.LogInformation("Loading sarif log: {file}", file);
            yield return SarifLog.Load(file);
        }
    }
}
