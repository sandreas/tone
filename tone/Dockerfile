FROM mcr.microsoft.com/dotnet/runtime:5.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["tone/tone.csproj", "tone/"]
RUN dotnet restore "tone/tone.csproj"
COPY . .
WORKDIR "/src/tone"
RUN dotnet build "tone.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "tone.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "tone.dll"]
