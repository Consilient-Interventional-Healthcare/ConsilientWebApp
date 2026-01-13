# Consilient

A comprehensive healthcare management platform for patient care coordination, provider assignments, billing, and clinical operations.

## Technology Stack

| Layer | Technologies |
|-------|-------------|
| Frontend | React 19, TypeScript, Vite, Tailwind CSS, TanStack Query |
| Backend | .NET 9, ASP.NET Core, Entity Framework Core, GraphQL |
| Database | SQL Server |
| Cloud | Microsoft Azure |
| CI/CD | GitHub Actions, Terraform |

## Development Environment (Azure)

- **React Application**: https://consilient-react-dev-westus2.azurewebsites.net
- **API**: https://consilient-api-dev-westus2.azurewebsites.net

## Features

- **Patient Management** - Patient records and hospitalization tracking
- **Provider Assignments** - Doctor-to-patient assignment workflows
- **Visit Tracking** - Appointment and visit management
- **Billing & Insurance** - Financial operations and insurance processing
- **Employee Management** - Staff administration

## Getting Started

### Prerequisites

- .NET 9 SDK
- Node.js 20+
- SQL Server
- Azure CLI (for cloud deployment)

### Development Setup

1. Clone the repository
2. Copy `appsettings.local.json.example` to `appsettings.local.json` and configure
3. Run database migrations
4. Start the backend: `dotnet run --project src/Consilient.Api`
5. Start the frontend: `cd src/Consilient.WebApp2 && npm install && npm run dev`

## Documentation

📚 **[Documentation Index](docs/Index.md)** - Complete guide to all project documentation

🗄️ **[Database Documentation](https://consilient-interventional-healthcare.github.io/ConsilientWebApp/dbs/)** - Auto-generated database schema documentation

## Project Structure

```
src/
├── Consilient.Api/              # REST API server
├── Consilient.WebApp2/          # React frontend
├── Consilient.Data/             # Database context and entities
├── Consilient.Data.GraphQL/     # GraphQL schema
├── Consilient.*.Services/       # Domain services
└── Consilient.Infrastructure.*/ # Cross-cutting concerns

infra/
└── terraform/                   # Infrastructure as Code

docs/                            # Project documentation
```

## License

Proprietary - All rights reserved
