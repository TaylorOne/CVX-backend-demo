# CVX - Collaborative Project Management Platform

A comprehensive ASP.NET Core Web API for managing collaborative projects, milestones, and team members with a token-based compensation system.

## Overview

CVX is a backend API built with ASP.NET Core 8.0 that facilitates collaborative project management. The platform enables organizations (Collaboratives) to create projects, assign milestones, manage team members, and track compensation through a Launch Token system.

## Key Features

- **Collaborative Management**: Create and manage organizational collaboratives with member roles
- **Project Administration**: Full project lifecycle management with approval workflows
- **Milestone Tracking**: Define, assign, and approve project milestones with deliverables
- **Member Management**: Invite system with role-based access control
- **Token Economy**: Launch Token allocation and compensation system with transaction tracking
- **Identity & Authentication**: Built on ASP.NET Core Identity with secure authentication
- **Approval Workflows**: Multi-stage approval processes for projects and milestones

## Tech Stack

- **Framework**: ASP.NET Core 8.0
- **Database**: SQL Server with Entity Framework Core
- **Authentication**: ASP.NET Core Identity
- **Logging**: Serilog with file logging
- **API Documentation**: Swagger/OpenAPI

## Project Structure

```
CVX/
├── Endpoints/              # API endpoint definitions
│   ├── CollaborativeEndpoints.cs
│   ├── ProjectEndpoints.cs
│   ├── MilestoneEndpoints.cs
│   ├── MemberEndpoints.cs
│   ├── ProfileEndpoints.cs
│   └── IdentityEndpoints.cs
├── Models/                 # Data models and entities
├── Services/               # Business logic services
├── Utilities/              # Helper classes and utilities
├── Migrations/             # EF Core database migrations
├── ApplicationDbContext.cs # Database context
└── Program.cs             # Application entry point
```

## Core Concepts

### Collaboratives
Organizations or groups that contain multiple projects and members. Each collaborative has:
- Name, description, and logo
- Launch Token balance for project funding
- Member roles (Admin, Member)
- Approval status workflow

### Projects
Work initiatives within a collaborative that include:
- Budget allocation in Launch Tokens
- Project admin compensation
- Team member assignments with roles
- Milestone tracking
- Multi-stage approval process (Draft → Submitted → Active/Declined)

### Milestones
Discrete units of work with:
- Description and definition of done
- Launch Token allocation
- Assignee tracking
- Completion artifacts and summaries
- Approval workflow
- Automatic payment processing upon approval

### Approval Workflows

PROJECTS: Draft → Submitted → Active/Declined
- All team members must approve for activation
- Project admin can submit for approval
- Members can decline with reasons

MILESTONES: Draft → Submitted → Archived/Declined
- Assignees complete work and submit
- Project admins approve or decline
- Automatic token payment upon approval

## API Endpoints

The API is organized around REST principles with the following main resources:

- **Collaboratives** - Organization management and member administration
- **Projects** - Project lifecycle and approval workflows  
- **Milestones** - Task tracking, completion, and compensation
- **Members** - Team member network and invitations
- **Profile** - User profile operations
- **Identity** - Authentication and registration

### Example Endpoints

```http
POST   /collaboratives          # Create collaborative
GET    /projects/{id}            # Get project details
POST   /projects/{id}/submit     # Submit project for approval
PATCH  /milestones/{id}          # Update milestone status
POST   /login                    # Authenticate user
```

### API Documentation

Full API documentation with request/response schemas is available at:
- **Swagger UI**: `https://localhost:7xxx/swagger` (when running locally)
- **OpenAPI Spec**: `https://localhost:7xxx/swagger/v1/swagger.json`

## Getting Started

### Prerequisites
- .NET 8.0 SDK
- SQL Server
- Visual Studio 2022 or VS Code

### Configuration

1. Configure CORS origins in `Program.cs` for your frontend application

2. Set up email service credentials in appsettings

### Database Setup

```bash
# Apply migrations
dotnet ef database update
```

### Running the Application

```bash
dotnet run
```

The API will be available at:
- Development: `https://localhost:7xxx`
- Swagger UI: `https://localhost:7xxx/swagger`

## Authentication

The API uses cookie-based authentication with ASP.NET Core Identity:
- 360-day session duration
- Secure cookies (HTTPS only)
- SameSite=None for cross-origin requests

## Logging

Logs are written to `logs/log-YYYYMMDD.txt` using Serilog with daily rolling files.

## CORS Configuration

Configured to accept requests from specified Vercel deployment origins. Update `Program.cs` to add your frontend URLs.

## Network Transaction Fee

The system applies a configurable network transaction fee on milestone payments (see `NetworkConstants`).

## License

MIT
