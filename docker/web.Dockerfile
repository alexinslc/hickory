# Build stage
FROM node:25-alpine3.21 AS deps
WORKDIR /app

# Copy package files (root only - monorepo structure)
COPY package*.json ./
RUN npm install --legacy-peer-deps

# Build stage
FROM node:25-alpine3.21 AS builder
WORKDIR /app

# Copy dependencies
COPY --from=deps /app/node_modules ./node_modules

# Copy source
COPY apps/web ./apps/web
COPY nx.json tsconfig.base.json ./

# Build app
WORKDIR /app/apps/web
RUN npm run build

# Runtime stage
FROM node:25-alpine3.21 AS runtime
WORKDIR /app

ENV NODE_ENV=production
ENV NEXT_TELEMETRY_DISABLED=1

# Copy built app
COPY --from=builder /app/apps/web/.next/standalone ./
COPY --from=builder /app/apps/web/.next/static ./apps/web/.next/static
COPY --from=builder /app/apps/web/public ./apps/web/public

# Expose port
EXPOSE 3000

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=40s --retries=3 \
  CMD node -e "require('http').get('http://localhost:3000/api/health', (r) => { process.exit(r.statusCode === 200 ? 0 : 1); });"

# Set entry point
CMD ["node", "apps/web/server.js"]
