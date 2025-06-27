# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

This is a multi-agent AI Deal Desk Assistant built with:
- **Backend**: ASP.NET Core 7.0 Web API using AutoGen for .NET
- **Frontend**: React 18 SPA with Create React App
- **AI Framework**: Microsoft AutoGen with OpenAI integration
- **Architecture**: Multi-agent orchestration system for deal approval workflows

## Development Commands

### Backend (.NET)
```bash
# Build and run the API
dotnet build
dotnet run

# Run in development mode (with hot reload)
dotnet watch run
```

### Frontend (React)
```bash
# Navigate to ClientApp directory first
cd ClientApp

# Install dependencies
npm install

# Start development server
npm start

# Build for production
npm run build

# Run tests
npm test

# Lint code
npm run lint
```

### Full Stack Development
The backend serves both API endpoints and the React frontend in production. In development, run both servers:
1. Backend: `dotnet run` (serves on https://localhost:7014)
2. Frontend: `cd ClientApp && npm start` (serves on http://localhost:3000)

## Architecture

### Agent System
The core system uses AutoGen's multi-agent orchestration pattern:

1. **DealDeskOrchestrator** (`Services/DealDeskOrchestrator.cs`): Main orchestrator that coordinates agent workflow
2. **Agent Types**:
   - **SalesRepProxyAgent**: Handles initial deal requests
   - **PricingAgent**: Analyzes pricing, discounts, and margin calculations
   - **LegalAgent**: Reviews legal terms and risk assessment
   - **FinanceAgent**: Evaluates financial impact and ARR
   - **VPApprovalAgent**: Makes final approval decisions

### API Endpoints
- `POST /api/dealdesk/chat`: Main endpoint for deal processing (streaming responses)
- `GET /api/dealdesk/health`: Health check endpoint

### Data Flow
1. Deal request comes through React frontend
2. `DealDeskController` streams responses via Server-Sent Events
3. `DealDeskOrchestrator` coordinates agent workflow sequentially
4. Each agent analyzes the deal and provides recommendations
5. Final approval/rejection decision is made by VP agent

### Key Models
- `DealRequest`: Input model for deal processing
- `AgentResponse`: Standard response format from agents
- `DealStatus`: Tracks deal state throughout workflow
- `AgentData`: Contains agent-specific data (margin, risk scores, etc.)

## Configuration

### Required Environment Variables
- `OPENAI_API_KEY`: Required for AI agent functionality
- Alternative: Set in `appsettings.json` under `OpenAI:ApiKey`

### CORS Configuration
The API is configured to allow requests from React development server:
- `http://localhost:3000`
- `https://localhost:3000`

## Development Notes

### AutoGen Integration
- Uses AutoGen.OpenAI package for AI capabilities
- Agents are created with OpenAI GPT-4 model
- System includes simulated responses for demo purposes

### Frontend Integration
- React app uses Server-Sent Events for real-time streaming
- Bootstrap and Reactstrap for UI components
- Service worker enabled for PWA functionality

### Error Handling
- Comprehensive logging throughout the application
- Graceful error handling in agent workflow
- Client-side error states for network issues

## Testing
Currently uses React Testing Library for frontend tests. No backend tests are implemented yet.