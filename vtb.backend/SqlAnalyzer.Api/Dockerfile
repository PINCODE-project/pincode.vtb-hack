FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["SqlAnalyzer.Api/SqlAnalyzer.Api.csproj", "SqlAnalyzer.Api/"]
COPY ["SqlAnalyzerLib/SqlAnalyzerLib.csproj", "SqlAnalyzerLib/"]
RUN dotnet restore "SqlAnalyzer.Api/SqlAnalyzer.Api.csproj"
COPY . .
WORKDIR "/src/SqlAnalyzer.Api"
RUN dotnet build "SqlAnalyzer.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

ENV ASPNETCORE_URLS=http://+:8080

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "SqlAnalyzer.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SqlAnalyzer.Api.dll"]
