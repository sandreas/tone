# tone
Audio converter and  tagger

# todo
- Libs
  - OperationResult (https://github.com/gnaeus/OperationResult)
  - System.IO.Abstractions (https://github.com/TestableIO/System.IO.Abstractions)
  - ATL (https://github.com/Zeugma440/atldotnet)
  - CommandLine (https://github.com/commandlineparser/commandline)
- 
- Deployment
  - Raspberry pi
  - Docker image
- Write articles
  - Github actions
  - Dependency injection for command line
# notes

## Dependency injection
see https://www.youtube.com/watch?v=dgJ1nS2CLpQ
- Install `Microsoft.Extensions.Hosting`


## general
- `dotnet publish tone/tone.csproj --framework net5.0 --runtime linux-x64 -c Release -o "tone/dist/"`
- Runtime identifer catalog: https://docs.microsoft.com/en-us/dotnet/core/rid-catalog
- Project settings

```xml
  <OutputType>Exe</OutputType>
  <TargetFramework>net5.0</TargetFramework>
  <DockerDefaultTargetOS>Linux</DockerDefaultTargetOS>
  <PublishSingleFile>true</PublishSingleFile>
  <SelfContained>true</SelfContained>
  <PublishReadyToRun>true</PublishReadyToRun>
  <PublishTrimmed>true</PublishTrimmed>
```




# deployment
```bash
#linux (linux-x64)
dotnet publish tone/tone.csproj --framework net6.0 --runtime linux-x64 -o "tone/dist/linux-x64/" -p:PublishSingleFile=true --self-contained true -p:PublishReadyToRun=true -p:PublishReadyToRunShowWarnings=true -p:PublishTrimmed=true -c Release

# macOS (osx-x64)
# dotnet publish tone/tone.csproj --runtime osx-x64 -o "tone/dist/osx-x64/" -p:PublishSingleFile=true --self-contained true -p:PublishReadyToRun=true -p:PublishTrimmed=true --framework net5.0 -c Release
dotnet publish tone/tone.csproj --runtime osx-x64 -o "tone/dist/osx-x64/" -p:PublishSingleFile=true --self-contained true -p:PublishTrimmed=true --framework net5.0 -c Release


# win
dotnet publish tone/tone.csproj --runtime win-x64 -o "tone/dist/win-x64/" -p:PublishSingleFile=true --self-contained true -p:PublishReadyToRun=true -p:PublishTrimmed=true --framework net5.0 -c Release

```