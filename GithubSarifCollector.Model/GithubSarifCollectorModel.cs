using System.Text.Json;
using Microsoft.CodeAnalysis.Sarif;
using Microsoft.Extensions.DependencyInjection;
using static GithubSarifCollector.Model.GithubSarifCollectorModelOps;

namespace GithubSarifCollector.Model;

public sealed class GithubSarifCollectorModel
{
    private readonly FileProvider _fileProvider;
    private readonly SarifLogProvider _sarifLogProvider;

    public GithubSarifCollectorModel(IServiceProvider serviceProvider)
    {
        _fileProvider = serviceProvider.GetRequiredService<FileProvider>();
        _sarifLogProvider = serviceProvider.GetRequiredService<SarifLogProvider>();
    }

    public void CollectSarifResults()
    {
        var workingDir = _fileProvider.WorkingDirectory;
        var sarifFiles = _fileProvider.EnumerateSarifFiles(workingDir);
        var sarifLogs = _sarifLogProvider.LoadSarifLogs(sarifFiles);
        var annotationRequests = MapToAnnotationRequests(sarifLogs,
            Environment.GetEnvironmentVariable("github.server_url"),
            Environment.GetEnvironmentVariable("github.repository"),
            Environment.GetEnvironmentVariable("github.ref_name"));
        var maxLevel = GetMaxLevel(annotationRequests);

        var githubOutputFile = _fileProvider.GithubOutputFile;
        _fileProvider.AppendTextToFile(githubOutputFile, $"checks-action-conclusion={MapToConclusion(maxLevel)}\n");
        _fileProvider.AppendTextToFile(githubOutputFile, $"checks-action-output={JsonSerializer.Serialize(MapToOutput(maxLevel))}\n");
        _fileProvider.AppendTextToFile(githubOutputFile, $"checks-action-annotations={JsonSerializer.Serialize(annotationRequests)}\n");
    }
}
