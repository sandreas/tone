#!/bin/sh

OBJ_DIR="${PWD}/obj"

# fix for https://github.com/dotnet/sdk/issues/21072
find "$OBJ_DIR" -type d -name 'R2R' -exec rm -r "{}" \;

dotnet publish -r linux-x64 --self-contained -c Release -o ./dist tone.csproj