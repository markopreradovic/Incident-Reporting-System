FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["ModerationService.csproj", "./"]
RUN dotnet restore "ModerationService.csproj"

COPY . .
RUN dotnet publish "ModerationService.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 80

COPY --from=build /app/publish .

# ✅ Kreiraj wwwroot direktorijum pre nego što se montira volume
RUN mkdir -p /app/wwwroot/images/incidents

ENTRYPOINT ["dotnet", "ModerationService.dll"]