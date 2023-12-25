using System;
using System.Collections.Generic;
using System.Collections.Immutable;
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
    internal static GithubDotnetResultsExporterRequest ParseArgs(string[] args)
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
        var result = new StringBuilder();
        result.AppendLine("## Build Results");

        foreach (var sarifResult in sarifResults)
        {
            var physicalLocation = sarifResult.Locations.First().PhysicalLocation;
            var relativePath = ToRelativePath(physicalLocation, workingDirectory);
            var fileUri = ToGithubFileUri(relativePath, physicalLocation.Region.StartLine, collectorRequest);
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
                ```{sarifResult.Message.Text}```  

                """);
        }

        return result.ToString();
    }

    private readonly record struct TestDefAndResult(UnitTestType TestDef, TestResultType TestResult);

    internal static string CreateSummaryMarkdown(IEnumerable<TestRunType> testRuns)
    {
        var result = new StringBuilder();
        result.AppendLine("## Test Results");

        var counters = testRuns
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

        result.AppendLine(
            $"""
            failed: {failCount}  
            skipped: {skipCount}  
            passed: {successCount}

            """);

        var unitTestsPerId = testRuns
            .Select(testRun => testRun.Items.OfType<TestDefinitionType>().First())
            .SelectMany(testDef => testDef.Items.OfType<UnitTestType>())
            .ToImmutableDictionary(test => test.id);

        var testResults = testRuns
            .Select(testRun => testRun.Items.OfType<ResultsType>().First())
            .SelectMany(testRun => testRun.Items.OfType<UnitTestResultType>())
            .Select(testResult => new TestDefAndResult(unitTestsPerId[testResult.testId], testResult))
            .Order(Comparer<TestDefAndResult>.Create(CompareUnitTestResults))
            .ToList();

        foreach (var (testDef, testResult) in testResults)
        {
            var symbol = testResult.outcome switch
            {
                "Passed" => ":heavy_check_mark:",
                "NotExecuted" => ":zzz:",
                _ => ":x:",
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

            result.AppendLine($"""
                <details><summary>{symbol} {testDef.TestMethod.className}.{testDef.TestMethod.name}</summary>

                {errorText}{stdOutText}{stdErrText}
                </details>
                """);
        }

        return result.ToString();
    }

    private static List<string> TestOutcomeOrder = new()
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

        compare = testMethod1.className.CompareTo(testMethod2.className);
        if (compare != 0)
            return compare;

        return testMethod1.name.CompareTo(testMethod2.name);
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
