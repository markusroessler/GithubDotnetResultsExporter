using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Sarif;

namespace GithubSarifCollector.Model;

internal static class GithubSarifCollectorModelOps
{
    internal static GithubSarifCollectorRequest ParseArgs(string[] args)
    {
        var exportChecksActionParams = false;
        var exportStepSummary = false;
        string? githubServerUrl = null;
        string? githubRepo = null;
        string? githubRefName = null;

        for (int i = 0; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "--export-checks-action-params":
                    exportChecksActionParams = Convert.ToBoolean(args.ElementAtOrDefault(++i));
                    break;
                case "--export-step-summary":
                    exportStepSummary = Convert.ToBoolean(args.ElementAtOrDefault(++i));
                    break;
                case "--github-server-url":
                    githubServerUrl = args.ElementAtOrDefault(++i);
                    break;
                case "--github-repo":
                    githubRepo = args.ElementAtOrDefault(++i);
                    break;
                case "--github-ref-name":
                    githubRefName = args.ElementAtOrDefault(++i);
                    break;
                default:
                    throw new Exception($"unknown arg {args[i]}");
            }
        }

        if (githubServerUrl == null)
            throw new Exception("missing argument: --github-server-url");

        if (githubRepo == null)
            throw new Exception("missing argument: --github-repo");

        if (githubRefName == null)
            throw new Exception("missing argument: --github-ref-name");

        return new(exportChecksActionParams, exportStepSummary, githubServerUrl, githubRepo, githubRefName);
    }

    internal static IList<Result> GetSarifResults(IEnumerable<SarifLog> sarifLogs)
    {
        return sarifLogs
            .SelectMany(log => log.Results())
            .OrderByDescending(result => result.Level)
            .ToList();
    }

    internal static IList<GithubAnnotationRequest> MapToAnnotationRequests(IList<Result> sarifResults, GithubSarifCollectorRequest collectorRequest, string workingDirectory)
    {
        return sarifResults
            .Select(request => MapToGithubAnnotationRequest(request, collectorRequest, workingDirectory))
            .ToList();
    }

    private static GithubAnnotationRequest MapToGithubAnnotationRequest(Result result, GithubSarifCollectorRequest collectorRequest, string workingDirectory)
    {
        var physicalLocation = result.Locations.First().PhysicalLocation;
        var relativePath = ToRelativePath(physicalLocation, workingDirectory);

        return new GithubAnnotationRequest
        {
            Path = relativePath,
            StartLine = physicalLocation.Region.StartLine,
            StartColumn = physicalLocation.Region.StartColumn,
            EndLine = physicalLocation.Region.EndLine,
            EndColumn = physicalLocation.Region.EndColumn,
            SarifLevel = result.Level,
            Message = result.Message.Text,
            RawDetails = ToGithubFileUri(relativePath, physicalLocation.Region.StartLine, collectorRequest, workingDirectory).ToString()
        };
    }

    private static string ToRelativePath(PhysicalLocation physicalLocation, string workingDirectory)
    {
        return Path.GetRelativePath(workingDirectory, physicalLocation.ArtifactLocation.Uri.LocalPath);
    }

    private static Uri ToGithubFileUri(string relativePath, int startLine, GithubSarifCollectorRequest collectorRequest, string workingDirectory)
    {
        return new Uri($"{collectorRequest.GithubServerUrl}/{collectorRequest.GithubRepo}/blob/{collectorRequest.GithubRefName}/{relativePath.Replace("\\", "/")}#L{startLine}");
    }

    internal static string CreateSummaryMarkdown(IList<Result> sarifResults, GithubSarifCollectorRequest collectorRequest, string workingDirectory)
    {
        var result = new StringBuilder();
        result.AppendLine("## Build Results");

        foreach (var sarifResult in sarifResults)
        {
            var physicalLocation = sarifResult.Locations.First().PhysicalLocation;
            var relativePath = ToRelativePath(physicalLocation, workingDirectory);
            var fileUri = ToGithubFileUri(relativePath, physicalLocation.Region.StartLine, collectorRequest, workingDirectory);
            var fileUriText = $"{fileUri.Segments.LastOrDefault()}{fileUri.Fragment}";
            var symbol = sarifResult.Level switch
            {
                FailureLevel.Error => ":x:",
                FailureLevel.Warning => ":warning:",
                _ => "🛈",
            };

            result.AppendLine(
                $"""
                {symbol} [{fileUriText}]({fileUri}) 
                {sarifResult.Message.Text}  

                """);
        }

        return result.ToString();
    }

    internal static GithubChecksApiOutput MapToOutput(FailureLevel logLevel)
    {
        return logLevel switch
        {
            FailureLevel.Warning => new GithubChecksApiOutput { Summary = "There are build warnings", TextDescription = "" },
            FailureLevel.Error => new GithubChecksApiOutput { Summary = "There are build errors", TextDescription = "" },
            _ => new GithubChecksApiOutput { Summary = "Everything is fine :)", TextDescription = "" }
        };
    }

    internal static string MapToConclusion(FailureLevel logLevel)
    {
        return logLevel switch
        {
            FailureLevel.Warning => "failure",
            FailureLevel.Error => "failure",
            _ => "success"
        };
    }

    internal static FailureLevel GetMaxLevel(IEnumerable<GithubAnnotationRequest> annotationRequests)
    {
        return annotationRequests
            .Select(req => (FailureLevel?)req.SarifLevel)
            .Max() ?? FailureLevel.None;
    }
}
