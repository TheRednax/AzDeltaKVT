# AzDeltaKVT
https://www.figma.com/design/06Imh9wh8HBFWP798KUXSq/AzDeltaKVT?node-id=0-1&t=jthNqfR45DHanc3X-1  -> crude figma wireframes to understand the assignment better

Added Kanban project to have some overview for what we need to do

# ---------------------------------------------------------------------------------------------------------------------------------------
#           Connection to database
# ---------------------------------------------------------------------------------------------------------------------------------------
  "ConnectionStrings:DefaultConnection": "Server=sqlserver;Database=AzDeltaKVTDatabase;User Id=sa;Password=Vives2023!;TrustServerCertificate=True;"

# ---------------------------------------------------------------------------------------------------------------------------------------
#           How to start docker (via bash)
# ---------------------------------------------------------------------------------------------------------------------------------------
cd directory/to/your/docker-compose.yml   --> Go to the directory where docker-compose is located
docker-compose up -d --build              --> This will build/create/run your docker with port 1433


# ---------------------------------------------------------------------------------------------------------------------------------------
#           Explanation Dockerfile API
# ---------------------------------------------------------------------------------------------------------------------------------------

# A Docker image is like a blueprint or snapshot of a filesystem. It includes:
# An OS base layer (often Debian or Alpine)
# System dependencies (like .NET runtime, libraries, etc.)
# Any configuration (like environment variables, entry points)
# Without an image, Docker wouldn't know what environment your app needs to run. You'd have to build all that from scratch — OS, runtime, config — which is impractical.
# By using mcr.microsoft.com/dotnet/aspnet:9.0, you're telling Docker:
# "Start with a prebuilt environment that has everything required to run an ASP.NET Core app targeting .NET 9."

# -------------------------------
# Base stage - Runtime only
# -------------------------------
# Use ASP.NET runtime image (no SDK) for the final container that runs the app
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
# Expose port 8080 to allow external access (Docker Compose, browsers, etc.)
EXPOSE 8080

# -------------------------------
# Build stage - Compiles the app
# -------------------------------
# Use the .NET SDK image, which includes compilers and build tools
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build

# Optional build configuration (default: Release)
ARG BUILD_CONFIGURATION=Release

# Set working directory for the source code
WORKDIR /src

# Leverage caching means: take advantage of Docker’s layer caching to speed up rebuilds.
# NuGet restore (next step) only depends on the .csproj file.
# If your project file hasn't changed, Docker will reuse the cached result and skip re-running dotnet restore.
# This makes builds much faster, especially when you rebuild after only changing source code.
COPY ["AzDeltaKVT.API/AzDeltaKVT.API.csproj", "AzDeltaKVT.API/"]

# dotnet restore downloads all NuGet dependencies listed in the .csproj file.
# These are stored in the container layer, ready for the next build step.
# Think of it like: “Get all the external libraries I need for this project to compile.”
RUN dotnet restore "AzDeltaKVT.API/AzDeltaKVT.API.csproj"

# You copy in the actual code (controllers, services, Program.cs, etc.).
COPY . .

# Move to the project directory to build the application
WORKDIR "/src/AzDeltaKVT.API"

# dotnet build: Compiles the code into intermediate output (DLLs, EXEs, etc.)
# -c Release: Sets the build configuration (can be Debug or Release)
# Release is optimized and ready for deployment
# Debug includes symbols and debugging info
# -o /app/build: Specifies the output directory inside the container
RUN dotnet build "AzDeltaKVT.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

# -------------------------------
# Publish stage - Prepares the app for deployment
# -------------------------------
FROM build AS publish
ARG BUILD_CONFIGURATION=Release

# Publish the app into a folder that contains everything needed to run it
# /p:UseAppHost=false disables platform-specific executable stubs
RUN dotnet publish "AzDeltaKVT.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# -------------------------------
# Final stage - Minimal runtime container
# -------------------------------
# Start from the base runtime-only image again
FROM base AS final

# Set working directory inside the container
WORKDIR /app

# Copy the published app from the previous stage into this minimal image
COPY --from=publish /app/publish .

# Command to run the application when the container starts
CMD ["dotnet", "AzDeltaKVT.API.dll"]


