using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml;
using GithubDotnetResultsExporter.Model.Vstst;
using Microsoft.CodeAnalysis.Sarif;

namespace GithubDotnetResultsExporter.Model;

internal static class GithubDotnetResultsExporterModelOps
{

    private const string InfoSymbol = "🛈";
    private const string ErrorSymbol = ":x:";

    internal static GithubDotnetResultsExporterRequest ParseArgs(string[] args)
    {
        var exportChecksActionParams = false;
        var exportStepSummary = false;
        string? githubServerUrl = null;
        string? githubRepo = null;
        string? githubRefName = null;
        var cultureInfo = CultureInfo.CurrentCulture;

        for (int i = 0; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "--export-checks-action-params":
                    exportChecksActionParams = Convert.ToBoolean(args.ElementAtOrDefault(++i), CultureInfo.InvariantCulture);
                    break;
                case "--export-step-summary":
                    exportStepSummary = Convert.ToBoolean(args.ElementAtOrDefault(++i), CultureInfo.InvariantCulture);
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
                case "--culture":
                    var cultureInfoStr = args.ElementAtOrDefault(++i);
                    if (cultureInfoStr != null)
                        cultureInfo = new CultureInfo(cultureInfoStr);
                    break;
                default:
                    throw new ArgumentException($"unknown arg {args[i]}");
            }
        }

        if (githubServerUrl == null)
            throw new ArgumentException("missing argument: --github-server-url");

        if (githubRepo == null)
            throw new ArgumentException("missing argument: --github-repo");

        if (githubRefName == null)
            throw new ArgumentException("missing argument: --github-ref-name");

