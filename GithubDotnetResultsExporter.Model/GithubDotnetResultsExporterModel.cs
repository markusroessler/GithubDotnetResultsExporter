using System.Text.Json;
using GithubDotnetResultsExporter.Model.Vstst;
using Microsoft.CodeAnalysis.Sarif;
using Microsoft.Extensions.DependencyInjection;
using static GithubDotnetResultsExporter.Model.GithubDotnetResultsExporterModelOps;

namespace GithubDotnetResultsExporter.Model;

public sealed class GithubDotnetResultsExporterModel
{
    private readonly FileProvider _fileProvider;
    private readonly SarifLogProvider _sarifLogProvider;
    private readonly TrxProvider _trxProvider;

    public GithubDotnetResultsExporterModel(IServiceProvider serviceProvider)
    {
        _fileProvider = serviceProvider.GetRequiredService<FileProvider>();
        _sarifLogProvider = serviceProvider.GetRequiredService<SarifLogProvider>();
        _trxProvider = serviceProvider.GetRequiredService<TrxProvider>();
    }

    public void ExportResults(string[] args)
    {
        var collectorRequest = ParseArgs(args);
        var workingDir = _fileProvider.WorkingDirectory;
        var sarifFiles = _fileProvider.EnumerateSarifFiles(workingDir);
        var sarifLogs = _sarifLogProvider.LoadSarifLogs(sarifFiles);
        var sarifResults = GetSarifResults(sarifLogs);
        var githubStepSummaryFile = _fileProvider.GithubStepSummaryFile;

        if (collectorRequest.exportChecksActionParams)
        {
            var annotationRequests = MapToAnnotationRequests(sarifResults, collectorRequest, workingDir);
            var maxLevel = GetMaxLevel(annotationRequests);

            var githubOutputFile = _fileProvider.GithubOutputFile;
            _fileProvider.AppendTextToFile(githubOutputFile, $"checks-action-conclusion={MapToConclusion(maxLevel)}\n");
            _fileProvider.AppendTextToFile(githubOutputFile, $"checks-action-output={JsonSerializer.Serialize(MapToOutput(maxLevel))}\n");
            _fileProvider.AppendTextToFile(githubOutputFile, $"checks-action-annotations={JsonSerializer.Serialize(annotationRequests)}\n");
        }

        if (collectorRequest.exportStepSummary)
        {
            var summaryMarkdown = CreateSummaryMarkdown(sarifResults, collectorRequest, workingDir);

            var trxFiles = _fileProvider.EnumerateTrxFiles(workingDir);
            var testRuns = _trxProvider.LoadTestRuns(trxFiles);
            summaryMarkdown += CreateSummaryMarkdown(testRuns);

            _fileProvider.AppendTextToFile(githubStepSummaryFile, summaryMarkdown);
        }
    }
}

internal sealed record GithubDotnetResultsExporterRequest(bool exportChecksActionParams, bool exportStepSummary, string GithubServerUrl, string GithubRepo, string GithubRefName);
