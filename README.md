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