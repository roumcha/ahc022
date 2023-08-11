#!/usr/bin/env bash
set -xe
export AWS_LAMBDA=1
export DOTNET_EnableWriteXorExecute=0
export DOTNET_CLI_TELEMETRY_OPTOUT=1

time ./tester ./main <in.txt >out.txt || exit 0
./vis in.txt out.txt || exit 0
