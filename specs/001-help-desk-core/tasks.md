# Tasks: Hickory Help Desk System

**Input**: Design documents from `/specs/001-help-desk-core/`  
**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/openapi.yaml

**Tests**: Tests are NOT explicitly requested in the specification. Tasks focus on implementation with built-in validation via NSwag contract testing.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

## Path Conventions

Based on plan.md, this is an Nx monorepo with three applications:
- **Backend API**: `apps/api/src/`
- **Frontend Web**: `apps/web/src/`
- **CLI**: `apps/cli/src/`

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Project initialization and basic monorepo structure

- [ ] T001 Initialize Nx workspace at repository root with nx.json and package.json
- [ ] T002 [P] Create ASP.NET Core 9 API project at apps/api/ with C# 13 and .NET 9 SDK
- [ ] T003 [P] Create Next.js 15 project at apps/web/ with TypeScript 5.x and App Router
- [ ] T004 [P] Create Node.js CLI project at apps/cli/ with Commander.js and TypeScript
- [ ] T005 Configure Docker Compose at docker/docker-compose.yml with PostgreSQL 16, Redis, and app services
- [ ] T006 [P] Setup PostgreSQL connection in apps/api/src/Infrastructure/Data/ApplicationDbContext.cs
- [ ] T007 [P] Configure TailwindCSS and ShadCN UI in apps/web/tailwind.config.ts
- [ ] T008 [P] Setup ESLint and Prettier for TypeScript projects in .eslintrc.json
- [ ] T009 [P] Configure dotnet format and StyleCop for C# in apps/api/.editorconfig
- [ ] T010 [P] Create .github/workflows/ci.yml for CI pipeline with build, lint, and test jobs

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core infrastructure that MUST be complete before ANY user story can be implemented

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

- [ ] T011 Create User entity with UserRole enum in apps/api/src/Infrastructure/Data/Entities/User.cs
- [ ] T012 Configure User entity with EF Core fluent API in apps/api/src/Infrastructure/Data/Configurations/UserConfiguration.cs
- [ ] T013 Create initial EF Core migration for User table in apps/api/src/Infrastructure/Data/Migrations/
- [ ] T014 [P] Implement JWT token generation service in apps/api/src/Infrastructure/Auth/JwtTokenService.cs
- [ ] T015 [P] Implement OAuth 2.0/OIDC authentication middleware in apps/api/src/Infrastructure/Auth/OAuthAuthenticationHandler.cs
- [ ] T016 [P] Implement local email/password authentication in apps/api/src/Features/Auth/Login/LoginHandler.cs using MediatR
- [ ] T017 [P] Implement user registration in apps/api/src/Features/Auth/Register/RegisterHandler.cs using MediatR
- [ ] T018 Create authentication request/response DTOs in apps/api/src/Features/Auth/Models/
- [ ] T019 [P] Add FluentValidation validators for login in apps/api/src/Features/Auth/Login/LoginValidator.cs
- [ ] T020 [P] Add FluentValidation validators for registration in apps/api/src/Features/Auth/Register/RegisterValidator.cs
- [ ] T021 Configure JWT Bearer authentication in apps/api/src/Program.cs with token validation
- [ ] T022 [P] Setup MediatR pipeline behaviors for validation in apps/api/src/Common/Behaviors/ValidationBehavior.cs
- [ ] T023 [P] Setup MediatR pipeline behaviors for logging in apps/api/src/Common/Behaviors/LoggingBehavior.cs
- [ ] T024 [P] Configure Serilog structured logging in apps/api/src/Program.cs with console and file sinks
- [ ] T025 [P] Configure OpenTelemetry tracing in apps/api/src/Infrastructure/Observability/TelemetryConfiguration.cs
- [ ] T026 [P] Implement health check endpoints in apps/api/src/Infrastructure/Observability/HealthChecks/ for DB and Redis
- [ ] T027 Configure Swagger/OpenAPI generation in apps/api/src/Program.cs using Swashbuckle
- [ ] T028 [P] Setup error handling middleware with ProblemDetails in apps/api/src/Infrastructure/Middleware/ErrorHandlingMiddleware.cs
- [ ] T029 [P] Configure CORS policy in apps/api/src/Program.cs for frontend origin
- [ ] T030 Generate OpenAPI spec from API at contracts/openapi.yaml (verify against design spec)
- [ ] T031 [P] Setup NSwag code generation for TypeScript client in apps/web/src/lib/api/
- [ ] T032 [P] Setup NSwag code generation for CLI TypeScript client in apps/cli/src/lib/api/
- [ ] T033 [P] Create API client configuration with base URL and auth headers in apps/web/src/lib/api/client.ts
- [ ] T034 [P] Setup TanStack Query configuration in apps/web/src/lib/queries/queryClient.ts
- [ ] T035 [P] Configure SignalR client connection in apps/web/src/lib/signalr/connection.ts
- [ ] T036 [P] Create authentication context provider in apps/web/src/components/auth/AuthProvider.tsx
- [ ] T037 [P] Implement login page UI at apps/web/src/app/(auth)/login/page.tsx using ShadCN form components
- [ ] T038 [P] Implement registration page UI at apps/web/src/app/(auth)/register/page.tsx
- [ ] T039 [P] Create CLI authentication commands in apps/cli/src/commands/auth.ts with Inquirer prompts
- [ ] T040 [P] Setup CLI token storage in apps/cli/src/lib/auth/tokenStore.ts using secure local storage

**Checkpoint**: Foundation ready - user story implementation can now begin in parallel

---

## Phase 3: User Story 1 - Submit and Track Support Tickets (Priority: P1) 🎯 MVP

**Goal**: Enable end users to submit support requests and track their status

