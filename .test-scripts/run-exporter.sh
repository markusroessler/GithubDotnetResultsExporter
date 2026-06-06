#!/bin/zsh

export GITHUB_OUTPUT="github-output.txt"
export GITHUB_STEP_SUMMARY="github-step-summary.md"  
dotnet run --project GithubDotnetResultsExporter.Csl -f net10.0 -- --github-server-url https://github.com --github-repo markusroessler/GithubDotnetResultsExporter --github-ref-name main --export-step-summary true