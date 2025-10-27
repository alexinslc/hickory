# Data Model: Hickory Help Desk System

**Feature**: 001-help-desk-core  
**Date**: October 26, 2025  
**Technology**: Entity Framework Core 9 with PostgreSQL

## Overview

This document defines the data model for the Hickory Help Desk System. The model is designed to support:
- Ticket creation, tracking, and resolution workflows
- User authentication and role-based authorization
- Organization through categories and tags
- Rich ticket history with comments and attachments
- Full-text search across ticket content
- Optimistic concurrency control for conflict detection

## Entity Relationship Diagram

```
┌─────────────┐         ┌──────────────┐         ┌─────────────┐
│   User      │◄────────┤   Ticket     │────────►│  Category   │
│             │ submitter│              │ category│             │
└─────────────┘         └──────────────┘         └─────────────┘
      ▲                        │ │
      │                        │ └────────┐
      │ assignee               │          │
      │                        ▼          ▼
      │                  ┌──────────┐  ┌──────────────┐
      │                  │ Comment  │  │ TicketTag    │
      │                  │          │  │ (join table) │
      │                  └──────────┘  └──────────────┘
      │                        │               │
      │ author                 │               ▼
      └────────────────────────┘         ┌─────────┐
                                         │   Tag   │
                                         └─────────┘
```

## Core Entities

### User

Represents a person using the system with authentication details, role assignments, and contact information.

**Fields**:

| Field | Type | Constraints | Description |
|-------|------|-------------|-------------|
| `Id` | `Guid` | PK, Required | Unique identifier |
| `Email` | `string(256)` | Required, Unique, Indexed | User's email address (also used for login) |
| `PasswordHash` | `string(512)` | Nullable | Hashed password (null if using SSO only) |
| `FirstName` | `string(100)` | Required | User's first name |
| `LastName` | `string(100)` | Required | User's last name |
| `Role` | `UserRole` (enum) | Required | Role: EndUser, Agent, or Administrator |
| `ExternalProviderId` | `string(256)` | Nullable, Indexed | External identity provider ID (for OAuth/OIDC) |
| `ExternalProvider` | `string(50)` | Nullable | Provider name (e.g., "Google", "AzureAD") |
| `IsActive` | `bool` | Required, Default: true | Whether account is active |
| `CreatedAt` | `DateTime (UTC)` | Required | Account creation timestamp |
| `LastLoginAt` | `DateTime (UTC)` | Nullable | Last successful login timestamp |
| `RowVersion` | `byte[]` | Timestamp | Optimistic concurrency token |

**Indexes**:
- Primary Key: `Id`
- Unique: `Email`
- Index: `ExternalProviderId` (for SSO lookup)
- Index: `Role` (for agent/admin queries)

**Relationships**:
- One-to-Many: User (submitter) → Tickets
- One-to-Many: User (assignee) → Tickets
- One-to-Many: User → Comments

**Validation Rules** (FluentValidation):
- Email: Valid email format, max 256 chars
- FirstName/LastName: 1-100 chars
- Role: Must be valid enum value
- ExternalProviderId: Required if ExternalProvider is set

**State Transitions**: N/A (no state machine)

---

### Ticket

Represents a support request with status tracking, priority classification, and assignment information.

**Fields**:

| Field | Type | Constraints | Description |
|-------|------|-------------|-------------|
| `Id` | `Guid` | PK, Required | Unique identifier |
| `TicketNumber` | `string(20)` | Required, Unique, Indexed | Human-readable ticket ID (e.g., "TKT-00001") |
| `Title` | `string(200)` | Required | Brief ticket summary |
| `Description` | `text` | Required | Detailed ticket description |
| `Status` | `TicketStatus` (enum) | Required | Status: New, Open, InProgress, Resolved, Closed, Reopened |
| `Priority` | `TicketPriority` (enum) | Required | Priority: Low, Normal, High, Critical |
| `SubmitterId` | `Guid` | FK, Required | User who submitted the ticket |
| `AssigneeId` | `Guid` | FK, Nullable | User assigned to the ticket |
| `CategoryId` | `Guid` | FK, Nullable | Ticket category |
| `CreatedAt` | `DateTime (UTC)` | Required | Ticket creation timestamp |
| `UpdatedAt` | `DateTime (UTC)` | Required | Last update timestamp |
| `ResolvedAt` | `DateTime (UTC)` | Nullable | Resolution timestamp |
| `ClosedAt` | `DateTime (UTC)` | Nullable | Closure timestamp |
| `Resolution` | `text` | Nullable | Resolution notes |
| `SearchVector` | `tsvector` | Generated | PostgreSQL full-text search vector |
| `RowVersion` | `byte[]` | Timestamp | Optimistic concurrency token |

