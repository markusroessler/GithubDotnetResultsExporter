#!/bin/bash
scriptDir=$(dirname "$0")
source "$scriptDir/lines-of-codes-lib.sh"

allSourceCodeFiles=$(printAllSourceCodeFiles)
appCount=$(printAppCount "$allSourceCodeFiles")
testCount=$(printTestCount "$allSourceCodeFiles")
totalCount=$(echo "$allSourceCodeFiles" | xargs grep -v '^$' | wc -l)

echo "## Lines of Code"
echo "| Type | Count | "
echo "| :--- | :--- | "
echo "| App | $appCount | "
echo "| Tests | $testCount | "
echo "| **Total** | **$totalCount** | "