        return new(exportChecksActionParams, exportStepSummary, githubServerUrl, githubRepo, githubRefName, cultureInfo);
    }

    internal static IList<Result> GetSarifResults(IEnumerable<SarifLog> sarifLogs)
    {
        return sarifLogs
            .SelectMany(log => log.Results())
            .Where(result => (result.Suppressions?.All(s => s.Kind == SuppressionKind.None)).GetValueOrDefault(true))
            .OrderByDescending(result => result.Level)
            .ToList();
    }

    internal static IList<GithubAnnotationRequest> MapToAnnotationRequests(IList<Result> sarifResults, GithubDotnetResultsExporterRequest collectorRequest, string workingDirectory)
    {
        return sarifResults
            .Select(request => MapToGithubAnnotationRequest(request, collectorRequest, workingDirectory))
            .ToList();
    }

    private static GithubAnnotationRequest MapToGithubAnnotationRequest(Result result, GithubDotnetResultsExporterRequest collectorRequest, string workingDirectory)
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
            RawDetails = ToGithubFileUri(relativePath, physicalLocation.Region.StartLine, collectorRequest).ToString()
        };
    }

    private static string ToRelativePath(PhysicalLocation physicalLocation, string workingDirectory)
    {
        return Path.GetRelativePath(workingDirectory, physicalLocation.ArtifactLocation.Uri.LocalPath);
    }

    private static Uri ToGithubFileUri(string relativePath, int startLine, GithubDotnetResultsExporterRequest collectorRequest)
    {
        return new Uri($"{collectorRequest.GithubServerUrl}/{collectorRequest.GithubRepo}/blob/{collectorRequest.GithubRefName}/{relativePath.Replace("\\", "/")}#L{startLine}");
    }

    internal static string CreateSummaryMarkdown(IList<Result> sarifResults, GithubDotnetResultsExporterRequest collectorRequest, string workingDirectory)
    {
        var errorCount = 0;
        var warningCount = 0;
        var noteCount = 0;
        foreach (var sarifResult in sarifResults)
        {
            switch (sarifResult.Level)
            {
                case FailureLevel.Error:
                    errorCount++;
                    break;

                case FailureLevel.Warning:
                    warningCount++;
                    break;

                default:
                    noteCount++;
                    break;
            }
        }

        var result = new StringBuilder();

        var headerSymbol = errorCount > 0 ? $" {ErrorSymbol}" : "";
        result.AppendLine(collectorRequest.CultureInfo, $"##{headerSymbol} Build Results");

        result.AppendLine(collectorRequest.CultureInfo,
            $"""
            |||
            |:---|---:|
            | Errors | {errorCount:N0} |
            | Warnings | {warningCount:N0} |
            | Notes | {noteCount:N0} |

            """);

        var noteSarifResults = sarifResults.Where(x => x.Level == FailureLevel.Note).ToList();
        var severeSarifResults = sarifResults.Where(x => x.Level != FailureLevel.Note).ToList();

        AppendSarifResults(severeSarifResults, result, collectorRequest, workingDirectory);

        if (noteSarifResults.Count > 0)
        {
            result.AppendLine($"<details><summary>{InfoSymbol} Notes</summary>\n");
            AppendSarifResults(noteSarifResults, result, collectorRequest, workingDirectory);
            result.AppendLine("</details>\n");
        }

        return result.ToString();
    }


    private static void AppendSarifResults(List<Result> sarifResults, StringBuilder result,
         GithubDotnetResultsExporterRequest collectorRequest, string workingDirectory)
    {
        foreach (var sarifResult in sarifResults)
        {
            var symbol = sarifResult.Level switch
            {
                FailureLevel.Error => ErrorSymbol,
                FailureLevel.Warning => ":warning:",
                _ => InfoSymbol,
            };

            var ruleHyperlink = "";
            var ruleId = sarifResult.RuleId;
            if (ruleId != null)
                ruleHyperlink = $" ([{ruleId}](https://www.google.com/search?q={ruleId}))";

            var ruleMessageLine = $"{sarifResult.Message.Text}{ruleHyperlink}";

            var physicalLocation = sarifResult.Locations?.FirstOrDefault()?.PhysicalLocation;
            if (physicalLocation != null)
            {
                var relativePath = ToRelativePath(physicalLocation, workingDirectory);
                var fileUri = ToGithubFileUri(relativePath, physicalLocation.Region.StartLine, collectorRequest);
                var fileUriText = $"{fileUri.Segments.LastOrDefault()}{fileUri.Fragment}";

                result.AppendLine(collectorRequest.CultureInfo,
                    $"""
                    {symbol} [{fileUriText}]({fileUri})  
                    {ruleMessageLine}  

                    """);
            }
            else
            {
                result.AppendLine(collectorRequest.CultureInfo,
                    $"""
                    {symbol} {ruleMessageLine}  

                    """);
            }
        }
    }

    private readonly record struct TestDefAndResult(UnitTestType TestDef, TestResultType TestResult);

    internal static string CreateSummaryMarkdown(IEnumerable<TestRunType> testRuns, CultureInfo cultureInfo)
    {
        var result = new StringBuilder();
        result.AppendLine("## Test Results");

        var testRunsList = testRuns.ToList();

        var counters = testRunsList
            .Select(testRun => testRun.Items.OfType<TestRunTypeResultSummary>().First())
            .Select(summary => summary.Items.OfType<CountersType>().First())
            .ToList();

        var successCount = 0;
        var failCount = 0;
        var skipCount = 0;

        // note: some counters like notExecuted are not populated
        foreach (var counter in counters)
        {
            successCount += counter.passed;
            failCount += counter.executed - counter.passed;
            skipCount += counter.total - counter.executed;
        }

        result.AppendLine(cultureInfo,
            $"""
            |||
            |:---|---:|
            | Failed | {failCount:N0} |
            | Skipped | {skipCount:N0} |
            | Passed | {successCount:N0} |
            """);

        var unitTestsPerId = testRunsList
            .SelectMany(testRun => testRun.Items.OfType<TestDefinitionType>()) // cardinality 0-1
            .SelectMany(testDef => testDef.Items.OfType<UnitTestType>())
            .ToImmutableDictionary(test => test.id);

        var testResults = testRuns
            .SelectMany(testRun => testRun.Items.OfType<ResultsType>()) // cardinality 0-1
            .SelectMany(testRun => testRun.Items.OfType<UnitTestResultType>())
            .Select(testResult => new TestDefAndResult(unitTestsPerId[testResult.testId], testResult))
            .Order(Comparer<TestDefAndResult>.Create(CompareUnitTestResults))
            .ToList();

        var hasTestResults = testResults.Count > 0;
        if (hasTestResults)
            result.AppendLine("<details><summary><b>Details</b></summary>");

        foreach (var (testDef, testResult) in testResults)
        {
            var symbol = testResult.outcome switch
            {
                "Passed" => ":heavy_check_mark:",
                "NotExecuted" => ":zzz:",
                _ => ErrorSymbol,
            };

            var output = testResult.Items?.OfType<OutputType>().FirstOrDefault();
            var errorText = "";
            var stdOutText = "";
            var stdErrText = "";
            if (output != null)
            {
                var errorInfo = output.ErrorInfo;
                if (errorInfo != null)
                {
                    errorText = $"""
                        **Error**  
                        ```
                        {(errorInfo.Message as XmlNode[])?.FirstOrDefault()?.Value}
                        {(errorInfo.StackTrace as XmlNode[])?.FirstOrDefault()?.Value}
                        ```

                        """;
                }
                var stdOut = output.StdOut as XmlNode[];
                if (stdOut != null)
                {
                    stdOutText = $"""
                        **StdOut**  
                        ```
                        {stdOut.First().Value}
                        ```

                        """;
                }
                var stdErr = output.StdErr as XmlNode[];
                if (stdErr != null)
                {
                    stdErrText = $"""
                        **StdErr**  
                        ```
                        {stdErr.First().Value}
                        ```

                        """;
                }
            }

            result.AppendLine(cultureInfo, $"""
                <details><summary>{symbol} {testDef.TestMethod.className}.{testDef.TestMethod.name}</summary>

                {errorText}{stdOutText}{stdErrText}
                </details>
                """);
        }

        if (hasTestResults)
            result.AppendLine("</details>");

        return result.ToString();
    }

    private static readonly List<string> TestOutcomeOrder = new()
    {
        "Failed",
        "NotExecuted",
        "Passed",
    };

    private static int CompareUnitTestResults(TestDefAndResult result1, TestDefAndResult result2)
    {
        var compare = TestOutcomeOrder.IndexOf(result1.TestResult.outcome) - TestOutcomeOrder.IndexOf(result2.TestResult.outcome);
        if (compare != 0)
            return compare;

        var testMethod1 = result1.TestDef.TestMethod;
        var testMethod2 = result2.TestDef.TestMethod;

        compare = string.CompareOrdinal(testMethod1.className, testMethod2.className);
        if (compare != 0)
            return compare;

        return string.CompareOrdinal(testMethod1.name, testMethod2.name);
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
