# Use the .NET 9 runtime as a parent image
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Use the .NET 9 SDK as build image
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj and restore as distinct layers
COPY ["FreeMarket.Tech.Challenge.Api/FreeMarket.Tech.Challenge.Api.csproj", "FreeMarket.Tech.Challenge.Api/"]
RUN dotnet restore "FreeMarket.Tech.Challenge.Api/FreeMarket.Tech.Challenge.Api.csproj"

# Copy everything else and build
COPY . .
WORKDIR "/src/FreeMarket.Tech.Challenge.Api"
RUN dotnet build "FreeMarket.Tech.Challenge.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "FreeMarket.Tech.Challenge.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Build runtime image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Set environment variables for proper container behavior
ENV ASPNETCORE_ENVIRONMENT=Development
ENV ASPNETCORE_URLS=http://+:80
ENV ASPNETCORE_HTTPS_PORTS=""
ENV ASPNETCORE_HTTP_PORTS=80

ENTRYPOINT ["dotnet", "FreeMarket.Tech.Challenge.Api.dll"]