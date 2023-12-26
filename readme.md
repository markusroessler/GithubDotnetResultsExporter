# Params
## --export-checks-action-params
true to export the following variables for use with https://github.com/LouisBrunner/checks-action  

```bash
checks-action-conclusion
checks-action-output
checks-action-annotations
```
default: false

## --export-step-summary
true to export a step summary for build and test results
default: false

## --github-server-url (required)
used to construct repo file urls  
example:
```bash
${{ github.server_url }}
```
default: ''

## --github-repo (required)
used to construct repo file urls  
example:
```bash
${{ github.repository }}
```
default: ''

## --github-ref-name (required)
used to construct repo file urls  
example:
```bash
${{ github.ref_name }}
```
default: ''

## --culture
used to format the output  
example: 'de-DE'  
default: CultureInfo.CurrentCulture

# Howto test local
```bash
dotnet pack GithubDotnetResultsExporter.Csl -p:Version=1.0.0-local.1
dotnet new tool-manifest
dotnet tool install --add-source ./GithubDotnetResultsExporter.Csl/bin/Debug GithubDotnetResultsExporter --version 1.0.0-local.1
$env:GITHUB_OUTPUT="D:\Entwicklung\DotNet\GithubDotnetResultsExporter\githuboutput.txt"
dotnet export-dotnet-results-for-github --github-server-url https://github.com --github-repo markusroessler/GithubDotnetResultsExporter --github-ref-name develop
```
OR
```bash
$env:GITHUB_OUTPUT="D:\Entwicklung\DotNet\GithubDotnetResultsExporter\github-output.txt"
$env:GITHUB_STEP_SUMMARY="D:\Entwicklung\DotNet\GithubDotnetResultsExporter\github-step-summary.md"  
dotnet run --project GithubDotnetResultsExporter.Csl -- --github-server-url https://github.com --github-repo markusroessler/GithubDotnetResultsExporter --github-ref-name develop --export-step-summary true
```

# Howto generate classes from trx xsd
```bash
Set-Alias xsd "C:\Program Files (x86)\Microsoft SDKs\Windows\v10.0A\bin\NETFX 4.8 Tools\x64\xsd.exe"   
xsd /c "C:\Program Files\Microsoft Visual Studio\2022\Community\Xml\Schemas\vstst.xsd"
```