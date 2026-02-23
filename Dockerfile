
# Используем .NET 8 SDK для сборки
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Копируем csproj и восстанавливаем зависимости
COPY *.csproj ./
RUN dotnet restore

# Копируем весь код и билдим
COPY . ./
RUN dotnet publish -c Release -o out

# Финальный образ
FROM mcr.microsoft.com/dotnet/runtime:8.0
WORKDIR /app
COPY --from=build /app/out .
CMD ["dotnet", "MathPocket.dll"]
=======
# Сборка проекта
FROM mcr.microsoft.comdotnetsdk8.0 AS build
WORKDIR app
COPY .csproj .
RUN dotnet restore
COPY . .
RUN dotnet publish -c Release -o out

# Запуск проекта
FROM mcr.microsoft.comdotnetruntime8.0
WORKDIR app
COPY --from=build appout .
CMD [dotnet, MathPocket.dll]
565ae6e (Add Dockerfile for Render)
