ARG DOTNET_VERSION=10.0
ARG BUILD_CONFIGURATION=Release

FROM mcr.microsoft.com/dotnet/sdk:${DOTNET_VERSION} AS restore
WORKDIR /src

ENV DOTNET_CLI_TELEMETRY_OPTOUT=1 \
    DOTNET_NOLOGO=1 \
    DOTNET_SKIP_FIRST_TIME_EXPERIENCE=1

COPY QuizForge.csproj ./
RUN dotnet restore "QuizForge.csproj"

FROM restore AS publish
ARG BUILD_CONFIGURATION=Release

COPY . ./
RUN dotnet publish "QuizForge.csproj" \
    --configuration "${BUILD_CONFIGURATION}" \
    --no-restore \
    --output /app/publish \
    /p:UseAppHost=false \
    /p:ContinuousIntegrationBuild=true

FROM mcr.microsoft.com/dotnet/aspnet:${DOTNET_VERSION}-noble-chiseled-extra AS production
WORKDIR /app

ENV ASPNETCORE_ENVIRONMENT=Production \
    ASPNETCORE_URLS=http://+:5059 

EXPOSE 8080

COPY --from=publish --chown=1654:1654 /app/publish ./

USER 1654
ENTRYPOINT ["dotnet", "QuizForge.dll"]
