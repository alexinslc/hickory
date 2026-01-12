# AGENTS.md

## Project Overview

Hickory is a help desk ticketing system with three apps:
- **api** - .NET 10 backend (PostgreSQL, Redis)
- **web** - Next.js frontend (React 19, TailwindCSS)
- **cli** - TypeScript CLI tool

Monorepo managed with Nx.

## Git Workflow

Always follow this workflow for any feature or fix:

```bash
# 1. Start fresh from main
git checkout main && git pull origin main

# 2. Create feature branch
git checkout -b feature/short-description   # or fix/short-description

# 3. Work on changes, commit with conventional commits
git add . && git commit -m "feat: add ticket search"

# 4. Push and create PR
git push -u origin feature/short-description
```

Then create a PR targeting `main` in `alexinslc/hickory`.

## Build & Test

### API (.NET)
```bash
cd apps/api
dotnet restore
dotnet build
dotnet test
```

### Web (Next.js)
```bash
npx nx build web
npx nx test web
npx nx lint web
```

### CLI
```bash
npx nx build cli
npx nx test cli
```

### All Projects
```bash
npx nx run-many --target=build --all
npx nx run-many --target=test --all
npx nx run-many --target=lint --all
```

## Before Creating a PR

- [ ] Build passes for affected projects
- [ ] Tests pass (add tests for new code)
- [ ] Linting is clean
- [ ] Commits use conventional format (`feat:`, `fix:`, `docs:`, `refactor:`, `test:`)

## Code Style

Follow existing patterns in the codebase:
- Check similar files before creating new ones
- Match naming conventions, file organization, and code structure
- When in doubt, look at neighboring files for guidance

## Security

- Never commit secrets, API keys, or credentials
- Use environment variables for sensitive configuration
- Validate all user input
- Follow existing auth/authorization patterns in the codebase

## Development Environment

Tools available on this machine:

| Category | Tool | Version |
|----------|------|---------|
| **Runtimes** | Node.js | v25.2.0 |
| | .NET | 10.0.101 |
| | Go | 1.25.5 |
| | Python | 3.13.2 |
| | Ruby | 2.6.10 |
| **Package Managers** | npm | via nvm |
| | pnpm | installed |
| | yarn | installed |
| | Homebrew | installed |
| | pipx | installed |
| **Version Control** | Git | 2.52.0 |
| | GitHub CLI | 2.83.2 |
| **Containers** | Docker | 29.1.3 |
| | Docker Compose | 2.40.3 |
| **Cloud/Infra** | Azure CLI | 2.81.0 |
| | Terraform | 1.14.3 |
| | kubectl | 1.35.0 |
| | Helm | 4.0.4 |
| **Utilities** | jq | 1.8.1 |
| | tmux | 3.6a |
| | glow | 2.1.1 |
| | tree | installed |
| | rclone | installed |
| **Terminal** | Warp | installed |
| | zsh | default shell |

**Notes:**
- Use `npx nx` instead of `nx` directly (not globally installed)
- Database clients (psql, redis-cli) run via Docker containers
- Use `gh` CLI for GitHub operations (PRs, issues, etc.)
- `asdf` and `rbenv` available for version management
- `cmake` available for native builds

## MCP Servers

The following MCP (Model Context Protocol) servers are available for AI agents:

| Server | Purpose |
|--------|---------|
| **GitHub** | Full GitHub integration - repos, PRs, issues, branches, code search |
| **Context7** | Fetch up-to-date library documentation and code examples |
| **Microsoft Docs** | Search and fetch official Microsoft/Azure documentation |
| **Docker/Containers** | List, manage, and inspect containers, images, volumes, networks |

**Usage Notes:**
- Use GitHub MCP for creating PRs, searching code, managing issues
- Use Context7 when you need current docs for any library (call `resolve-library-id` first)
- Use Microsoft Docs for Azure, .NET, and other Microsoft technology questions
- Use Container tools to manage local Docker environment
