# Howto test local
```bash
dotnet pack GithubSarifCollector.Csl -p:Version=1.0.0-local.1
dotnet new tool-manifest
dotnet tool install --add-source ./GithubSarifCollector.Csl/bin/Debug GithubSarifCollector --version 1.0.0-local.1
$env:GITHUB_OUTPUT="D:\Entwicklung\DotNet\GithubSarifCollector\githuboutput.txt"
dotnet collect-sarifs-for-github --github-server-url https://github.com --github-repo markusroessler/GithubSarifCollector --github-ref-name develop
```
OR
```bash
$env:GITHUB_OUTPUT="D:\Entwicklung\DotNet\GithubSarifCollector\github-output.txt"
$env:GITHUB_STEP_SUMMARY="D:\Entwicklung\DotNet\GithubSarifCollector\github-step-summary.md"  
dotnet run --project GithubSarifCollector.Csl -- --github-server-url https://github.com --github-repo markusroessler/GithubSarifCollector --github-ref-name develop
```
