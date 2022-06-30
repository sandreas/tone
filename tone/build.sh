#!/bin/sh

OBJ_DIR="$(dirname $0)/obj"
R2R_DIR="$(find "$OBJ_DIR" -type d -name 'R2R')" #-exec rm -r {} \;

if [ -d "$R2R_DIR" ]; then
  # fix for https://github.com/dotnet/sdk/issues/21072
  echo "fixing issue by deleting R2R: $R2R_DIR"
  rm -r "$R2R_DIR"
fi 

dotnet publish -r linux-x64 --self-contained -c Release -o ./dist tone.csproj