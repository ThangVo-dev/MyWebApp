FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY WebApp.API ./WebApp.API
COPY WebApp.Data ./WebApp.Data
COPY WebApp.Service ./WebApp.Service
COPY WebApp.Shared ./WebApp.Shared

WORKDIR /src/WebApp.API
RUN dotnet restore "WebApp.API.csproj"
RUN dotnet build WebApp.API.csproj -c Release -o /app/build

FROM build AS publish
RUN dotnet publish /src/WebApp.API/WebApp.API.csproj -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "WebApp.API.dll"]