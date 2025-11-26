FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# copy csproj and restore as distinct layers
COPY ["MySaaSAgent.API/MySaaSAgent.API.csproj", "MySaaSAgent.API/"]
RUN dotnet restore "MySaaSAgent.API/MySaaSAgent.API.csproj"

# copy everything else and build
COPY . .
WORKDIR "/src/MySaaSAgent.API"
RUN dotnet publish "MySaaSAgent.API.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "MySaaSAgent.API.dll"]
