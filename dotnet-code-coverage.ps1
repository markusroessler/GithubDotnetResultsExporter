# Executes unittests and generates a coverage report for a solution
# prerequisites: dotnet tool install/update --global dotnet-reportgenerator-globaltool
$ErrorActionPreference="Stop"

if (Test-Path TestResults) 
{
    Remove-Item -Path TestResults -Recurse
}
Remove-Item -Path **/TestResults -Recurse

dotnet test --logger:html --logger:"trx;LogFileName=TestResults/TestResults.trx" --collect:"XPlat Code Coverage" --settings coverlet.runsettings
if ( -not $? ) { exit }

reportgenerator.exe
if ( -not $? ) { exit }

Invoke-Item TestResults\coverage-report\index.html