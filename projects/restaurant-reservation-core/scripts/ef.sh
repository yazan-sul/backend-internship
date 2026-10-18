#!/usr/bin/env bash
set -euo pipefail

cd -- "$(dirname -- "$0")/.."

# This Mac's .NET 8 runtime is x64; use its working ARM64 .NET 10 runtime.
DOTNET_ROLL_FORWARD=LatestMajor dotnet ef "$@"
