# Use ASP.NET runtime image for base
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["AzDeltaKVT.API/AzDeltaKVT.API.csproj", "AzDeltaKVT.API/"]
RUN dotnet restore "AzDeltaKVT.API/AzDeltaKVT.API.csproj"
COPY . .
WORKDIR "/src/AzDeltaKVT.API"
RUN dotnet build "AzDeltaKVT.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Publish stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "AzDeltaKVT.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Final stage
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

COPY wait-for-sql.sh /app/wait-for-sql.sh
RUN chmod +x /app/wait-for-sql.sh

ENTRYPOINT ["/app/wait-for-sql.sh", "sqlserver", "dotnet", "AzDeltaKVT.API.dll"]

