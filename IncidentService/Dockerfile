# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Kopiraj csproj i restore-uj pakete
COPY ["IncidentService.csproj", "./"]
RUN dotnet restore "IncidentService.csproj"

# Kopiraj sav kod
COPY . ./

# Publish - koristi -o /app/publish da bude jasnije
RUN dotnet publish "IncidentService.csproj" \
    -c Release \
    -o /app/publish \
    --no-restore

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Kopiraj objavljenu aplikaciju
COPY --from=build /app/publish .

# ✅ Kreiraj wwwroot direktorijum pre nego što se montira volume
RUN mkdir -p /app/wwwroot/images/incidents

ENV ASPNETCORE_URLS=http://+:80
EXPOSE 80

ENTRYPOINT ["dotnet", "IncidentService.dll"]