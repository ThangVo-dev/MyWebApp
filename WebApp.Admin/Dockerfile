FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY WebApp.Admin ./WebApp.Admin
COPY WebApp.Service ./WebApp.Service
COPY WebApp.Shared ./WebApp.Shared
COPY WebApp.Data ./WebApp.Data

WORKDIR /src/WebApp.Admin
RUN dotnet restore "WebApp.Admin.csproj"
RUN dotnet build WebApp.Admin.csproj -c Release -o /app/build

FROM build AS publish
RUN dotnet publish /src/WebApp.Admin/WebApp.Admin.csproj -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "WebApp.Admin.dll"]