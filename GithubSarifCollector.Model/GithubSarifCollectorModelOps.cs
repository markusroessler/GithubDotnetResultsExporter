using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Sarif;

namespace GithubSarifCollector.Model;

internal static class GithubSarifCollectorModelOps
{
    internal static GithubSarifCollectorRequest ParseArgs(string[] args)
    {
        string? githubServerUrl = null;
        string? githubRepo = null;
        string? githubRefName = null;

        for (int i = 0; i < args.Length; i++)
        {
            switch (args[i])
            {
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

        return new(githubServerUrl, githubRepo, githubRefName);
    }

    internal static IList<GithubAnnotationRequest> MapToAnnotationRequests(IEnumerable<SarifLog> sarifLogs, GithubSarifCollectorRequest collectorRequest)
    {
        return sarifLogs
            .SelectMany(log => log.Results().Select(result => result))
            .Select(request => MapToGithubAnnotationRequest(request, collectorRequest))
            .ToList();
    }

    private static GithubAnnotationRequest MapToGithubAnnotationRequest(Result result, GithubSarifCollectorRequest collectorRequest)
    {
        var physicalLocation = result.Locations.First().PhysicalLocation;
        var path = Path.GetRelativePath(Environment.CurrentDirectory, physicalLocation.ArtifactLocation.Uri.LocalPath);
        return new GithubAnnotationRequest
        {
            Path = path,
            StartLine = physicalLocation.Region.StartLine,
            StartColumn = physicalLocation.Region.StartColumn,
            EndLine = physicalLocation.Region.EndLine,
            EndColumn = physicalLocation.Region.EndColumn,
            SarifLevel = result.Level,
            Message = result.Message.Text,
            RawDetails = $"{collectorRequest.GithubServerUrl}/{collectorRequest.GithubRepo}/blob/{collectorRequest.GithubRefName}/{path.Replace("\\", "/")}#L{physicalLocation.Region.StartLine}"
        };
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
