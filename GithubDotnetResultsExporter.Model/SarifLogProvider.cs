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

    public IList<SarifLog> LoadSarifLogs(IEnumerable<string> files)
    {
        return files.Select(file =>
        {
            _logger.LogInformation("Loading sarif log: {File}", file);
            return SarifLog.Load(file);
        }).ToList();
    }
}
