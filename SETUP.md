# Setup Guide

## Prerequisites
- .NET 8 SDK
- Node.js 18+ and npm
- Git

## Initial Setup

### 1. Clone the Repository
```bash
git clone https://github.com/your-username/auto-report-generator.git
cd auto-report-generator
```

### 2. Configure API Keys (Choose One Method)

#### Method A: User Secrets (Recommended for Development)
```bash
cd backend
dotnet user-secrets init
dotnet user-secrets set "AI:Groq:ApiKey" "your_groq_key_here"
dotnet user-secrets set "AI:HuggingFace:ApiKey" "your_hf_key_here"
dotnet user-secrets set "AI:TogetherAI:ApiKey" "your_together_key_here"
```

#### Method B: Environment Variables (Recommended for Production)
**Linux/Mac:**
```bash
export AI__Groq__ApiKey="your_groq_key_here"
export AI__HuggingFace__ApiKey="your_hf_key_here"
export AI__TogetherAI__ApiKey="your_together_key_here"
```

**Windows PowerShell:**
```powershell
$env:AI__Groq__ApiKey="your_groq_key_here"
$env:AI__HuggingFace__ApiKey="your_hf_key_here"
$env:AI__TogetherAI__ApiKey="your_together_key_here"
```

#### Method C: Local Config File (Not Committed)
Create `backend/appsettings.Local.json`:
```json
{
  "AI": {
    "Groq": {
      "ApiKey": "your_groq_key_here"
    },
    "HuggingFace": {
      "ApiKey": "your_hf_key_here"
    },
    "TogetherAI": {
      "ApiKey": "your_together_key_here"
    }
  }
}
```

### 3. Get Free API Keys

**Groq (Primary - Recommended):**
- Visit: https://console.groq.com/
- Sign up (free, no credit card)
- Create API key
- Limit: 14,400 requests/day

**Hugging Face (Backup 1):**
- Visit: https://huggingface.co/settings/tokens
- Sign up (free, no credit card)
- Create access token
- Free tier available

**Together AI (Backup 2):**
- Visit: https://api.together.xyz/
- Sign up (free tier available)
- Create API key

**Note:** System works without API keys using built-in heuristic analysis!

### 4. Install Dependencies

**Backend:**
```bash
cd backend
dotnet restore
```

**Frontend:**
```bash
cd frontend
npm install
```

### 5. Run the Application

**Backend (Terminal 1):**
```bash
cd backend
dotnet run
# Runs on http://localhost:5000
```

**Frontend (Terminal 2):**
```bash
cd frontend
npm run dev
# Runs on http://localhost:5173
```

### 6. Access the Application
Open your browser and navigate to: http://localhost:5173

## Database Setup

The application uses SQLite by default (no setup required). The database file `reports.db` is created automatically on first run.

### Switch to PostgreSQL (Optional)
1. Install PostgreSQL
2. Create database: `createdb autoreport`
3. Update `backend/appsettings.json`:
```json
{
  "UsePostgres": true,
  "ConnectionStrings": {
    "Postgres": "Host=localhost;Database=autoreport;Username=postgres;Password=yourpassword"
  }
}
```
4. Run migrations: `dotnet ef database update`

## Troubleshooting

### Backend won't start
- Ensure .NET 8 SDK is installed: `dotnet --version`
- Check port 5000 is not in use
- Verify API keys are configured (or leave empty for heuristic mode)

### Frontend won't start
- Ensure Node.js 18+ is installed: `node --version`
- Delete `node_modules` and run `npm install` again
- Check port 5173 is not in use

### AI not working
- Verify API keys are correctly set
- Check backend logs for error messages
- System automatically falls back to heuristic analysis if AI fails

### Database errors
- Delete `backend/reports.db*` files and restart backend
- Database will be recreated automatically

## Production Deployment

See [DEPLOYMENT.md](DEPLOYMENT.md) for production deployment instructions.

## Support

For issues or questions, please open an issue on GitHub.
