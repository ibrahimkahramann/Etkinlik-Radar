FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/Services/FollowerService/FollowerService.csproj", "src/Services/FollowerService/"]
RUN dotnet restore "src/Services/FollowerService/FollowerService.csproj"
COPY . .
WORKDIR "/src/src/Services/FollowerService"
RUN dotnet build "FollowerService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "FollowerService.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FollowerService.dll"]
