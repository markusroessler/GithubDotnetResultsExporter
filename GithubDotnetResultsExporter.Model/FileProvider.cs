using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GithubSarifCollector.Model;

internal sealed class FileProvider
{
    public string WorkingDirectory => Environment.CurrentDirectory;

    public string GithubOutputFile => Environment.GetEnvironmentVariable("GITHUB_OUTPUT") ?? throw new Exception("GITHUB_OUTPUT not defined");

    public string GithubStepSummaryFile => Environment.GetEnvironmentVariable("GITHUB_STEP_SUMMARY") ?? throw new Exception("GITHUB_STEP_SUMMARY not defined");

    public IEnumerable<string> EnumerateSarifFiles(string repoDir)
    {
        return Directory.EnumerateFiles(repoDir, "compiler-diagnostics.sarif", new EnumerationOptions { RecurseSubdirectories = true, MaxRecursionDepth = 1 });
    }

    public void AppendTextToFile(string file, string contents) => File.AppendAllText(file, contents);
}
