# Copilot Instructions for CalcHub

## Project Overview

CalcHub is a full-stack Australian financial calculator platform built with .NET 8 (backend) and React (frontend), featuring clean architecture and modern design.

## Technology Stack

### Backend
- **.NET 8/9** with **ASP.NET Core**
- **Clean Architecture** pattern with three layers:
  - `CalcHub.Domain` - Domain entities, value objects, and enums
  - `CalcHub.Application` - Business logic services and use cases
  - `CalcHub.Api` - REST API controllers and DTOs
- **Swagger/OpenAPI** for API documentation

### Frontend
- **React 19** with **Vite** build tool
- **Tailwind CSS** for styling
- **ESLint** for linting

## Project Structure

```
CalcHub/
├── src/
│   ├── CalcHub.Api/          # ASP.NET Core Web API
│   │   ├── Controllers/      # API endpoints
│   │   ├── Models/           # Request/Response DTOs
│   │   └── Program.cs        # Application entry point
│   ├── CalcHub.Application/  # Business logic layer
│   │   └── Services/         # Calculator services
│   └── CalcHub.Domain/       # Domain models
│       └── CCS/              # Child Care Subsidy domain
├── calchub-frontend/         # React frontend
│   ├── src/
│   │   └── components/       # React components
│   └── public/
│       └── config.js         # Runtime API URL configuration
├── infra/                    # Azure infrastructure (Bicep)
│   ├── main.bicep            # Main infrastructure orchestration
│   ├── app/                  # App Service definitions
│   └── core/                 # Shared resources (monitoring)
├── azure.yaml                # Azure Developer CLI configuration
└── CalcHub.sln               # Solution file
```

## Development Guidelines

### Backend (.NET)

- Follow **Clean Architecture** principles - keep domain logic separate from infrastructure
- Use **dependency injection** for services (register in `DependencyInjection.cs`)
- Create **DTOs** in the `Models` folder for API request/response objects
- Use the `Money` value object for financial calculations
- Validate input using the `Validate()` method pattern on input classes
- Use **async/await** for I/O-bound operations
- Add **XML documentation comments** for public API endpoints

### Frontend (React)

- Use **functional components** with hooks
- Use **Tailwind CSS** utility classes for styling
- Keep components in the `components/` directory
- Use **lucide-react** for icons
- **API URL Configuration**: Use `window.ENV.VITE_API_URL` for runtime API URL (auto-detected in production)
- Never hardcode API URLs - always use the runtime configuration from `config.js`

### Code Style

- Use **PascalCase** for C# classes, methods, and properties
- Use **camelCase** for JavaScript/TypeScript variables and functions
- Use **kebab-case** for CSS classes and file names in frontend
- Add meaningful comments only when logic is complex
- Follow existing code patterns in the repository

## Build and Run

### Backend
```bash
cd src/CalcHub.Api
dotnet run
# API runs at: http://localhost:5144
```

### Frontend
```bash
cd calchub-frontend
npm install
npm run dev
# React app runs at: http://localhost:5173
```

### Build Solution
```bash
dotnet build CalcHub.sln
```

### Lint Frontend
```bash
cd calchub-frontend
npm run lint
```

## Adding New Calculators

When adding a new calculator:

1. **Domain Layer** (`CalcHub.Domain/`):
   - Create a new folder for the calculator domain
   - Add input and result classes
   - Add any required enums

2. **Application Layer** (`CalcHub.Application/`):
   - Create service interface in `Services/`
   - Implement the calculator service
   - Register the service in `DependencyInjection.cs`

3. **API Layer** (`CalcHub.Api/`):
   - Add request/response DTOs in `Models/`
   - Add endpoint in `CalculatorsController.cs`

4. **Frontend** (`calchub-frontend/`):
   - Create a new component in `src/components/`
   - Add routing if needed in `App.jsx`

## Testing

- Write unit tests for calculator services
- Test edge cases for financial calculations
- Validate input boundaries

## Currency and Locale

- All monetary values are in **Australian Dollars (AUD)**
- Use the `Money` class for all financial values
- Round monetary calculations to 2 decimal places

## Azure Deployment

### Deployment Method
- Use **Azure Developer CLI (azd)** for deployment
- Infrastructure is defined in **Bicep** templates in `/infra`
- Configuration in `azure.yaml` defines two services: `api` and `frontend`

### Infrastructure Components
- **Azure App Services** (Linux) - Hosts both frontend and backend
- **Application Insights** - Application monitoring and telemetry
- **Log Analytics Workspace** - Centralized logging
- **Managed Identity** - Secure service authentication

### Key Deployment Files
- `azure.yaml` - Defines services and hosting configuration
- `infra/main.bicep` - Main infrastructure orchestration
- `infra/app/api.bicep` - Backend API App Service configuration
- `infra/app/frontend.bicep` - Frontend App Service configuration
- `calchub-frontend/public/config.js` - Runtime API URL auto-detection

### Environment Configuration
- **Backend CORS**: Configured via `CORS_ORIGINS` environment variable
- **Frontend API URL**: Auto-detected at runtime based on hostname pattern (azweb → azapi)
- **Production Swagger**: Enabled for API testing in production

### Deployment Commands
```bash
azd up              # Provision infrastructure and deploy
azd deploy          # Deploy both services
azd deploy api      # Deploy only backend
azd deploy frontend # Deploy only frontend
```

### Important Notes
- Frontend uses runtime configuration (`config.js`) to detect API URL
- CORS is handled at application level (.NET), not at Azure App Service level
- Both services share a single App Service Plan (B1 Basic)
- All resources use unique naming with `uniqueString()` function
