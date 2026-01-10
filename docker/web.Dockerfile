# Build stage
FROM node:25-alpine3.21 AS deps
WORKDIR /app

# Copy package files (root only - monorepo structure)
COPY package*.json ./
RUN npm ci --legacy-peer-deps && \
    npm cache clean --force

# Build stage
FROM node:25-alpine3.21 AS builder
WORKDIR /app

# Copy dependencies
COPY --from=deps /app/node_modules ./node_modules

# Copy root package.json for npm scripts
COPY package*.json ./

# Copy source
COPY apps/web ./apps/web
COPY nx.json tsconfig.base.json ./

# Build app using nx from root
RUN npx nx build web && \
    npm prune --production

# Runtime stage
FROM node:25-alpine3.21 AS runtime
WORKDIR /app

ENV NODE_ENV=production
ENV NEXT_TELEMETRY_DISABLED=1

# Create non-root user for security
RUN addgroup -g 1001 -S nodejs && \
    adduser -S nextjs -u 1001

# Copy built app with correct ownership
COPY --from=builder --chown=nextjs:nodejs /app/apps/web/.next/standalone ./
COPY --from=builder --chown=nextjs:nodejs /app/apps/web/.next/static ./apps/web/.next/static
COPY --from=builder --chown=nextjs:nodejs /app/apps/web/public ./apps/web/public

# Switch to non-root user
USER nextjs

# Expose port
EXPOSE 3000

# Health check to verify Next.js server is responding
# Tests the root endpoint (/) which should return 200 OK
HEALTHCHECK --interval=30s --timeout=10s --start-period=40s --retries=3 \
  CMD node -e "require('http').get('http://localhost:3000/', (r) => { process.exit(r.statusCode === 200 ? 0 : 1); }).on('error', () => { process.exit(1); });"

# Set entry point
CMD ["node", "apps/web/server.js"]
