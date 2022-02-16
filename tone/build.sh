#!/bin/sh

dotnet publish -r linux-x64 --self-contained -c Release -o ./dist tone.csproj
