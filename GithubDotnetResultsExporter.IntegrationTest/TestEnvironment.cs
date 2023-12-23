using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GithubDotnetResultsExporter.Model;

namespace GithubDotnetResultsExporter.IntegrationTest;

public sealed class TestEnvironment : IEnvironment
{
    public string CurrentDirectory { get; set; }

    public string GithubStepSummaryFile => Path.Combine(CurrentDirectory, "github-step-summary.md");

    public string GetEnvironmentVariable(string variable)
    {
        return variable switch
        {
            "GITHUB_OUTPUT" => Path.Combine(CurrentDirectory, "github-output.txt"),
            "GITHUB_STEP_SUMMARY" => GithubStepSummaryFile,
            _ => null
        };
    }
}
