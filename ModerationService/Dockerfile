FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*
COPY ["ModerationService.csproj", "./"]
RUN dotnet restore "ModerationService.csproj"
COPY . .
RUN dotnet publish "ModerationService.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

RUN apt-get update && \
    apt-get install -y curl && \
    rm -rf /var/lib/apt/lists/*

EXPOSE 8080
COPY --from=build /app/publish .

RUN mkdir -p /app/images/incidents && \
    chmod -R 777 /app/images

ENTRYPOINT ["dotnet", "ModerationService.dll"]