**Indexes**:
- Primary Key: `Id`
- Unique: `TicketNumber`
- Index: `SubmitterId` (for user ticket list)
- Index: `AssigneeId` (for agent queue)
- Index: `Status` (for filtering)
- Index: `Priority` (for filtering)
- Index: `CategoryId` (for filtering)
- Index: `CreatedAt` (for sorting)
- GIN Index: `SearchVector` (for full-text search)

**Relationships**:
- Many-to-One: Ticket → User (submitter)
- Many-to-One: Ticket → User (assignee)
- Many-to-One: Ticket → Category
- One-to-Many: Ticket → Comments
- One-to-Many: Ticket → Attachments
- Many-to-Many: Ticket ↔ Tag (via TicketTag)

**Validation Rules** (FluentValidation):
- Title: 5-200 chars
- Description: 10-10000 chars
- Status: Must be valid enum value
- Priority: Must be valid enum value
- TicketNumber: Auto-generated, matches format `TKT-\d{5}`

**State Transitions**:
```
New → Open → InProgress → Resolved → Closed
                ↓             ↓
             Reopened ────────┘
```

**Business Rules**:
- Only submitter or agents can view ticket
- Only agents can change status
- Only agents can assign tickets
- Cannot reopen tickets closed for >30 days
- Resolution notes required when status changes to Resolved

---

### Comment

Represents a reply on a ticket with visibility control for internal agent notes.

**Fields**:

| Field | Type | Constraints | Description |
|-------|------|-------------|-------------|
| `Id` | `Guid` | PK, Required | Unique identifier |
| `TicketId` | `Guid` | FK, Required | Associated ticket |
| `AuthorId` | `Guid` | FK, Required | Comment author |
| `Content` | `text` | Required | Comment content |
| `IsInternal` | `bool` | Required, Default: false | Whether visible to end users |
| `CreatedAt` | `DateTime (UTC)` | Required | Comment creation timestamp |
| `RowVersion` | `byte[]` | Timestamp | Optimistic concurrency token |

**Indexes**:
- Primary Key: `Id`
- Index: `TicketId, CreatedAt` (for ticket timeline)
- Index: `AuthorId` (for author activity)

**Relationships**:
- Many-to-One: Comment → Ticket
- Many-to-One: Comment → User (author)
- One-to-Many: Comment → Attachments

**Validation Rules** (FluentValidation):
- Content: 1-10000 chars
- IsInternal: Can only be set by agents

**Business Rules**:
- Internal comments only visible to agents and admins
- End users can only create public comments
- Comments cannot be deleted, only marked as edited

---

### Category

Represents a predefined classification for organizing tickets.

**Fields**:

| Field | Type | Constraints | Description |
|-------|------|-------------|-------------|
| `Id` | `Guid` | PK, Required | Unique identifier |
| `Name` | `string(100)` | Required, Unique | Category name |
| `Description` | `string(500)` | Nullable | Category description |
| `Color` | `string(7)` | Nullable | Hex color code (e.g., "#FF5733") |
| `IsActive` | `bool` | Required, Default: true | Whether category is active |
| `SortOrder` | `int` | Required, Default: 0 | Display order |
| `CreatedAt` | `DateTime (UTC)` | Required | Creation timestamp |

**Indexes**:
- Primary Key: `Id`
- Unique: `Name`
- Index: `IsActive, SortOrder` (for active category listing)

**Relationships**:
- One-to-Many: Category → Tickets

**Validation Rules** (FluentValidation):
- Name: 2-100 chars
- Description: Max 500 chars
- Color: Valid hex color format

**Business Rules**:
- Cannot delete categories with associated tickets
- Only admins can create/modify categories

---

### Tag

Represents a flexible label for organizing and filtering tickets.

**Fields**:

| Field | Type | Constraints | Description |
|-------|------|-------------|-------------|
| `Id` | `Guid` | PK, Required | Unique identifier |
| `Name` | `string(50)` | Required, Unique | Tag name |
| `Color` | `string(7)` | Nullable | Hex color code |
| `CreatedAt` | `DateTime (UTC)` | Required | Creation timestamp |

**Indexes**:
- Primary Key: `Id`
- Unique: `Name` (case-insensitive)

**Relationships**:
- Many-to-Many: Tag ↔ Ticket (via TicketTag)

**Validation Rules** (FluentValidation):
- Name: 2-50 chars, alphanumeric and hyphens only
- Color: Valid hex color format

**Business Rules**:
- Tags auto-created when first used
- Unused tags can be pruned by admins

---

### TicketTag (Join Table)

Represents the many-to-many relationship between tickets and tags.

**Fields**:

