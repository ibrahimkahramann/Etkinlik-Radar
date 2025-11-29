FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/Services/EventCatalogService/EventCatalogService.csproj", "src/Services/EventCatalogService/"]
COPY ["src/BuildingBlocks/EventBus.Messages/EventBus.Messages.csproj", "src/BuildingBlocks/EventBus.Messages/"]
RUN dotnet restore "src/Services/EventCatalogService/EventCatalogService.csproj"
COPY . .
WORKDIR "/src/src/Services/EventCatalogService"
RUN dotnet build "EventCatalogService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "EventCatalogService.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "EventCatalogService.dll"]
