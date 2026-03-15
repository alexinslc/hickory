# Build stage
# Note: .NET 10 images use Ubuntu by default (previously Debian)
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy solution and project files
COPY apps/api/Hickory.Api.csproj apps/api/

# Copy source code
COPY apps/api/ apps/api/

# Build and publish with optimizations
WORKDIR /src/apps/api
RUN dotnet publish Hickory.Api.csproj -c Release -o /app/publish \
    /p:PublishTrimmed=false \
    /p:PublishSingleFile=false

# Runtime stage
# Note: .NET 10 images use Ubuntu by default (previously Debian)
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# Install curl for health checks
RUN apt-get update && apt-get install -y --no-install-recommends curl && rm -rf /var/lib/apt/lists/*

# Create non-root user for security
RUN groupadd -r dotnet && useradd -r -g dotnet dotnet && \
    chown -R dotnet:dotnet /app

# Copy published app
COPY --from=build /app/publish .

# Switch to non-root user
USER dotnet

# Expose port
EXPOSE 8080

# Health check using /health/ready endpoint (checks DB + Redis connectivity)
# The API provides multiple health endpoints:
# - /health: Overall health status (checks DB + Redis)
# - /health/ready: Readiness check (database and cache connections)
# - /health/live: Liveness check (application responsiveness)
# Using /health/ready so the container is only marked healthy when dependencies are reachable
HEALTHCHECK --interval=30s --timeout=10s --start-period=40s --retries=3 \
  CMD curl -f http://localhost:8080/health/ready || exit 1

# Set entry point
ENTRYPOINT ["dotnet", "Hickory.Api.dll"]
