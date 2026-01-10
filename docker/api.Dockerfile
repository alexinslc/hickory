# Build stage
# Note: .NET 10 images use Ubuntu by default (previously Debian)
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy solution and project files
COPY apps/api/Hickory.Api.csproj apps/api/
RUN dotnet restore apps/api/Hickory.Api.csproj --no-cache

# Copy source code
COPY apps/api/ apps/api/

# Build and publish with optimizations
WORKDIR /src/apps/api
RUN dotnet publish Hickory.Api.csproj -c Release -o /app/publish \
    --no-restore \
    /p:PublishTrimmed=false \
    /p:PublishSingleFile=false

# Runtime stage
# Note: .NET 10 images use Ubuntu by default (previously Debian)
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# Create non-root user for security
RUN groupadd -r dotnet && useradd -r -g dotnet dotnet && \
    chown -R dotnet:dotnet /app

# Copy published app
COPY --from=build /app/publish .

# Switch to non-root user
USER dotnet

# Expose port
EXPOSE 8080

# Health check (no additional tools needed - uses wget which is in aspnet image)
HEALTHCHECK --interval=30s --timeout=10s --start-period=40s --retries=3 \
  CMD wget --no-verbose --tries=1 --spider http://localhost:8080/health || exit 1

# Set entry point
ENTRYPOINT ["dotnet", "Hickory.Api.dll"]
