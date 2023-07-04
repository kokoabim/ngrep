#!/bin/bash
set -e

dotnet publish src/ngrep/ngrep.csproj -c Release -r "${1:-osx-arm64}" --self-contained true -p:PublishSingleFile=true -p:PublishTrimmed=true -o publish
cp publish/ngrep /usr/local/bin/ngrep