| Field | Type | Constraints | Description |
|-------|------|-------------|-------------|
| `TicketId` | `Guid` | FK, Required | Associated ticket |
| `TagId` | `Guid` | FK, Required | Associated tag |
| `CreatedAt` | `DateTime (UTC)` | Required | Association timestamp |

**Indexes**:
- Composite Primary Key: `(TicketId, TagId)`
- Index: `TagId` (for reverse lookups)

**Relationships**:
- Many-to-One: TicketTag → Ticket
- Many-to-One: TicketTag → Tag

---

### Attachment

Represents a file uploaded to a ticket or comment.

**Fields**:

| Field | Type | Constraints | Description |
|-------|------|-------------|-------------|
| `Id` | `Guid` | PK, Required | Unique identifier |
| `TicketId` | `Guid` | FK, Nullable | Associated ticket (for ticket-level attachments) |
| `CommentId` | `Guid` | FK, Nullable | Associated comment (for comment-level attachments) |
| `FileName` | `string(256)` | Required | Original file name |
| `ContentType` | `string(100)` | Required | MIME type (e.g., "image/png") |
| `SizeBytes` | `long` | Required | File size in bytes |
| `StoragePath` | `string(512)` | Required | Path/key in storage system |
| `UploadedById` | `Guid` | FK, Required | User who uploaded the file |
| `CreatedAt` | `DateTime (UTC)` | Required | Upload timestamp |

**Indexes**:
- Primary Key: `Id`
- Index: `TicketId` (for ticket attachments)
- Index: `CommentId` (for comment attachments)

**Relationships**:
- Many-to-One: Attachment → Ticket
- Many-to-One: Attachment → Comment
- Many-to-One: Attachment → User (uploader)

**Validation Rules** (FluentValidation):
- FileName: 1-256 chars
- SizeBytes: Max 10MB (10,485,760 bytes)
- ContentType: Allowed types (images, documents, logs)
- Exactly one of TicketId or CommentId must be set

**Business Rules**:
- Attachments cannot be modified after upload
- Only ticket participants can download attachments
- Attachments are soft-deleted (marked as deleted, not removed from storage)

---

### KnowledgeArticle (Future Enhancement)

Represents a self-service help document for deflecting support tickets.

**Fields**:

| Field | Type | Constraints | Description |
|-------|------|-------------|-------------|
| `Id` | `Guid` | PK, Required | Unique identifier |
| `Title` | `string(200)` | Required | Article title |
| `Content` | `text` | Required | Article content (Markdown) |
| `Summary` | `string(500)` | Required | Brief summary for search results |
| `AuthorId` | `Guid` | FK, Required | Article author |
| `CategoryId` | `Guid` | FK, Nullable | Associated category |
| `IsPublished` | `bool` | Required, Default: false | Whether publicly visible |
| `ViewCount` | `int` | Required, Default: 0 | Number of views |
| `HelpfulCount` | `int` | Required, Default: 0 | Number of "helpful" votes |
| `UnhelpfulCount` | `int` | Required, Default: 0 | Number of "not helpful" votes |
| `CreatedAt` | `DateTime (UTC)` | Required | Creation timestamp |
| `UpdatedAt` | `DateTime (UTC)` | Required | Last update timestamp |
| `SearchVector` | `tsvector` | Generated | PostgreSQL full-text search vector |
| `RowVersion` | `byte[]` | Timestamp | Optimistic concurrency token |

**Indexes**:
- Primary Key: `Id`
- Index: `IsPublished, ViewCount DESC` (for popular articles)
- Index: `CategoryId` (for category filtering)
- GIN Index: `SearchVector` (for full-text search)

**Relationships**:
- Many-to-One: KnowledgeArticle → User (author)
- Many-to-One: KnowledgeArticle → Category
- Many-to-Many: KnowledgeArticle ↔ Tag (via ArticleTag, similar to TicketTag)

**Note**: Priority P3 feature, may be deferred to later releases.

---

## Enumerations

### UserRole

```csharp
public enum UserRole
{
    EndUser = 0,       // Can create and view own tickets
    Agent = 1,         // Can manage assigned tickets, view all tickets
    Administrator = 2  // Full system access
}
```

### TicketStatus

```csharp
public enum TicketStatus
{
    New = 0,        // Just created, unassigned
    Open = 1,       // Assigned, awaiting work
    InProgress = 2, // Agent actively working
    Resolved = 3,   // Solution provided, awaiting closure
    Closed = 4,     // Ticket closed
    Reopened = 5    // Closed ticket reopened by user
}
```

### TicketPriority

```csharp
public enum TicketPriority
{
    Low = 0,
    Normal = 1,
    High = 2,
    Critical = 3
}
```

---

## Database Migrations Strategy