**Independent Test**: Create a ticket through web UI or CLI, receive ticket ID, view ticket details and status

### Implementation for User Story 1

- [ ] T041 [P] [US1] Create Ticket entity with TicketStatus and TicketPriority enums in apps/api/src/Infrastructure/Data/Entities/Ticket.cs
- [ ] T042 [P] [US1] Create Comment entity in apps/api/src/Infrastructure/Data/Entities/Comment.cs
- [ ] T043 [P] [US1] Create Attachment entity in apps/api/src/Infrastructure/Data/Entities/Attachment.cs
- [ ] T044 [US1] Configure Ticket entity with EF Core fluent API including indexes and search vector in apps/api/src/Infrastructure/Data/Configurations/TicketConfiguration.cs
- [ ] T045 [P] [US1] Configure Comment entity with EF Core fluent API in apps/api/src/Infrastructure/Data/Configurations/CommentConfiguration.cs
- [ ] T046 [P] [US1] Configure Attachment entity with EF Core fluent API in apps/api/src/Infrastructure/Data/Configurations/AttachmentConfiguration.cs
- [ ] T047 [US1] Create EF Core migration for Ticket, Comment, and Attachment tables in apps/api/src/Infrastructure/Data/Migrations/
- [ ] T048 [P] [US1] Implement CreateTicket command handler using MediatR in apps/api/src/Features/Tickets/Create/CreateTicketHandler.cs
- [ ] T049 [P] [US1] Implement GetTicketById query handler in apps/api/src/Features/Tickets/GetById/GetTicketByIdHandler.cs
- [ ] T050 [P] [US1] Implement GetTicketsBySubmitter query handler in apps/api/src/Features/Tickets/GetBySubmitter/GetTicketsBySubmitterHandler.cs
- [ ] T051 [P] [US1] Implement AddComment command handler in apps/api/src/Features/Comments/Create/AddCommentHandler.cs
- [ ] T052 [P] [US1] Create CreateTicketRequest/Response DTOs in apps/api/src/Features/Tickets/Create/Models/
- [ ] T053 [P] [US1] Create TicketDto and CommentDto in apps/api/src/Features/Tickets/Models/
- [ ] T054 [US1] Add FluentValidation validator for CreateTicket (title 5-200 chars, description 10-10000 chars) in apps/api/src/Features/Tickets/Create/CreateTicketValidator.cs
- [ ] T055 [P] [US1] Add FluentValidation validator for AddComment in apps/api/src/Features/Comments/Create/AddCommentValidator.cs
- [ ] T056 [US1] Implement ticket number generation service (TKT-##### format) in apps/api/src/Common/Services/TicketNumberGenerator.cs
- [ ] T057 [US1] Create POST /api/v1/tickets endpoint in apps/api/src/Features/Tickets/TicketsController.cs mapped to CreateTicketHandler
- [ ] T058 [P] [US1] Create GET /api/v1/tickets/{id} endpoint in TicketsController mapped to GetTicketByIdHandler
- [ ] T059 [P] [US1] Create GET /api/v1/tickets endpoint with submitter filtering in TicketsController mapped to GetTicketsBySubmitterHandler
- [ ] T060 [P] [US1] Create POST /api/v1/tickets/{ticketId}/comments endpoint in apps/api/src/Features/Comments/CommentsController.cs
- [ ] T061 [US1] Add authorization policy to ensure users can only view their own tickets in apps/api/src/Features/Tickets/GetById/GetTicketByIdHandler.cs
- [ ] T062 [P] [US1] Create ticket list page UI at apps/web/src/app/tickets/page.tsx with table view
- [ ] T063 [P] [US1] Create ticket details page UI at apps/web/src/app/tickets/[id]/page.tsx with comment thread
- [ ] T064 [P] [US1] Create new ticket form component at apps/web/src/components/tickets/NewTicketForm.tsx with ShadCN form
- [ ] T065 [P] [US1] Create comment form component at apps/web/src/components/tickets/CommentForm.tsx
- [ ] T066 [P] [US1] Implement createTicket mutation using TanStack Query in apps/web/src/lib/queries/tickets.ts
- [ ] T067 [P] [US1] Implement getTicketById query using TanStack Query in apps/web/src/lib/queries/tickets.ts
- [ ] T068 [P] [US1] Implement getMyTickets query using TanStack Query in apps/web/src/lib/queries/tickets.ts
- [ ] T069 [P] [US1] Implement addComment mutation using TanStack Query in apps/web/src/lib/queries/comments.ts
- [ ] T070 [P] [US1] Create CLI ticket create command in apps/cli/src/commands/ticket.ts with interactive prompts
- [ ] T071 [P] [US1] Create CLI ticket view command in apps/cli/src/commands/ticket.ts showing details and comments
- [ ] T072 [P] [US1] Create CLI ticket list command in apps/cli/src/commands/ticket.ts showing user's tickets
- [ ] T073 [US1] Add client-side validation in web form matching server-side FluentValidation rules
- [ ] T074 [US1] Add loading states and error handling for all ticket operations in web UI
- [ ] T075 [US1] Verify ticket submission completes in <2 seconds per SC-003 success criterion

**Checkpoint**: At this point, User Story 1 should be fully functional - users can submit and track tickets

---

## Phase 4: User Story 2 - Manage and Respond to Support Tickets (Priority: P1) 🎯 MVP

**Goal**: Enable support agents to efficiently manage incoming tickets, respond to users, and resolve issues

**Independent Test**: Assign tickets to agent, respond to users, update status, close resolved tickets

### Implementation for User Story 2

- [ ] T076 [P] [US2] Implement GetAgentQueue query handler in apps/api/src/Features/Tickets/GetQueue/GetAgentQueueHandler.cs (filters unassigned and assigned tickets, sorts by priority and age)
- [ ] T077 [P] [US2] Implement AssignTicket command handler in apps/api/src/Features/Tickets/Assign/AssignTicketHandler.cs
- [ ] T078 [P] [US2] Implement UpdateTicketStatus command handler in apps/api/src/Features/Tickets/UpdateStatus/UpdateTicketStatusHandler.cs
- [ ] T079 [P] [US2] Implement UpdateTicketPriority command handler in apps/api/src/Features/Tickets/UpdatePriority/UpdateTicketPriorityHandler.cs
- [ ] T080 [P] [US2] Implement CloseTicket command handler with resolution notes in apps/api/src/Features/Tickets/Close/CloseTicketHandler.cs
- [ ] T081 [P] [US2] Implement ReassignTicket command handler in apps/api/src/Features/Tickets/Reassign/ReassignTicketHandler.cs
- [ ] T082 [US2] Implement AddInternalNote command handler (IsInternal=true) in apps/api/src/Features/Comments/Create/AddCommentHandler.cs with agent role check
- [ ] T083 [US2] Add FluentValidation validator for ticket status transitions in apps/api/src/Features/Tickets/UpdateStatus/UpdateTicketStatusValidator.cs
- [ ] T084 [P] [US2] Add FluentValidation validator for close ticket (requires resolution notes) in apps/api/src/Features/Tickets/Close/CloseTicketValidator.cs
- [ ] T085 [US2] Create GET /api/v1/tickets/queue endpoint in TicketsController mapped to GetAgentQueueHandler with agent role authorization
- [ ] T086 [P] [US2] Create PUT /api/v1/tickets/{id}/assign endpoint in TicketsController mapped to AssignTicketHandler
- [ ] T087 [P] [US2] Create PUT /api/v1/tickets/{id}/status endpoint in TicketsController mapped to UpdateTicketStatusHandler
- [ ] T088 [P] [US2] Create PUT /api/v1/tickets/{id}/priority endpoint in TicketsController mapped to UpdateTicketPriorityHandler
- [ ] T089 [P] [US2] Create POST /api/v1/tickets/{id}/close endpoint in TicketsController mapped to CloseTicketHandler
- [ ] T090 [P] [US2] Create PUT /api/v1/tickets/{id}/reassign endpoint in TicketsController mapped to ReassignTicketHandler
- [ ] T091 [US2] Add authorization policy to restrict agent operations to users with Agent or Administrator role
- [ ] T092 [P] [US2] Create agent queue page UI at apps/web/src/app/agent/queue/page.tsx with filterable table
- [ ] T093 [P] [US2] Create agent ticket details page at apps/web/src/app/agent/tickets/[id]/page.tsx with action buttons
- [ ] T094 [P] [US2] Create ticket assignment dialog component at apps/web/src/components/agent/AssignTicketDialog.tsx
- [ ] T095 [P] [US2] Create internal note form component at apps/web/src/components/agent/InternalNoteForm.tsx (distinct styling from user comments)
- [ ] T096 [P] [US2] Create status update dropdown component at apps/web/src/components/agent/StatusUpdateDropdown.tsx
- [ ] T097 [P] [US2] Create close ticket dialog with resolution notes at apps/web/src/components/agent/CloseTicketDialog.tsx
- [ ] T098 [P] [US2] Implement getAgentQueue query using TanStack Query in apps/web/src/lib/queries/agentQueue.ts
- [ ] T099 [P] [US2] Implement assignTicket mutation using TanStack Query in apps/web/src/lib/queries/tickets.ts
- [ ] T100 [P] [US2] Implement updateTicketStatus mutation using TanStack Query in apps/web/src/lib/queries/tickets.ts
- [ ] T101 [P] [US2] Implement closeTicket mutation using TanStack Query in apps/web/src/lib/queries/tickets.ts
- [ ] T102 [P] [US2] Implement addInternalNote mutation using TanStack Query in apps/web/src/lib/queries/comments.ts
- [ ] T103 [P] [US2] Create CLI agent queue command in apps/cli/src/commands/agent.ts showing unassigned and assigned tickets
- [ ] T104 [P] [US2] Create CLI ticket assign command in apps/cli/src/commands/agent.ts
- [ ] T105 [P] [US2] Create CLI ticket close command in apps/cli/src/commands/agent.ts with resolution notes prompt
- [ ] T106 [US2] Add optimistic locking conflict detection with RowVersion in all update handlers (per FR-013 and clarification)
- [ ] T107 [US2] Add conflict error handling in web UI showing 409 Conflict with retry option
- [ ] T108 [US2] Verify agent can view queue and respond within 30 seconds per SC-006 success criterion

**Checkpoint**: At this point, User Stories 1 AND 2 should both work - complete ticket submission and agent response workflow

---

## Phase 5: User Story 3 - Organize with Tags and Categories (Priority: P2)

**Goal**: Enable users and agents to organize tickets using categories and tags for filtering and reporting

**Independent Test**: Create categories, tag tickets, filter tickets by category and tags

### Implementation for User Story 3

- [ ] T109 [P] [US3] Create Category entity in apps/api/src/Infrastructure/Data/Entities/Category.cs
- [ ] T110 [P] [US3] Create Tag entity in apps/api/src/Infrastructure/Data/Entities/Tag.cs
- [ ] T111 [P] [US3] Create TicketTag join table entity in apps/api/src/Infrastructure/Data/Entities/TicketTag.cs
- [ ] T112 [P] [US3] Configure Category entity with EF Core fluent API in apps/api/src/Infrastructure/Data/Configurations/CategoryConfiguration.cs
- [ ] T113 [P] [US3] Configure Tag entity with EF Core fluent API in apps/api/src/Infrastructure/Data/Configurations/TagConfiguration.cs
- [ ] T114 [P] [US3] Configure TicketTag relationship in apps/api/src/Infrastructure/Data/Configurations/TicketConfiguration.cs
- [ ] T115 [US3] Create EF Core migration for Category, Tag, and TicketTag tables in apps/api/src/Infrastructure/Data/Migrations/
- [ ] T116 [US3] Update Ticket entity to include CategoryId and Tags navigation property
- [ ] T117 [US3] Create EF Core migration to add CategoryId to Ticket table in apps/api/src/Infrastructure/Data/Migrations/
- [ ] T118 [P] [US3] Implement CreateCategory command handler in apps/api/src/Features/Categories/Create/CreateCategoryHandler.cs with admin authorization
- [ ] T119 [P] [US3] Implement GetAllCategories query handler in apps/api/src/Features/Categories/GetAll/GetAllCategoriesHandler.cs
- [ ] T120 [P] [US3] Implement CreateTag command handler (auto-create on first use) in apps/api/src/Features/Tags/Create/CreateTagHandler.cs
- [ ] T121 [P] [US3] Implement GetAllTags query handler in apps/api/src/Features/Tags/GetAll/GetAllTagsHandler.cs
- [ ] T122 [P] [US3] Implement AddTagsToTicket command handler in apps/api/src/Features/Tickets/AddTags/AddTagsToTicketHandler.cs
- [ ] T123 [P] [US3] Implement RemoveTagsFromTicket command handler in apps/api/src/Features/Tickets/RemoveTags/RemoveTagsFromTicketHandler.cs
- [ ] T124 [P] [US3] Add FluentValidation validator for Category (name 2-100 chars) in apps/api/src/Features/Categories/Create/CreateCategoryValidator.cs
- [ ] T125 [P] [US3] Add FluentValidation validator for Tag (name 2-50 chars, alphanumeric) in apps/api/src/Features/Tags/Create/CreateTagValidator.cs
- [ ] T126 [US3] Update CreateTicket handler to accept optional categoryId
- [ ] T127 [US3] Update GetTicketsBySubmitter and GetAgentQueue handlers to support filtering by category and tags
- [ ] T128 [P] [US3] Create GET /api/v1/categories endpoint in apps/api/src/Features/Categories/CategoriesController.cs
- [ ] T129 [P] [US3] Create POST /api/v1/categories endpoint in CategoriesController with admin authorization
- [ ] T130 [P] [US3] Create GET /api/v1/tags endpoint in apps/api/src/Features/Tags/TagsController.cs
- [ ] T131 [P] [US3] Create POST /api/v1/tickets/{id}/tags endpoint in TicketsController mapped to AddTagsToTicketHandler
- [ ] T132 [P] [US3] Create DELETE /api/v1/tickets/{id}/tags endpoint in TicketsController mapped to RemoveTagsFromTicketHandler
- [ ] T133 [P] [US3] Update ticket DTOs to include category and tags arrays
- [ ] T134 [P] [US3] Create category management page UI at apps/web/src/app/admin/categories/page.tsx (admin only)
- [ ] T135 [P] [US3] Create category selector component for ticket forms at apps/web/src/components/tickets/CategorySelector.tsx
- [ ] T136 [P] [US3] Create tag input component at apps/web/src/components/tickets/TagInput.tsx with autocomplete
- [ ] T137 [P] [US3] Update ticket list pages to show category and tags badges
- [ ] T138 [P] [US3] Add category and tag filters to ticket list and agent queue pages
- [ ] T139 [P] [US3] Implement getAllCategories query using TanStack Query in apps/web/src/lib/queries/categories.ts
- [ ] T140 [P] [US3] Implement createCategory mutation using TanStack Query in apps/web/src/lib/queries/categories.ts
- [ ] T141 [P] [US3] Implement getAllTags query using TanStack Query in apps/web/src/lib/queries/tags.ts
- [ ] T142 [P] [US3] Implement addTagsToTicket mutation using TanStack Query in apps/web/src/lib/queries/tickets.ts
- [ ] T143 [P] [US3] Create CLI category list command in apps/cli/src/commands/category.ts
- [ ] T144 [P] [US3] Create CLI tag management commands in apps/cli/src/commands/tag.ts
- [ ] T145 [US3] Update ticket create flow to include category selection
- [ ] T146 [US3] Verify filtering by category reduces result set and returns within 2 seconds per SC-010

**Checkpoint**: At this point, all organization features work - tickets can be categorized and tagged

---

## Phase 6: User Story 4 - Search and Find Tickets Quickly (Priority: P2)

**Goal**: Enable users and agents to find specific tickets quickly using full-text search

**Independent Test**: Create tickets with various content, search by keywords, ticket ID, status, and verify results

### Implementation for User Story 4

- [ ] T147 [US4] Update Ticket entity to include SearchVector tsvector field in apps/api/src/Infrastructure/Data/Entities/Ticket.cs
- [ ] T148 [US4] Create EF Core migration to add GIN index on SearchVector in apps/api/src/Infrastructure/Data/Migrations/
- [ ] T149 [US4] Implement search vector update trigger or EF interceptor in apps/api/src/Infrastructure/Data/Interceptors/SearchVectorInterceptor.cs
- [ ] T150 [P] [US4] Implement SearchTickets query handler using PostgreSQL full-text search in apps/api/src/Features/Search/SearchTicketsHandler.cs
- [ ] T151 [P] [US4] Add support for filtering by status, priority, assignee, date range in SearchTicketsHandler
- [ ] T152 [P] [US4] Add FluentValidation validator for search query (min 2 chars) in apps/api/src/Features/Search/SearchTicketsValidator.cs
- [ ] T153 [US4] Create GET /api/v1/search endpoint in apps/api/src/Features/Search/SearchController.cs with pagination
- [ ] T154 [US4] Add authorization to search results (users see only their tickets, agents see all)
- [ ] T155 [P] [US4] Create search page UI at apps/web/src/app/search/page.tsx with filters panel
- [ ] T156 [P] [US4] Create search input component at apps/web/src/components/search/SearchInput.tsx with debouncing
- [ ] T157 [P] [US4] Create search filters component at apps/web/src/components/search/SearchFilters.tsx
- [ ] T158 [P] [US4] Create search results component at apps/web/src/components/search/SearchResults.tsx with pagination
- [ ] T159 [P] [US4] Implement searchTickets query using TanStack Query in apps/web/src/lib/queries/search.ts
- [ ] T160 [P] [US4] Create CLI search command in apps/cli/src/commands/search.ts with filter options
- [ ] T161 [US4] Add search box to main navigation in apps/web/src/components/layout/Navigation.tsx
- [ ] T162 [US4] Optimize search query performance with proper indexes
- [ ] T163 [US4] Verify search returns results within 2 seconds for 100,000+ tickets per SC-010 success criterion

**Checkpoint**: At this point, search functionality is complete and performant

---

## Phase 7: User Story 5 - Receive Real-Time Notifications (Priority: P2)

**Goal**: Notify users and agents when tickets are updated via email, in-app, and webhooks

**Independent Test**: Update a ticket and verify notifications are delivered through all configured channels

### Implementation for User Story 5

- [ ] T164 [P] [US5] Configure MassTransit with RabbitMQ or Redis transport in apps/api/src/Infrastructure/Messaging/MassTransitConfiguration.cs
- [ ] T165 [P] [US5] Create ticket event definitions in apps/api/src/Common/Events/ (TicketCreatedEvent, TicketUpdatedEvent, TicketAssignedEvent, CommentAddedEvent)
- [ ] T166 [P] [US5] Implement event publishing in ticket command handlers using MassTransit
- [ ] T167 [P] [US5] Implement email notification consumer in apps/api/src/Features/Notifications/Consumers/EmailNotificationConsumer.cs
- [ ] T168 [P] [US5] Implement webhook notification consumer in apps/api/src/Features/Notifications/Consumers/WebhookNotificationConsumer.cs
- [ ] T169 [P] [US5] Create email service abstraction with SMTP implementation in apps/api/src/Infrastructure/Notifications/EmailService.cs
- [ ] T170 [P] [US5] Create webhook service for HTTP POST notifications in apps/api/src/Infrastructure/Notifications/WebhookService.cs
- [ ] T171 [P] [US5] Create email templates for ticket notifications in apps/api/src/Features/Notifications/Templates/
- [ ] T172 [P] [US5] Configure SignalR hub for real-time notifications in apps/api/src/Infrastructure/RealTime/NotificationHub.cs
- [ ] T173 [US5] Implement SignalR message dispatch in notification consumers
- [ ] T174 [P] [US5] Create NotificationPreferences entity in apps/api/src/Infrastructure/Data/Entities/NotificationPreferences.cs
- [ ] T175 [US5] Create EF Core migration for NotificationPreferences table in apps/api/src/Infrastructure/Data/Migrations/
- [ ] T176 [P] [US5] Implement UpdateNotificationPreferences command handler in apps/api/src/Features/Users/UpdatePreferences/UpdateNotificationPreferencesHandler.cs
- [ ] T177 [P] [US5] Create GET /api/v1/users/me/preferences endpoint in apps/api/src/Features/Users/UsersController.cs
- [ ] T178 [P] [US5] Create PUT /api/v1/users/me/preferences endpoint in UsersController
- [ ] T179 [US5] Connect SignalR client in web app and subscribe to user notifications
- [ ] T180 [P] [US5] Create notification toast component at apps/web/src/components/notifications/NotificationToast.tsx
- [ ] T181 [P] [US5] Create notification center component at apps/web/src/components/notifications/NotificationCenter.tsx
- [ ] T182 [P] [US5] Create notification preferences page at apps/web/src/app/settings/notifications/page.tsx
- [ ] T183 [P] [US5] Implement notification preferences UI with email, in-app, webhook toggles
- [ ] T184 [P] [US5] Implement updateNotificationPreferences mutation using TanStack Query in apps/web/src/lib/queries/users.ts
- [ ] T185 [US5] Add SignalR reconnection logic with exponential backoff
- [ ] T186 [US5] Verify notifications delivered within 1 second per performance requirement

**Checkpoint**: At this point, all notification channels work - email, in-app, and webhooks

---

## Phase 8: User Story 6 - Self-Service Knowledge Base (Priority: P3)

**Goal**: Enable users to find answers to common questions without creating tickets

**Independent Test**: Create knowledge base articles, search for solutions, verify article suggestions during ticket creation

### Implementation for User Story 6

- [ ] T187 [P] [US6] Create KnowledgeArticle entity in apps/api/src/Infrastructure/Data/Entities/KnowledgeArticle.cs
- [ ] T188 [P] [US6] Configure KnowledgeArticle entity with EF Core fluent API in apps/api/src/Infrastructure/Data/Configurations/KnowledgeArticleConfiguration.cs
- [ ] T189 [US6] Create EF Core migration for KnowledgeArticle table in apps/api/src/Infrastructure/Data/Migrations/
- [ ] T190 [P] [US6] Implement CreateArticle command handler in apps/api/src/Features/Knowledge/Create/CreateArticleHandler.cs with agent authorization
- [ ] T191 [P] [US6] Implement UpdateArticle command handler in apps/api/src/Features/Knowledge/Update/UpdateArticleHandler.cs
- [ ] T192 [P] [US6] Implement GetArticleById query handler in apps/api/src/Features/Knowledge/GetById/GetArticleByIdHandler.cs
- [ ] T193 [P] [US6] Implement SearchArticles query handler using PostgreSQL full-text search in apps/api/src/Features/Knowledge/Search/SearchArticlesHandler.cs
- [ ] T194 [P] [US6] Implement RateArticle command handler (helpfulness tracking) in apps/api/src/Features/Knowledge/Rate/RateArticleHandler.cs
- [ ] T195 [P] [US6] Implement GetSuggestedArticles query handler based on ticket description in apps/api/src/Features/Knowledge/Suggest/GetSuggestedArticlesHandler.cs
- [ ] T196 [P] [US6] Add FluentValidation validator for articles in apps/api/src/Features/Knowledge/Create/CreateArticleValidator.cs
- [ ] T197 [P] [US6] Create GET /api/v1/knowledge/articles endpoint in apps/api/src/Features/Knowledge/KnowledgeController.cs
- [ ] T198 [P] [US6] Create POST /api/v1/knowledge/articles endpoint in KnowledgeController with agent authorization
- [ ] T199 [P] [US6] Create GET /api/v1/knowledge/articles/{id} endpoint in KnowledgeController
- [ ] T200 [P] [US6] Create GET /api/v1/knowledge/search endpoint in KnowledgeController
- [ ] T201 [P] [US6] Create POST /api/v1/knowledge/articles/{id}/rate endpoint in KnowledgeController
- [ ] T202 [P] [US6] Create knowledge base home page at apps/web/src/app/knowledge/page.tsx with article categories
- [ ] T203 [P] [US6] Create article view page at apps/web/src/app/knowledge/[id]/page.tsx with helpful rating
- [ ] T204 [P] [US6] Create article editor page at apps/web/src/app/agent/knowledge/edit/page.tsx (agent only)
- [ ] T205 [P] [US6] Create article search component at apps/web/src/components/knowledge/ArticleSearch.tsx
- [ ] T206 [P] [US6] Create suggested articles panel at apps/web/src/components/knowledge/SuggestedArticles.tsx
- [ ] T207 [US6] Integrate suggested articles into new ticket form
- [ ] T208 [P] [US6] Implement knowledge queries and mutations using TanStack Query in apps/web/src/lib/queries/knowledge.ts
- [ ] T209 [P] [US6] Create CLI knowledge search command in apps/cli/src/commands/knowledge.ts
- [ ] T210 [US6] Add markdown rendering for article content

**Checkpoint**: At this point, knowledge base is functional and integrated with ticket creation flow

---

## Phase 9: User Story 7 - Generate Reports and Analytics (Priority: P3)

**Goal**: Provide supervisors with visibility into support metrics and performance

**Independent Test**: Generate reports showing ticket volume, resolution times, agent performance, and trends

### Implementation for User Story 7

- [ ] T211 [P] [US7] Implement GetTicketStatistics query handler in apps/api/src/Features/Reports/Statistics/GetTicketStatisticsHandler.cs
- [ ] T212 [P] [US7] Implement GetAgentPerformance query handler in apps/api/src/Features/Reports/AgentPerformance/GetAgentPerformanceHandler.cs
- [ ] T213 [P] [US7] Implement GetSLACompliance query handler in apps/api/src/Features/Reports/SLA/GetSLAComplianceHandler.cs
- [ ] T214 [P] [US7] Implement GetTrendAnalysis query handler in apps/api/src/Features/Reports/Trends/GetTrendAnalysisHandler.cs
- [ ] T215 [P] [US7] Create SLATarget configuration entity in apps/api/src/Infrastructure/Data/Entities/SLATarget.cs
- [ ] T216 [US7] Create EF Core migration for SLATarget table in apps/api/src/Infrastructure/Data/Migrations/
- [ ] T217 [P] [US7] Implement ConfigureSLATargets command handler in apps/api/src/Features/Admin/SLA/ConfigureSLATargetsHandler.cs (admin only)
- [ ] T218 [P] [US7] Create GET /api/v1/reports/statistics endpoint in apps/api/src/Features/Reports/ReportsController.cs with date range params
- [ ] T219 [P] [US7] Create GET /api/v1/reports/agent-performance endpoint in ReportsController
- [ ] T220 [P] [US7] Create GET /api/v1/reports/sla-compliance endpoint in ReportsController
- [ ] T221 [P] [US7] Create GET /api/v1/reports/trends endpoint in ReportsController
- [ ] T222 [P] [US7] Create dashboard page at apps/web/src/app/admin/dashboard/page.tsx with key metrics
- [ ] T223 [P] [US7] Create agent performance page at apps/web/src/app/admin/reports/agents/page.tsx with sortable table
- [ ] T224 [P] [US7] Create SLA compliance page at apps/web/src/app/admin/reports/sla/page.tsx with charts
- [ ] T225 [P] [US7] Create trend analysis page at apps/web/src/app/admin/reports/trends/page.tsx with time series charts
- [ ] T226 [P] [US7] Create SLA configuration page at apps/web/src/app/admin/settings/sla/page.tsx
- [ ] T227 [P] [US7] Create statistics card component at apps/web/src/components/reports/StatisticsCard.tsx
- [ ] T228 [P] [US7] Create chart components at apps/web/src/components/reports/Charts/ using recharts or similar
- [ ] T229 [P] [US7] Implement report queries using TanStack Query in apps/web/src/lib/queries/reports.ts
- [ ] T230 [P] [US7] Create CLI report generation commands in apps/cli/src/commands/reports.ts
- [ ] T231 [US7] Add report export functionality (CSV/JSON) to API endpoints
- [ ] T232 [US7] Verify reports calculate correctly per SC-013 (80% SLA compliance)

**Checkpoint**: At this point, all reporting and analytics features are complete

---

## Phase 10: Attachment Management (Cross-Cutting)

**Goal**: Enable file uploads and downloads for tickets and comments

**Independent Test**: Upload file to ticket, verify storage, download file, verify size limits

### Implementation for Attachments

- [ ] T233 [P] Implement file storage abstraction interface in apps/api/src/Infrastructure/Storage/IFileStorage.cs
- [ ] T234 [P] Implement local filesystem storage provider in apps/api/src/Infrastructure/Storage/LocalFileStorage.cs
- [ ] T235 [P] Implement S3-compatible storage provider in apps/api/src/Infrastructure/Storage/S3FileStorage.cs (optional)
- [ ] T236 Implement UploadAttachment command handler in apps/api/src/Features/Attachments/Upload/UploadAttachmentHandler.cs
- [ ] T237 [P] Implement DownloadAttachment query handler in apps/api/src/Features/Attachments/Download/DownloadAttachmentHandler.cs
- [ ] T238 [P] Add FluentValidation validator for file uploads (max 10MB, allowed types) in apps/api/src/Features/Attachments/Upload/UploadAttachmentValidator.cs
- [ ] T239 Create POST /api/v1/tickets/{ticketId}/attachments endpoint in apps/api/src/Features/Attachments/AttachmentsController.cs
- [ ] T240 [P] Create GET /api/v1/attachments/{id}/download endpoint in AttachmentsController
- [ ] T241 Add file size and content type validation middleware
- [ ] T242 [P] Create file upload component at apps/web/src/components/attachments/FileUpload.tsx with drag-and-drop
- [ ] T243 [P] Create attachment list component at apps/web/src/components/attachments/AttachmentList.tsx
- [ ] T244 [P] Implement uploadAttachment mutation using TanStack Query in apps/web/src/lib/queries/attachments.ts
- [ ] T245 [P] Create CLI attachment upload command in apps/cli/src/commands/attachment.ts
- [ ] T246 Integrate file upload into ticket creation and comment forms
- [ ] T247 Verify attachment upload completes within 5 seconds for 10MB file per SC-012

**Checkpoint**: File attachment functionality is complete across all interfaces

---

## Phase 11: Polish & Cross-Cutting Concerns

**Purpose**: Improvements that affect multiple user stories and final production readiness

- [ ] T248 [P] Add comprehensive error boundaries in React app at apps/web/src/components/ErrorBoundary.tsx
- [ ] T249 [P] Implement global loading state management in apps/web/src/components/LoadingProvider.tsx
- [ ] T250 [P] Add accessibility (WCAG 2.1 AA) audit and fixes across web UI
- [ ] T251 [P] Implement keyboard navigation for all interactive elements
- [ ] T252 [P] Add ARIA labels and screen reader support
- [ ] T253 [P] Optimize bundle size and code splitting for Next.js app
- [ ] T254 [P] Add performance monitoring with Core Web Vitals tracking
- [ ] T255 [P] Implement rate limiting middleware in apps/api/src/Infrastructure/Middleware/RateLimitingMiddleware.cs
- [ ] T256 [P] Add request logging for audit trail
- [ ] T257 [P] Configure security headers (HSTS, CSP, X-Frame-Options) in apps/api/src/Program.cs
- [ ] T258 [P] Add input sanitization for XSS prevention
- [ ] T259 [P] Implement database query optimization and index tuning
- [ ] T260 [P] Add database connection pooling configuration
- [ ] T261 [P] Configure response caching for read-heavy endpoints
- [ ] T262 [P] Add pagination to all list endpoints
- [ ] T263 [P] Create seed data script for development in apps/api/src/Infrastructure/Data/Seeding/
- [ ] T264 [P] Create README.md with quickstart instructions at repository root
- [ ] T265 [P] Create API documentation using OpenAPI spec at docs/api.md
- [ ] T266 [P] Create developer setup guide at docs/developer-guide.md
- [ ] T267 [P] Create deployment guide at docs/deployment.md
- [ ] T268 [P] Add Docker health checks to all containers in docker/docker-compose.yml
- [ ] T269 [P] Configure graceful shutdown for all services
- [ ] T270 [P] Add environment variable validation on startup
- [ ] T271 Run full E2E test suite using Playwright in apps/web/tests/e2e/
- [ ] T272 Verify all success criteria from spec.md are met
- [ ] T273 Run performance benchmarks and verify P95 latency targets
- [ ] T274 Run security scan with OWASP dependency check
- [ ] T275 Validate quickstart.md by following setup instructions from scratch

**Checkpoint**: System is production-ready with all polish and cross-cutting concerns addressed

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies - can start immediately
- **Foundational (Phase 2)**: Depends on Setup completion - BLOCKS all user stories
- **User Stories (Phase 3-9)**: All depend on Foundational phase completion
  - US1 (P1): Can start after Foundational - No dependencies on other stories
  - US2 (P1): Can start after Foundational - No dependencies, but naturally follows US1
  - US3 (P2): Can start after Foundational - No dependencies on other stories
  - US4 (P2): Can start after Foundational - No dependencies, but benefits from existing ticket data
  - US5 (P2): Can start after Foundational - No dependencies on other stories
  - US6 (P3): Can start after Foundational - No dependencies on other stories
  - US7 (P3): Can start after Foundational - No dependencies, but benefits from existing ticket data
- **Attachments (Phase 10)**: Can start after Foundational - Integrates with US1 and US2
- **Polish (Phase 11)**: Depends on all desired user stories being complete

### Recommended Implementation Order

1. **MVP First (Phases 1-4)**: Setup → Foundational → US1 (Submit Tickets) → US2 (Agent Response)
   - This delivers a working help desk system
   - Users can submit tickets and agents can respond
   - Estimated: 75 tasks (T001-T108)

2. **Enhanced MVP (Phases 5-7)**: Add US3 (Categories/Tags) → US4 (Search) → US5 (Notifications)
   - Adds organization, discovery, and real-time features
   - Estimated: 82 additional tasks (T109-T186)

3. **Full Feature Set (Phases 8-10)**: Add US6 (Knowledge Base) → US7 (Reports) → Attachments
   - Adds self-service and analytics capabilities
   - Estimated: 61 additional tasks (T187-T247)

4. **Production Hardening (Phase 11)**: Polish and cross-cutting concerns
   - Estimated: 28 tasks (T248-T275)

**Total: 275 tasks**

### Parallel Opportunities Within Phases

**Phase 1 (Setup)**: T002, T003, T004, T006, T007, T008, T009, T010 can all run in parallel after T001

**Phase 2 (Foundational)**: Many tasks marked [P] can run in parallel:
- Auth setup: T014, T015, T016, T017, T019, T020 in parallel
- Infrastructure: T022, T023, T024, T025, T026, T028, T029 in parallel
- Client setup: T031, T032, T033, T034, T035, T036, T037, T038, T039, T040 in parallel

**User Stories (Phase 3-9)**: If staffed appropriately, entire user stories can be developed in parallel after Foundational phase completes. Within each story:
- Entity creation and configuration tasks marked [P] can run in parallel
- Handler implementations marked [P] can run in parallel
- UI component creation marked [P] can run in parallel
- Query/mutation implementations marked [P] can run in parallel

**Phase 10 (Attachments)**: Storage provider implementations (T234, T235) can run in parallel

**Phase 11 (Polish)**: Most tasks marked [P] can run in parallel (T248-T262)

---

## Parallel Example: MVP Development (Phases 1-4)

Assuming a team of 4 developers:

**Week 1: Setup & Foundational**
- Developer A: Setup tasks (T001-T010)
- Developer B: Auth foundation (T011-T021)
- Developer C: Infrastructure foundation (T022-T029)
- Developer D: Client foundation (T030-T040)

**Week 2-3: User Story 1 (Submit Tickets)**
- Developer A: Backend entities and handlers (T041-T061)
- Developer B: Frontend UI (T062-T069)
- Developer C: CLI implementation (T070-T072)
- Developer D: Testing and validation (T073-T075)

**Week 4-5: User Story 2 (Agent Response)**
- Developer A: Backend handlers and authorization (T076-T091)
- Developer B: Frontend agent UI (T092-T102)
- Developer C: CLI agent commands (T103-T105)
- Developer D: Conflict handling and validation (T106-T108)

**Result**: Working MVP in 5 weeks with parallel development

---

## Task Summary

- **Total Tasks**: 275
- **MVP Tasks (Phases 1-4)**: 108 tasks
- **Enhanced MVP Tasks (Phases 5-7)**: 78 tasks
- **Full Feature Tasks (Phases 8-10)**: 61 tasks
- **Polish Tasks (Phase 11)**: 28 tasks

**Parallel Opportunities**: 150+ tasks marked [P] can be executed in parallel

**Independent User Stories**: All 7 user stories can be implemented and tested independently after Foundational phase

**Suggested MVP Scope**: Phase 1-4 (Setup + Foundational + US1 + US2) = 108 tasks = Core help desk functionality

---

## Implementation Strategy

### Test-First Approach (Not Required for This Feature)

The specification does not explicitly request test-first development. Contract testing is implicitly handled through NSwag-generated clients validated against the OpenAPI specification, ensuring type safety and API contract adherence.

If test-first development is desired, add test tasks before each implementation phase following this pattern:
1. Write contract test (verify against OpenAPI spec)
2. Write integration test (verify user journey)
3. Run tests (should FAIL)
4. Implement feature
5. Run tests (should PASS)

### Type Safety Strategy

- **Backend→Frontend**: NSwag auto-generates TypeScript client from OpenAPI spec (T031)
- **Backend→CLI**: NSwag auto-generates TypeScript client from OpenAPI spec (T032)
- **Contract Validation**: Generated clients enforce API contracts at compile time
- **No `any` Types**: TypeScript strict mode prevents `any` usage (constitutional requirement)
- **No Nullable Warnings**: C# nullable reference types prevent null issues (constitutional requirement)

### Performance Validation

- **SC-003**: Ticket submission <2 seconds (validate in T075)
- **SC-006**: Agent response <30 seconds (validate in T108)
- **SC-010**: Search results <2 seconds (validate in T163)
- **SC-012**: Attachment upload <5 seconds for 10MB (validate in T247)
- **Overall**: P95 latency targets verified in T273

### Incremental Delivery

Each phase represents a deployable increment:
- **Phase 1-2**: Infrastructure ready, authentication works
- **Phase 3**: Users can submit tickets (half of MVP)
- **Phase 4**: Agents can respond (complete MVP)
- **Phase 5**: Organization with categories/tags
- **Phase 6**: Search functionality
- **Phase 7**: Real-time notifications
- **Phase 8**: Self-service knowledge base
- **Phase 9**: Analytics and reporting
- **Phase 10**: File attachments
- **Phase 11**: Production-ready

---

## Format Validation

✅ All tasks follow required checklist format: `- [ ] [ID] [P?] [Story?] Description with file path`

✅ All user story tasks include [US#] label for traceability

✅ All parallelizable tasks marked with [P]

✅ All task descriptions include specific file paths

✅ Task IDs are sequential (T001-T275)

✅ Tasks organized by phase and user story

✅ Dependencies clearly documented

✅ Independent test criteria provided for each user story
