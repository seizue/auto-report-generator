# Auto Report Generator

AI-powered report generator that transforms raw text into professional reports with charts, insights, and multi-format exports.

## Live Demo

**Try it now:** [https://auto-report-generator-rouge.vercel.app/](https://auto-report-generator-rouge.vercel.app/)

- **Frontend**: Deployed on Vercel
- **Backend**: Deployed on Render
- **Database**: PostgreSQL on Render

> Note: First request may take 30-60 seconds as the free tier backend spins up from sleep.

## Features

- **18+ Report Types**: Financial, Incident, Meeting, Academic, Strategy, and more
- **AI Enhancement**: Multi-provider fallback (Groq → Hugging Face → Together AI → Heuristic)
- **Smart Text Parsing**: Paste raw notes, get structured reports with charts
- **Document Upload**: Extract text from PDF, DOCX, images (with OCR)
- **Multi-Language**: 8 languages with RTL support
- **Export**: PDF and DOCX with intelligent formatting
- **GDPR Compliant**: Cookie consent, auto-deletion after 2 days
- **Report History**: Compare, edit, and regenerate reports

## Tech Stack

- **Backend**: ASP.NET Core 8, Entity Framework Core, PostgreSQL
- **Frontend**: React + Vite, Recharts
- **AI**: Groq (Llama 3.1), Hugging Face (Mistral), Together AI (Mixtral)
- **OCR**: Tesseract, iText 7, PDFtoImage
- **Export**: QuestPDF, DocumentFormat.OpenXml


## Local Development

### Prerequisites
- .NET 8 SDK
- Node.js 18+

### 1. Clone Repository
```bash
git clone https://github.com/seizue/auto-report-generator.git
cd auto-report-generator
```

### 2. Run Backend
```bash
cd backend
dotnet restore
dotnet run
# Runs on http://localhost:5000
```

### 3. Run Frontend
```bash
cd frontend
npm install
npm run dev
# Runs on http://localhost:5173
```

### 4. Open Application
Navigate to http://localhost:5173

## AI Configuration (Optional)

The system works without API keys using heuristic analysis. For AI enhancement, configure one or more providers:

**User Secrets (Development):**
```bash
cd backend
dotnet user-secrets init
dotnet user-secrets set "AI:Groq:ApiKey" "your_key"
dotnet user-secrets set "AI:HuggingFace:ApiKey" "your_key"
dotnet user-secrets set "AI:TogetherAI:ApiKey" "your_key"
```

**Environment Variables (Production):**
```bash
AI__Groq__ApiKey=your_key
AI__HuggingFace__ApiKey=your_key
AI__TogetherAI__ApiKey=your_key
```

**Get Free API Keys:**
- **Groq**: https://console.groq.com/ (14,400 requests/day)
- **Hugging Face**: https://huggingface.co/settings/tokens
- **Together AI**: https://api.together.xyz/

**Fallback Chain**: Groq → Hugging Face → Together AI → Heuristic (always works)

## Configuration

### Database
- **Development**: SQLite (auto-created)
- **Production**: PostgreSQL (recommended for Render/cloud deployment)

Configure in `backend/appsettings.json`:
```json
{
  "UsePostgres": false,  // true for production
  "ConnectionStrings": {
    "Sqlite": "Data Source=reports.db",
    "Postgres": "Host=localhost;Database=autoreport;Username=postgres;Password=yourpassword"
  }
}
```

### Data Retention
Configure in `backend/appsettings.json`:
```json
{
  "DataRetention": {
    "RetentionDays": 2,
    "CleanupIntervalHours": 1
  }
}
```

## API Endpoints

### Reports
- `POST /api/generate-report` - Generate & save report
- `GET /api/reports` - List all reports
- `GET /api/reports/{id}` - Get single report
- `PUT /api/reports/{id}` - Update report
- `DELETE /api/reports/{id}` - Delete report

### Export
- `POST /api/export/pdf/{id}` - Export as PDF
- `POST /api/export/docx/{id}` - Export as DOCX
- `POST /api/summary/export/pdf` - Export summary as PDF
- `POST /api/summary/export/docx` - Export summary as DOCX

### Processing
- `POST /api/parse` - Parse raw text
- `POST /api/summary/generate` - Generate AI summary
- `POST /api/ocr/extract` - Extract text from image

## Project Structure

```
auto-report-generator/
├── backend/
│   ├── Controllers/       # API endpoints
│   ├── Services/          # Business logic
│   ├── Models/            # Data models
│   ├── Data/              # Database context
│   ├── Migrations/        # EF Core migrations
│   └── Dockerfile         # Docker configuration
└── frontend/
    ├── src/
    │   ├── components/    # UI components
    │   ├── pages/         # Page components
    │   └── services/      # API client
    └── vercel.json        # Vercel configuration
```

## Free Tier Limits

- **Vercel**: Unlimited bandwidth, 100 GB-hours build time/month
- **Render**: 750 hours/month (enough for 24/7), 512 MB RAM
- **PostgreSQL**: 1 GB storage, 97 hours/month (enough for development)

## License

MIT

## Contributing

Pull requests welcome. For major changes, open an issue first.
