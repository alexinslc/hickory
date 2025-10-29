#!/bin/bash
set -e

echo "🚀 Starting post-create setup..."

# Navigate to workspace
cd /workspaces/hickory

# Install Node.js dependencies
echo "📦 Installing Node.js dependencies..."
npm install

# Restore .NET packages
echo "📦 Restoring .NET packages..."
cd apps/api
dotnet restore
cd /workspaces/hickory

# Wait for PostgreSQL to be ready
echo "⏳ Waiting for PostgreSQL to be ready..."
until pg_isready -h localhost -U hickory; do
  echo "PostgreSQL is unavailable - sleeping"
  sleep 2
done
echo "✅ PostgreSQL is ready!"

# Wait for Redis to be ready
echo "⏳ Waiting for Redis to be ready..."
until redis-cli -h localhost ping > /dev/null 2>&1; do
  echo "Redis is unavailable - sleeping"
  sleep 2
done
echo "✅ Redis is ready!"

# Run database migrations
echo "🗄️  Running database migrations..."
cd apps/api
dotnet ef database update || echo "⚠️  Database migration failed - you may need to run it manually"
cd /workspaces/hickory

# Set up git config if not already set
if [ -z "$(git config user.name)" ]; then
  echo "⚙️  Git user.name not set. You may want to configure it with:"
  echo "   git config --global user.name 'Your Name'"
fi

if [ -z "$(git config user.email)" ]; then
  echo "⚙️  Git user.email not set. You may want to configure it with:"
  echo "   git config --global user.email 'your.email@example.com'"
fi

echo ""
echo "✨ Setup complete! You can now:"
echo "   • Start the API: cd apps/api && dotnet run"
echo "   • Start the Web: cd apps/web && npm run dev"
echo "   • Run tests: npm test"
echo "   • Build all: npx nx run-many --target=build --all"
echo ""
echo "📝 Services available:"
echo "   • Web Frontend: http://localhost:3000"
echo "   • API Backend: http://localhost:5000"
echo "   • PostgreSQL: localhost:5432"
echo "   • Redis: localhost:6379"
echo "   • MailHog UI: http://localhost:8025"
echo ""
