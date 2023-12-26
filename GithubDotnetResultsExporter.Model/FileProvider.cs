using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GithubDotnetResultsExporter.Model;

internal sealed class FileProvider
{
    private readonly IEnvironment _environment;

    public string WorkingDirectory => _environment.CurrentDirectory;

    public string GithubOutputFile => _environment.GetEnvironmentVariable("GITHUB_OUTPUT") ?? throw new InvalidOperationException("GITHUB_OUTPUT not defined");

    public string GithubStepSummaryFile => _environment.GetEnvironmentVariable("GITHUB_STEP_SUMMARY") ?? throw new InvalidOperationException("GITHUB_STEP_SUMMARY not defined");


    public FileProvider(IEnvironment environment)
    {
        _environment = environment;
    }

    public IEnumerable<string> EnumerateSarifFiles(string repoDir)
    {
        return Directory.EnumerateFiles(repoDir, "compiler-diagnostics.sarif", new EnumerationOptions { RecurseSubdirectories = true, MaxRecursionDepth = 1 });
    }

    public IEnumerable<string> EnumerateTrxFiles(string repoDir)
    {
        return Directory.EnumerateFiles(repoDir, "TestResults.trx", new EnumerationOptions { RecurseSubdirectories = true, MaxRecursionDepth = 3 });
    }

    public void AppendTextToFile(string file, string contents) => File.AppendAllText(file, contents);

}
