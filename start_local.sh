#!/usr/bin/env bash
set -xe
export DOTNET_CLI_TELEMETRY_OPTOUT=1

# time ./main <in.txt >out.txt 2>err.txt
# time ./tester ./main <in.txt >out.txt
# ./vis in.txt out.txt

./main

cat <<EOS
Score	1234567890 pt
Rate:50.24%
[WA]

real    0m1.002s
EOS
