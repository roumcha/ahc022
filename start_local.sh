#!/usr/bin/env bash
set -xe
export DOTNET_CLI_TELEMETRY_OPTOUT=1

time ./tester ./main <in.txt >out.txt || exit 0
./vis in.txt out.txt || exit 0
