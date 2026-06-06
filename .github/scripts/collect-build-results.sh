#!/bin/bash

if [[ $1 == "use_dotnet_run" ]]; then 
    dotnet run --project GithubDotnetResultsExporter.Csl --framework net10.0 -- \
        --export-step-summary true --github-server-url $2 --github-repo $3 --github-ref-name $4 --step-summary-content-types $5 --culture "de-DE"

elif [[ $1 == "use_installed" ]]; then
    export-dotnet-results-for-github \
        --export-step-summary true --github-server-url $2 --github-repo $3 --github-ref-name $4 --step-summary-content-types $5 --culture "de-DE"

else
    echo "invalid arg: $1"
    exit 1
fi