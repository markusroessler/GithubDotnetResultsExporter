#!/bin/bash

printAllSourceCodeFiles() {
    allSourceCodeFiles=$(git ls-files | grep -E '\.cs$|\.js$|\.razor$')
    echo "$allSourceCodeFiles"
}

printCount() {
    xargs grep -v '^$' | wc -l
}

printAppCount() {
    allSourceCodeFiles=$1
    libCount=$(echo "$allSourceCodeFiles" | grep -E -v '.*Test/.*|Sample.*/' | printCount)
    echo $libCount
}

printTestCount() {
    allSourceCodeFiles=$1
    testCount=$(echo "$allSourceCodeFiles" | grep -E '.*Test/.*' | printCount)
    echo $testCount
}