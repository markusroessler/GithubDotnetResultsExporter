using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Sarif;

namespace GithubSarifCollector.Model;

internal static class GithubSarifCollectorModelOps
{
    internal static IList<GithubAnnotationRequest> MapToAnnotationRequests(IEnumerable<SarifLog> sarifLogs)
    {
        return sarifLogs
            .SelectMany(log => log.Results().Select(result => result))
            .Select(MapToGithubAnnotationRequest)
            .ToList();
    }

    private static GithubAnnotationRequest MapToGithubAnnotationRequest(Result result)
    {
        var physicalLocation = result.Locations.First().PhysicalLocation;
        return new GithubAnnotationRequest
        {
            Path = Path.GetRelativePath(Environment.CurrentDirectory, physicalLocation.ArtifactLocation.Uri.LocalPath),
            StartLine = physicalLocation.Region.StartLine,
            StartColumn = physicalLocation.Region.StartColumn,
            EndLine = physicalLocation.Region.EndLine,
            EndColumn = physicalLocation.Region.EndColumn,
            SarifLevel = result.Level,
            Message = result.Message.Text,
            RawDetails = "https://github.com/markusroessler/BackupDotnetConsole/blob/feature/9-implement-github-build/Backup.Model/BackupModel.cs#L26"
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
