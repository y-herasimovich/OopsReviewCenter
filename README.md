# OopsReviewCenter

OopsReviewCenter: Blazor/.NET 8 app for recording production incidents, building timelines, documenting impact & root cause, and tracking follow-up action items. Includes user management, role-based access, and comprehensive incident tracking with SQLite persistence.

## Features

- **Incident Management**: Create and track production incidents with severity levels, status, and detailed information
- **Timeline Tracking**: Build detailed timelines of events related to each incident
- **Action Items**: Assign and track follow-up tasks for incident resolution
- **User & Role Management**: Role-based access control with user authentication
- **Tags & Templates**: Organize incidents with tags and use templates for consistency
- **Markdown Export**: Export incident reports to Markdown format
- **SQLite Database**: Local persistence with Entity Framework Core

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [SQLite](https://www.sqlite.org/) (optional, for running seed script manually)

## Getting Started

### 1. Clone the Repository

```bash
git clone https://github.com/y-herasimovich/OopsReviewCenter.git
cd OopsReviewCenter
```

### 2. Restore Dependencies

```bash
dotnet restore
```

### 3. Build the Solution

```bash
dotnet build
```

### 4. Run the Application

```bash
cd OopsReviewCenter
dotnet run
```

The application will be available at:
- HTTP: `http://localhost:5000`
- HTTPS: `https://localhost:5001`

## Database

### Location

The SQLite database file is stored at:
```
OopsReviewCenter/App_Data/oopsreviewcenter.db
```

The `App_Data` directory is automatically created on first run if it doesn't exist.

### Migrations

The project uses Entity Framework Core migrations for database schema management.

#### View Current Migration Status
```bash
cd OopsReviewCenter
dotnet ef migrations list
```

#### Create a New Migration
```bash
cd OopsReviewCenter
dotnet ef migrations add <MigrationName>
```

#### Apply Migrations
Migrations are automatically applied on application startup. To manually apply:
```bash
cd OopsReviewCenter
dotnet ef database update
```

#### Remove Last Migration (if not applied)
```bash
cd OopsReviewCenter
dotnet ef migrations remove
```

### Seeding Test Data

**IMPORTANT**: Test data seeding is manual only and should NOT run automatically on application startup.

#### Option 1: Using sqlite3 CLI
```bash
# From repository root
sqlite3 OopsReviewCenter/App_Data/oopsreviewcenter.db < scripts/seed-test-data.sql
```

#### Option 2: Using SQLite command line
```bash
sqlite3 OopsReviewCenter/App_Data/oopsreviewcenter.db
.read scripts/seed-test-data.sql
.quit
```

#### What the Seed Script Contains
- 4 Roles (Administrator, Incident Manager, Developer, Viewer)
- 8 Users with different roles
- 10 Tags for categorization
- 3 Templates for incident reporting
- 32 Realistic Incidents with varied severities, statuses, and timestamps
- Timeline Events for major incidents
- Action Items for incident follow-up
- Incident-Tag relationships

Some incidents have `ResolvedByUserId` set (resolved incidents), while others are still open or investigating.

## Testing

The solution includes comprehensive unit tests covering:
- Password hashing functionality (determinism, salt differences, verification)
- EF Core model configuration (using in-memory SQLite)
- CRUD operations for all domain entities
- Relationship navigation (User-Role, Incident-User, etc.)

### Run All Tests
```bash
dotnet test
```

### Run Tests with Detailed Output
```bash
dotnet test --verbosity normal
```

### Run Tests from Specific Project
```bash
dotnet test OopsReviewCenterTests/OopsReviewCenterTests.csproj
```

## Project Structure

```
OopsReviewCenter/
├── OopsReviewCenter.sln          # Solution file
├── OopsReviewCenter/             # Main application project
│   ├── Components/               # Blazor components
│   │   ├── Layout/              # Layout components
│   │   └── Pages/               # Page components (Incidents, Details, etc.)
│   ├── Data/                    # Database context
│   │   └── ApplicationDbContext.cs
│   ├── Migrations/              # EF Core migrations
│   ├── Models/                  # Domain entities
│   │   ├── Incident.cs
│   │   ├── User.cs
│   │   ├── Role.cs
│   │   ├── ActionItem.cs
│   │   ├── TimelineEvent.cs
│   │   ├── Tag.cs
│   │   ├── Template.cs
│   │   └── IncidentTag.cs
│   ├── Services/                # Application services
│   │   ├── PasswordHasher.cs
│   │   └── MarkdownExportService.cs
│   ├── wwwroot/                 # Static files
│   ├── App_Data/                # SQLite database location (gitignored)
│   ├── Program.cs               # Application entry point
│   └── OopsReviewCenter.csproj
├── OopsReviewCenterTests/       # Unit test project
│   ├── PasswordHasherTests.cs
│   ├── DatabaseModelTests.cs
│   └── OopsReviewCenterTests.csproj
└── scripts/                     # Database scripts
    └── seed-test-data.sql       # Manual seed script
```

## Domain Models

### Core Entities

- **Role**: User roles with permissions (Admin, Incident Manager, Developer, Viewer)
- **User**: System users with authentication and role assignment
- **Incident**: Production incidents with severity, status, timeline, and resolution tracking
- **TimelineEvent**: Chronological events within an incident
- **ActionItem**: Follow-up tasks for incident resolution
- **Tag**: Categorization tags (Database, API, Security, etc.)
- **Template**: Reusable templates for creating incidents and action items
- **IncidentTag**: Many-to-many relationship between incidents and tags

### Key Relationships

- User → Role (many-to-one)
- Incident → User (ResolvedBy, many-to-one, optional)
- Incident → TimelineEvents (one-to-many)
- Incident → ActionItems (one-to-many)
- Incident ↔ Tags (many-to-many through IncidentTag)

## Security

- Passwords are hashed using PBKDF2 with individual salts
- The `PasswordHasher` service provides secure password hashing and verification
- Database files (*.db, *.db-shm, *.db-wal) are excluded from version control

## Development Notes

- The application uses Blazor Server with interactive components
- EF Core migrations automatically apply on startup via `Database.Migrate()`
- The connection string defaults to `App_Data/oopsreviewcenter.db` if not specified in configuration
- Seed data has been removed from the DbContext to prevent automatic seeding

## Troubleshooting

### Database Locked Error
If you encounter "database is locked" errors:
1. Ensure no other process is accessing the database file
2. Close any open SQLite connections
3. Restart the application

### Migration Errors
If migrations fail to apply:
1. Check the `Migrations` folder exists
2. Verify the database file has write permissions
3. Try manually applying migrations: `dotnet ef database update`

### Build Errors
If the solution fails to build:
1. Clean the solution: `dotnet clean`
2. Restore packages: `dotnet restore`
3. Rebuild: `dotnet build`

## Contributing

This is an educational/demonstration project for incident management. Contributions are welcome!

## License

This project is open source. Check the repository for license details.

