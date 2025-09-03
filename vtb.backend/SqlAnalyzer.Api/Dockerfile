# Используем многостадийную сборку для .NET 9
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Копируем csproj и восстанавливаем зависимости
COPY ["SqlAnalyzer.Api.csproj", "/src/"]
RUN dotnet restore "/src/SqlAnalyzer.Api.csproj"

# Копируем остальной код и публикуем
COPY . /src
RUN dotnet publish "/src/SqlAnalyzer.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Финальный образ на aspnet runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

# Переменная окружения — слушаем на 8080 внутри контейнера
ENV ASPNETCORE_URLS=http://+:8080

# Копируем опубликованное приложение
COPY --from=build /app/publish .

# Открываем порт и запускаем приложение
EXPOSE 8080
ENTRYPOINT ["dotnet", "SqlAnalyzer.Api.dll"]
