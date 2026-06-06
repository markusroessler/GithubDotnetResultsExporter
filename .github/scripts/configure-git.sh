#!/bin/bash

# prevent git error "detected dubious ownership in repository at '/__w/TimetrackingApp/TimetrackingApp'"
git config --global --add safe.directory $GITHUB_WORKSPACE

# see https://api.github.com/users/github-actions%5Bbot%5D
git config --global user.name "github-actions[bot]"
git config --global user.email "41898282+github-actions[bot]@users.noreply.github.com"
