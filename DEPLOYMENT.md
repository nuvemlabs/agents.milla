# Deployment Guide

## Environment Variables Required

Set these environment variables in your Azure Web App configuration:

```
OPENAI_API_KEY=your_openai_api_key_here
DEMO_ACCESS_CODE=your_secret_demo_code_here
ASPNETCORE_ENVIRONMENT=Production
```

## Local Development

1. **Set environment variables:**
   ```bash
   export OPENAI_API_KEY="your-api-key"
   export DEMO_ACCESS_CODE="your-demo-code"
   ```

2. **Start backend:**
   ```bash
   dotnet run
   ```

3. **Start frontend (in separate terminal):**
   ```bash
   cd ClientApp
   npm start
   ```

4. **Access the application:**
   - Frontend: http://localhost:3000
   - Backend API: https://localhost:7236

## Azure Web App Deployment

### Prerequisites
- Azure Web App created (Free tier supported)
- Node.js 18.x runtime configured in Azure
- .NET 7.0 runtime configured in Azure

### Deployment Options

#### Option 1: GitHub Actions (Recommended)
1. Set `AZURE_WEBAPP_NAME` in `.github/workflows/build-and-deploy.yml`
2. Add `AZURE_WEBAPP_PUBLISH_PROFILE` secret to GitHub repository
3. Push to main branch to trigger deployment

#### Option 2: Manual Deployment
```bash
# Build React app
cd ClientApp
npm install
npm run build

# Publish .NET app
cd ..
dotnet publish -c Release -o ./publish

# Deploy publish folder to Azure Web App
```

### Configuration in Azure
1. **App Settings:** Add environment variables listed above
2. **Startup Command:** (leave empty, uses default)
3. **Always On:** Enable if not on Free tier
4. **HTTPS Only:** Enable (recommended)

## Testing the Deployment

1. Navigate to your Azure Web App URL
2. Enter the demo access code you configured
3. Try these test messages:
   - "Generate a proposal for ACME Corp, 250 seats, 2-year term"
   - "What discount rates are acceptable for enterprise deals?"
   - "Analyze the financial impact of a $500K deal"

## Troubleshooting

### Common Issues:
- **"Demo access not configured"**: Ensure `DEMO_ACCESS_CODE` environment variable is set
- **OpenAI API errors**: Verify `OPENAI_API_KEY` is valid and has sufficient quota
- **Build failures**: Ensure Node.js 18.x is configured in Azure
- **404 errors**: Verify the React app was built and included in deployment

### Logs:
- View logs in Azure Portal > App Service > Log stream
- Check Application Insights if configured