# tone
Audio player and tagger

## notes
- `dotnet publish tone/tone.csproj --framework net5.0 --runtime linux-x64 -c Release -o "tone/dist/"`
- Runtime identifer catalog: https://docs.microsoft.com/en-us/dotnet/core/rid-catalog
- Project settings ```xml
        <OutputType>Exe</OutputType>
        <TargetFramework>net5.0</TargetFramework>
        <DockerDefaultTargetOS>Linux</DockerDefaultTargetOS>
        <PublishSingleFile>true</PublishSingleFile>
        <SelfContained>true</SelfContained>
        <PublishReadyToRun>true</PublishReadyToRun>
        <PublishTrimmed>true</PublishTrimmed>
```

```
#linux (linux-x64)
dotnet publish tone/tone.csproj --framework net6.0 --runtime linux-x64 -o "tone/dist/linux-x64/" -p:PublishSingleFile=true --self-contained true -p:PublishReadyToRun=true -p:PublishReadyToRunShowWarnings=true -p:PublishTrimmed=true -c Release

# macOS (osx-x64)
# dotnet publish tone/tone.csproj --runtime osx-x64 -o "tone/dist/osx-x64/" -p:PublishSingleFile=true --self-contained true -p:PublishReadyToRun=true -p:PublishTrimmed=true --framework net5.0 -c Release
dotnet publish tone/tone.csproj --runtime osx-x64 -o "tone/dist/osx-x64/" -p:PublishSingleFile=true --self-contained true -p:PublishTrimmed=true --framework net5.0 -c Release


# win
dotnet publish tone/tone.csproj --runtime win-x64 -o "tone/dist/win-x64/" -p:PublishSingleFile=true --self-contained true -p:PublishReadyToRun=true -p:PublishTrimmed=true --framework net5.0 -c Release

```