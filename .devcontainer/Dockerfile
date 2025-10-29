# Use the official Microsoft devcontainers image with .NET and Node.js
FROM mcr.microsoft.com/devcontainers/dotnet:1-9.0-bookworm

# Install Node.js 20.x
RUN curl -fsSL https://deb.nodesource.com/setup_20.x | bash - \
    && apt-get update \
    && apt-get install -y nodejs \
    && apt-get clean -y \
    && rm -rf /var/lib/apt/lists/*

# Install additional dependencies
RUN apt-get update && export DEBIAN_FRONTEND=noninteractive \
    && apt-get install -y \
        git \
        curl \
        wget \
        unzip \
        vim \
        nano \
        postgresql-client \
        redis-tools \
        ca-certificates \
        gnupg \
        lsb-release \
    && apt-get clean -y \
    && rm -rf /var/lib/apt/lists/*

# Install Docker CLI (for docker-from-docker approach)
RUN mkdir -p /etc/apt/keyrings \
    && curl -fsSL https://download.docker.com/linux/debian/gpg | gpg --dearmor -o /etc/apt/keyrings/docker.gpg \
    && echo "deb [arch=$(dpkg --print-architecture) signed-by=/etc/apt/keyrings/docker.gpg] https://download.docker.com/linux/debian $(lsb_release -cs) stable" | tee /etc/apt/sources.list.d/docker.list > /dev/null \
    && apt-get update \
    && apt-get install -y docker-ce-cli docker-compose-plugin \
    && apt-get clean -y \
    && rm -rf /var/lib/apt/lists/*

# Install global npm packages
RUN npm install -g npm@latest \
    && npm install -g nx@latest \
    && npm install -g typescript@latest \
    && npm install -g ts-node@latest

# Verify installations
RUN dotnet --version \
    && node --version \
    && npm --version \
    && docker --version \
    && psql --version \
    && redis-cli --version

# Set up non-root user
ARG USERNAME=vscode
ARG USER_UID=1000
ARG USER_GID=$USER_UID

# Add user to docker group
RUN groupadd -g 999 docker || true \
    && usermod -aG docker ${USERNAME}

# Switch to non-root user
USER ${USERNAME}

# Set working directory
WORKDIR /workspaces/hickory
