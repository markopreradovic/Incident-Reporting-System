# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Kopiraj csproj i restore-uj pakete
COPY ["IncidentService.csproj", "./"]
RUN dotnet restore "IncidentService.csproj"

# Kopiraj sav kod
COPY . ./

# Publish
RUN dotnet publish "IncidentService.csproj" \
    -c Release \
    -o /app/publish \
    --no-restore

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# ✅ Install curl for health checks
RUN apt-get update && \
    apt-get install -y curl && \
    rm -rf /var/lib/apt/lists/*

# Kopiraj objavljenu aplikaciju
COPY --from=build /app/publish .

# ✅ Kreiraj images direktorijum (matching docker-compose volume)
RUN mkdir -p /app/images/incidents && \
    chmod -R 777 /app/images

ENV ASPNETCORE_URLS=http://+:80
EXPOSE 80

ENTRYPOINT ["dotnet", "IncidentService.dll"]