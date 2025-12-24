FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ApiGateway.csproj ./
RUN dotnet restore ApiGateway.csproj

COPY . ./
RUN dotnet publish ApiGateway.csproj -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

COPY --from=build /app .

COPY ocelot.json .

ENV ASPNETCORE_URLS=http://+:80
EXPOSE 80

ENTRYPOINT ["dotnet", "ApiGateway.dll"]
