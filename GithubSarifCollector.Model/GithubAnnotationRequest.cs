using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Sarif;

namespace GithubSarifCollector.Model;

/// <summary>
/// https://docs.github.com/en/rest/checks/runs?apiVersion=2022-11-28#annotations-object
/// </summary>
internal sealed class GithubAnnotationRequest
{
    [JsonPropertyName("path")]
    public string Path { get; set; } = default!;

    [JsonPropertyName("start_line")]
    public int StartLine { get; set; }

    [JsonPropertyName("end_line")]
    public int EndLine { get; set; }

    [JsonPropertyName("start_column")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? StartColumn { get; set; }

    [JsonPropertyName("end_column")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? EndColumn { get; set; }

    [JsonIgnore]
    public FailureLevel SarifLevel { get; set; } = FailureLevel.None;

    /// <summary>
    /// Can be one of: notice, warning, failure
    /// </summary>
    [JsonPropertyName("annotation_level")]
    public string AnnotationLevel => MapToAnnotationLevel(SarifLevel);

    [JsonPropertyName("message")]
    public string Message { get; set; } = default!;

    [JsonPropertyName("title")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Title { get; set; }

    [JsonPropertyName("raw_details")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? RawDetails { get; set; }

    private static string MapToAnnotationLevel(FailureLevel level)
    {
        return level switch
        {
            FailureLevel.Warning => "warning",
            FailureLevel.Error => "failure",
            _ => "notice"
        };
    }
}