1. **Code-First Migrations**: Use EF Core migrations for all schema changes
2. **Migration Naming**: `YYYYMMDDHHMMSS_DescriptiveNameOfChange`
3. **Data Seeding**: Seed default categories and admin user in initial migration
4. **Rollback Testing**: All migrations must be tested for up and down operations
5. **Zero-Downtime Deployments**: Migrations must support rolling deployments (additive changes, no breaking renames)

**Initial Migration Includes**:
- All core entities (User, Ticket, Comment, Category, Tag, Attachment)
- Default categories: "Hardware", "Software", "Network", "Access", "Other"
- Admin user (credentials provided via environment variables)
- Full-text search configuration for PostgreSQL

---

## Performance Considerations

### Indexing Strategy

All queries analyzed for index coverage:
- **User Lookups**: Indexed on Email, ExternalProviderId
- **Ticket Queries**: Indexed on SubmitterId, AssigneeId, Status, Priority, CategoryId, CreatedAt
- **Search**: GIN index on SearchVector for full-text search
- **Relationships**: Foreign keys automatically indexed

### Full-Text Search

PostgreSQL `tsvector` for ticket search:
```sql
CREATE INDEX idx_ticket_search ON tickets USING GIN(search_vector);

-- Trigger to update search_vector on insert/update
CREATE TRIGGER ticket_search_update
BEFORE INSERT OR UPDATE ON tickets
FOR EACH ROW EXECUTE FUNCTION
  tsvector_update_trigger(search_vector, 'pg_catalog.english', title, description);
```

### Query Optimization

- **Pagination**: All list queries use `Skip().Take()` with appropriate indexes
- **Lazy Loading Disabled**: Explicit `Include()` statements to prevent N+1 queries
- **Compiled Queries**: EF Core compiled queries for frequently-used queries
- **Read-Only Queries**: Use `AsNoTracking()` for read-only operations

---

## Data Retention & Archival

### Soft Deletes

Tickets and attachments use soft delete pattern:
- Add `IsDeleted` boolean and `DeletedAt` timestamp
- Filter out deleted records in queries via global query filters
- Admins can purge deleted records after retention period

### Audit Trail

EF Core interceptors capture audit information:
- Who created/modified each record
- When records were created/modified
- Stored in separate `AuditLog` table (not shown in this document)

---

## Security Considerations

### Row-Level Security (Business Logic)

- **Tickets**: Users can only view tickets where they are submitter, assignee, or have Agent/Admin role
- **Comments**: Internal comments filtered out for EndUser role
- **Attachments**: Access validated against ticket visibility rules

### Data Protection

- **Password Hashing**: ASP.NET Identity with PBKDF2 (minimum 100,000 iterations)
- **Personal Data**: Email, FirstName, LastName marked with `[PersonalData]` attribute for GDPR compliance
- **Attachment Storage**: Files encrypted at rest (storage provider responsibility)

---

## Technology-Specific Implementation Notes

### Entity Framework Core Configuration

```csharp
public class TicketConfiguration : IEntityTypeConfiguration<Ticket>
{
    public void Configure(EntityTypeBuilder<Ticket> builder)
    {
        builder.HasKey(t => t.Id);
        builder.Property(t => t.TicketNumber).IsRequired().HasMaxLength(20);
        builder.HasIndex(t => t.TicketNumber).IsUnique();
        
        // Optimistic concurrency
        builder.Property(t => t.RowVersion).IsRowVersion();
        
        // Relationships
        builder.HasOne(t => t.Submitter)
               .WithMany()
               .HasForeignKey(t => t.SubmitterId)
               .OnDelete(DeleteBehavior.Restrict);
               
        builder.HasOne(t => t.Assignee)
               .WithMany()
               .HasForeignKey(t => t.AssigneeId)
               .OnDelete(DeleteBehavior.SetNull);
    }
}
```

### PostgreSQL-Specific Features

```csharp
// In DbContext OnModelCreating
modelBuilder.Entity<Ticket>()
    .HasGeneratedTsVectorColumn(
        t => t.SearchVector,
        "english",
        t => new { t.Title, t.Description })
    .HasIndex(t => t.SearchVector)
    .HasMethod("GIN");
```

---

## Testing Strategy

### Unit Tests
- FluentValidation validator tests for all entities
- State transition logic tests for Ticket status

### Integration Tests
- CRUD operations for all entities
- Query performance tests (ensure <100ms for simple queries)
- Full-text search accuracy tests

### Test Data
- Use Bogus library for generating realistic test data
- Testcontainers for PostgreSQL instance in integration tests

---

**Data Model Version**: 1.0  
**Last Updated**: October 26, 2025  
**Next Steps**: Generate API contracts (OpenAPI specification)
