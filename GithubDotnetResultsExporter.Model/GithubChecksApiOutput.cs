using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace GithubDotnetResultsExporter.Model;

internal sealed class GithubChecksApiOutput
{
    [JsonPropertyName("title")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Title { get; set; }

    [JsonPropertyName("summary")]
    public string Summary { get; set; } = default!;

    [JsonPropertyName("text_description")]
    public string TextDescription { get; set; } = default!;
}
