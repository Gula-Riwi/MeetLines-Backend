FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# copy csproj and restore as distinct layers
COPY ["MeetLines.API/MeetLines.API.csproj", "MeetLines.API/"]
RUN dotnet restore "MeetLines.API/MeetLines.API.csproj"

# copy everything else and build
COPY . .
WORKDIR "/src/MeetLines.API"
RUN dotnet publish "MeetLines.API.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "MeetLines.API.dll"]
