#!/bin/bash
dotnet test -c:$1 --no-build --logger:html --logger:"trx;LogFileName=TestResults/TestResults.trx" --collect:"XPlat Code Coverage" --settings $2 $3