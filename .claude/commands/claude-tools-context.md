---
title: Claude Tools Project Context
date: 2026-02-08
type: technical-doc
status: active
tags:
- tech/vue
- tech/typescript
- tech/csharp
- tech/sqlite
- domain/personal-assistant
- domain/llm
- domain/network-tools
---

# Claude Tools - Project Context

Personal assistant app for shell-based tasks (network monitoring, port survey) and LLM-powered tools via Claude API, with full audit trail.

## Project Location

**Main Project**: `/home/tgarcin/Git/ML/claude-tools/`
- `ClaudeTools/` - C# ASP.NET Core 8 backend (Minimal API + SQLite)
- `ClaudeToolsViewer/` - Vue3 + Quasar frontend

**GitHub**: `git@github.com:tgarcin/claude-tools.git`

**Tmux Dev Script**: `~/bin/claude-tools-dev.sh`

## Quick Start

```bash
# Option 1: Tmux dev environment (recommended)
claude-tools-dev.sh

# Option 2: Manual
# Terminal 1: Backend
cd ~/Git/ML/claude-tools/ClaudeTools
dotnet run    # http://localhost:5000

# Terminal 2: Frontend
cd ~/Git/ML/claude-tools/ClaudeToolsViewer
yarn dev      # http://localhost:9000 (proxies /api → :5000)
```

## Architecture

### Backend (ClaudeTools/)

- **Framework**: ASP.NET Core 8 Minimal API
- **Database**: SQLite via EF Core (`claudetools.db`, auto-migrates on startup)
- **Pattern**: Vertical slice (Features folder)
- **Claude API**: Raw HttpClient → `https://api.anthropic.com/v1/messages`
- **Audit**: Every API call logged (prompt, response, tokens, success/error)

**API Key**: Set in `appsettings.json` → `Claude:ApiKey` or env var `ANTHROPIC_API_KEY`

### Frontend (ClaudeToolsViewer/)

- **Framework**: Vue 3 + Composition API (`<script setup lang="ts">`)
- **UI**: Quasar 2 (dark theme)
- **State**: Pinia stores
- **Accent color**: Purple `#c084fc`
- **Package manager**: Yarn (PnP)

## API Endpoints

| Endpoint | Method | Description |
|----------|--------|-------------|
| `/api/chat` | POST | Send message to Claude, returns response + token usage |
| `/api/audit` | GET | List recent audit logs (default 20) |
| `/api/audit/{id}` | GET | Full audit log detail (includes prompt/response) |

**Chat Request:**
```json
{ "message": "What's the weather in Montreal?" }
```

**Chat Response:**
```json
{ "text": "...", "tokensIn": 42, "tokensOut": 128 }
```

## Key Files

### Backend
- `Program.cs` - App bootstrap, DI, SQLite, CORS, endpoint mapping
- `Data/AppDbContext.cs` - EF Core context with AuditLog DbSet
- `Data/Entities/AuditLog.cs` - Audit entity (timestamp, operation, input/output, tokens, errors)
- `Features/Chat/ChatService.cs` - Claude API client with auto-audit logging
- `Features/Chat/ChatEndpoint.cs` - POST /api/chat
- `Features/Audit/AuditEndpoint.cs` - GET /api/audit
- `appsettings.json` - Claude API key + model config

### Frontend
- `src/stores/chat.ts` - Pinia store (ask, loading, error, lastResponse)
- `src/components/WeatherCard.vue` - Location input → Claude weather query
- `src/layouts/MainLayout.vue` - Dark header bar
- `src/pages/IndexPage.vue` - Home page with WeatherCard
- `src/css/quasar.variables.scss` - Purple theme variables
- `quasar.config.js` - Dev proxy :9000 → :5000

## Development Commands

```bash
# Backend
cd ~/Git/ML/claude-tools/ClaudeTools
dotnet build
dotnet run
dotnet ef migrations add <MigrationName>

# Frontend
cd ~/Git/ML/claude-tools/ClaudeToolsViewer
yarn install
yarn lint
yarn tsc
yarn dev
yarn build
```

## Planned Features

- Network monitoring (ping, traceroute, port scan)
- Port survey / service discovery
- LLM-powered code analysis
- Shell command execution with audit trail
- System health dashboard

## Conventions

- **Commit format**: Conventional commits (`feat`, `fix`, `docs`, `refactor`)
- **Backend JSON**: camelCase (configured in Program.cs)
- **Frontend**: Composition API, Pinia setup stores, Quasar dark theme
- **Audit**: All external API calls must be logged to AuditLog table
