#!/usr/bin/env bash
set -euo pipefail

export DOTNET_ROOT="$HOME/.dotnet"
export PATH="$DOTNET_ROOT:$PATH"

if ! command -v dotnet >/dev/null 2>&1; then
  curl -fsSL https://dot.net/v1/dotnet-install.sh -o dotnet-install.sh
  bash dotnet-install.sh --version 8.0.100
fi

cd src/CalculadoraCostes.Web
dotnet publish -c Release -o dist
