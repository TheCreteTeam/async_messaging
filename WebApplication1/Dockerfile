FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Use Azure Devops Artifact
#ENV AZURE_ARTIFACTS_FEED_URL=https://pkgs.dev.azure.com/TheCreteTeam/6733bd99-93cd-4992-933d-26bd2a613104/_packaging/async_mesaging_common/nuget/v3/index.json
#ARG AZURE_DEVOPS_TOKEN=PASS_AS_AN_ARG
#RUN echo $AZURE_ARTIFACTS_FEED_URL
#RUN echo $AZURE_DEVOPS_TOKEN
#RUN dotnet nuget add source $AZURE_ARTIFACTS_FEED_URL \
#          --name azure \
#          --username az \
#          --password $AZURE_DEVOPS_TOKEN \
#          --store-password-in-clear-text

# Use local built Artifact
COPY ["AsyncMessagingCommon/AsyncMessagingCommon.csproj", "AsyncMessagingCommon/"]
COPY ["WebApplication1/WebApplication1.csproj", "WebApplication1/"]

RUN dotnet restore "AsyncMessagingCommon/AsyncMessagingCommon.csproj"
COPY . .
WORKDIR "/src/AsyncMessagingCommon"
#RUN dotnet build "AsyncMessagingCommon.csproj" -c Release
RUN dotnet pack --configuration Release --output /app/nupkg

WORKDIR "/src/WebApplication1"
RUN ls -la /src 
RUN ls -lah /app/nupkg

#RUN dotnet add package AsyncMessagingCommon -v 1.0.0 -n -s /app/nupkg/AsyncMessagingCommon.1.0.0.nupkg
RUN cat WebApplication1.csproj
RUN echo '<configuration>\n\
  <packageSources>\n\
    <add key="local-packages" value="/app/nupkg/" />\n\
  </packageSources>\n\
</configuration>' > NuGet.config
RUN cat NuGet.config
RUN dotnet restore "WebApplication1.csproj"

WORKDIR /src
COPY . .

WORKDIR /src/WebApplication1
RUN dotnet build "WebApplication1.csproj" -c Release -o /app/build --no-restore

FROM build AS publish
RUN dotnet publish "WebApplication1.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "WebApplication1.dll"]
