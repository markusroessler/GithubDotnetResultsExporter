# Howto test local
```bash
dotnet pack GithubDotnetResultsExporter.Csl -p:Version=1.0.0-local.1
dotnet new tool-manifest
dotnet tool install --add-source ./GithubDotnetResultsExporter.Csl/bin/Debug GithubDotnetResultsExporter --version 1.0.0-local.1
$env:GITHUB_OUTPUT="D:\Entwicklung\DotNet\GithubDotnetResultsExporter\githuboutput.txt"
dotnet collect-sarifs-for-github --github-server-url https://github.com --github-repo markusroessler/GithubDotnetResultsExporter --github-ref-name develop
```
OR
```bash
$env:GITHUB_OUTPUT="D:\Entwicklung\DotNet\GithubDotnetResultsExporter\github-output.txt"
$env:GITHUB_STEP_SUMMARY="D:\Entwicklung\DotNet\GithubDotnetResultsExporter\github-step-summary.md"  
dotnet run --project GithubDotnetResultsExporter.Csl -- --github-server-url https://github.com --github-repo markusroessler/GithubDotnetResultsExporter --github-ref-name develop
```
