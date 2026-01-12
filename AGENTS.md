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
