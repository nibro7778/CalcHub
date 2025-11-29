# CalcHub - Australian Financial Calculators

A full-stack financial calculator platform built with .NET Core and React, featuring clean architecture and modern design.

## 🌐 Live Demo

- **Frontend**: [https://azwebeq4iryown4o7m.azurewebsites.net](https://azwebeq4iryown4o7m.azurewebsites.net)
- **API (Swagger)**: [https://azapieq4iryown4o7m.azurewebsites.net](https://azapieq4iryown4o7m.azurewebsites.net)

## 🚀 Quick Start

### Prerequisites
- [.NET 8.0 SDK](https://dotnet.microsoft.com/download)
- [Node.js 18+](https://nodejs.org/)
- [Azure Developer CLI (azd)](https://learn.microsoft.com/azure/developer/azure-developer-cli/install-azd) - for Azure deployment

### Run Backend API
```bash
cd src/CalcHub.Api
dotnet run
```
API runs at: http://localhost:5144

### Run Frontend
```bash
cd calchub-frontend
npm install
npm run dev
```
React app runs at: http://localhost:5173

## ☁️ Deploy to Azure

This project is configured for deployment to Azure using Azure Developer CLI (azd).

### One-Command Deployment
```bash
# Login to Azure
azd auth login

# Provision infrastructure and deploy
azd up
```

### What Gets Deployed
- **2x Azure App Services** (Frontend + API)
- **Application Insights** for monitoring
- **Log Analytics Workspace** for centralized logging
- **Managed Identity** for secure Azure service authentication

### Infrastructure as Code
All Azure resources are defined using Bicep templates in the `/infra` folder:
- `main.bicep` - Main infrastructure orchestration
- `app/api.bicep` - Backend API App Service
- `app/frontend.bicep` - Frontend App Service
- `core/monitoring/` - Application Insights & Log Analytics

### Deployment Commands
```bash
# Deploy both services
azd deploy

# Deploy only API
azd deploy api

# Deploy only frontend
azd deploy frontend

# View application logs
azd monitor --logs

# Tear down all resources
azd down
```

## 📊 Current Calculators

- **Child Care Subsidy (CCS)** - Calculate Australian Government CCS based on income and activity level (2024-25 rates)

## 🛠️ Tech Stack

**Backend:** 
- .NET 8.0
- ASP.NET Core Web API
- Clean Architecture (Domain, Application, API layers)
- Swagger/OpenAPI

**Frontend:** 
- React 19
- Vite 7
- Tailwind CSS 3
- Lucide Icons

**Infrastructure:**
- Azure App Service (Linux)
- Application Insights
- Bicep (Infrastructure as Code)
- Azure Developer CLI

## 📁 Project Structure

```
CalcHub/
├── src/
│   ├── CalcHub.Api/              # Web API layer
│   ├── CalcHub.Application/      # Business logic layer
│   └── CalcHub.Domain/           # Domain models
├── calchub-frontend/             # React frontend
│   ├── src/
│   │   ├── components/           # React components
│   │   └── assets/               # Static assets
│   └── public/
│       └── config.js             # Runtime configuration
├── infra/                        # Azure infrastructure (Bicep)
│   ├── main.bicep
│   ├── app/                      # App Service definitions
│   └── core/                     # Shared resources
└── azure.yaml                    # Azure Developer CLI config
```

## 🔧 Configuration

### Frontend Environment Variables
- **Development**: Uses `http://localhost:5144` (auto-detected)
- **Production**: Auto-detects API URL based on Azure hostname
- Configuration is handled at runtime via `/config.js`

### Backend Environment Variables
- `ASPNETCORE_ENVIRONMENT` - Environment name (Development/Production)
- `CORS_ORIGINS` - Allowed frontend origins (comma-separated)
- `APPLICATIONINSIGHTS_CONNECTION_STRING` - Application Insights connection

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/calculator-name`)
3. Commit changes (`git commit -m 'Add stamp duty calculator'`)
4. Push to branch (`git push origin feature/calculator-name`)
5. Open a Pull Request

## 📝 License

This project is open source and available under the MIT License.

## 📧 Contact

**Developer**: Calculator Tools  
**Email**: mailme7778@gmail.com