# ---------------------------------------------------------------------------------------------------------------------------------------
#           Explanation Dockerfile UI
# ---------------------------------------------------------------------------------------------------------------------------------------

# -------------------------------
# Build stage - Builds the front-end
# -------------------------------

# Use the .NET 9.0 SDK image based on Ubuntu Noble
# This image contains everything needed to build .NET apps (compiler, CLI tools, etc.)
FROM mcr.microsoft.com/dotnet/sdk:9.0-noble AS build

# Set the working directory inside the container
WORKDIR /app

# Copy all source files from the host into the container
COPY . .

# Build and publish the front-end project (Blazor, Razor, etc.)
# dotnet publish:
# - Compiles the project
# - Copies static assets (CSS, JS, HTML)
# - Puts the output into the specified folder
# -c Release: use the optimized Release build configuration
# -o /app/publish: output everything to /app/publish
RUN dotnet publish AzDeltaKVT.UI/AzDeltaKVT.UI.csproj -c Release -o /app/publish

# -------------------------------
# Runtime stage - Static file server using NGINX
# -------------------------------

# Use a minimal NGINX image based on Alpine Linux
# This image is small and ideal for serving static files (HTML, JS, CSS)
FROM nginx:alpine

# Copy the built front-end output (wwwroot) from the build container
# into the default NGINX web root directory
COPY --from=build /app/publish/wwwroot /usr/share/nginx/html

# Expose port 80 to allow web traffic (HTTP)
EXPOSE 80

# NGINX will automatically serve the static files from /usr/share/nginx/html
# No CMD is needed here because the base nginx:alpine image already defines it



# ---------------------------------------------------------------------------------------------------------------------------------------
#           Explanation Docker-compose
# ---------------------------------------------------------------------------------------------------------------------------------------
# Use Docker Compose version 3.9 (latest stable for most features and compatibility)
version: '3.9'

services:
  # ----------------------------------
  # SQL Server Database Container
  # ----------------------------------
  db:
    # Use the official Microsoft SQL Server 2022 Linux image
    image: mcr.microsoft.com/mssql/server:2022-latest

    # Name the container "sqlserver"
    container_name: sqlserver

    # Set environment variables required to run SQL Server:
    # - SA_PASSWORD: the system administrator password
    # - ACCEPT_EULA: must be set to 'Y' to accept Microsoft's license
    environment:
      - SA_PASSWORD=Vives2023!
      - ACCEPT_EULA=Y

    # Map container port 1433 (SQL Server default) to host port 1433
    ports:
      - "1433:1433"

    # Persist database files across container restarts
    volumes:
      - sqlserverdata:/var/opt/mssql

  # ----------------------------------
  # ASP.NET Core API Container
  # ----------------------------------
  api:
    # Build the image from a Dockerfile
    build:
      # Use the root of the project as the build context (where .sln and folders live)
      context: .
      # Specify the path to the Dockerfile for the API
      dockerfile: AzDeltaKVT.API/Dockerfile

    # Ensure the API only starts after the database container is up
    depends_on:
      - db

    # Pass connection string to the API via environment variable
    # This string uses:
    # - Server=sqlserver (name of the DB service)
    # - User Id and Password as configured above
    environment:
      - ConnectionStrings__DefaultConnection=Server=sqlserver;Database=VoortgangstoetsDb;User Id=sa;Password=Vives2023!;TrustServerCertificate=True;

    # Map container port 8080 (from Dockerfile's EXPOSE) to host port 8080
    ports:
      - "8080:8080"

    # Add a custom command to wait for the database before running the app
    # `sleep 30` gives the SQL Server time to start up
    # Then it starts the ASP.NET app
    command: ["sh", "-c", "sleep 30 && dotnet AzDeltaKVT.API.dll"]

  # ----------------------------------
  # Front-End UI Container (Blazor / Razor)
  # ----------------------------------
  ui:
    # Build the image for the UI project
    build:
      context: .
      dockerfile: AzDeltaKVT.UI/Dockerfile

    # Map container port 80 (served by NGINX) to host port 80 (HTTP)
    ports:
      - "80:80"

    # Wait for the API container to be running before starting UI
    depends_on:
      - api

# ----------------------------------
# Volumes - Shared persistent storage
# ----------------------------------
volumes:
  # A named volume to persist SQL Server data outside the container lifecycle
  sqlserverdata:


