# Auto Report Generator

A fast, minimal web app for generating professional daily, weekly, and work log reports with GDPR compliance and automatic data retention.

## Features

### Core Functionality
- **Multiple Report Types**: Daily accomplishment, weekly summary, and work log reports
- **Multi-Language Support (i18n)**: 8 languages with RTL support for Arabic & Hebrew
- **Drag & Drop Interface**: Reorder tasks by dragging, drag files for text extraction
- **Universal Document Upload**: Extract text from PDF, DOCX, and images (JPG, PNG, BMP, TIFF, WEBP)
- **Smart Text Parsing**: Paste raw text and automatically extract structured data
- **AI-Powered Summaries**: Generate paragraph-style summary reports with charts
- **OCR Support**: Extract text from images using Tesseract OCR
- **Report Comparison**: Compare reports side-by-side with diff highlighting and progress tracking
- **Smart Suggestions**: AI-powered task suggestions and productivity insights based on history
- **Export Options**: Download reports as PDF or DOCX
- **Report History**: View and manage all generated reports
- **Edit & Regenerate**: Modify existing reports and regenerate them
- **Dark/Light Theme**: Toggle between themes with preference saved

### GDPR Compliance
- **Cookie Consent Banner**: GDPR-compliant cookie consent with accept/reject options
- **Privacy Policy**: Comprehensive privacy policy page
- **Cookie Policy**: Detailed cookie usage and management information
- **Automatic Data Deletion**: Reports automatically deleted after 2 days (configurable)
- **User Rights**: Full transparency about data collection and user rights
- **Data Minimization**: Only essential data is collected and stored

### Data Retention
- **Automatic Cleanup**: Background service deletes reports older than 2 days
- **Configurable Retention**: Customize retention period in `appsettings.json`
- **Scheduled Checks**: Runs cleanup every hour (configurable)
- **Logging**: Full audit trail of cleanup operations

## Stack
- **Backend**: ASP.NET Core 8 Web API (C#)
- **Frontend**: React + Vite
- **Database**: SQLite (local) / PostgreSQL (production)
- **ORM**: Entity Framework Core
- **AI Enhancement**: Multi-provider fallback system
  - **Primary**: Groq (Llama 3.1, 14,400 requests/day)
  - **Backup 1**: Hugging Face (Mistral, free tier)
  - **Backup 2**: Together AI (Mixtral, free tier)
  - **Fallback**: Heuristic analysis (always available, no API needed)
- **OCR**: Tesseract OCR Engine
- **PDF Text Extraction**: iText 7 with OCR fallback
- **PDF to Image**: PDFtoImage (PDFium)
- **Image Processing**: SkiaSharp
- **DOCX Text Extraction**: DocX (Xceed.Words.NET)
- **Drag & Drop**: @dnd-kit
- **Internationalization**: i18next, react-i18next
- **Charts**: Recharts
- **PDF Export**: QuestPDF
- **DOCX Export**: DocumentFormat.OpenXml

## Quick Start

### 1. Clone & Setup
```bash
git clone https://github.com/your-username/auto-report-generator.git
cd auto-report-generator
```

### 2. Configure API Keys (Optional)
See [SETUP.md](SETUP.md) for detailed instructions. Choose one method:

**User Secrets (Recommended for Development):**
```bash
cd backend
dotnet user-secrets set "AI:Groq:ApiKey" "your_key_here"
dotnet user-secrets set "AI:HuggingFace:ApiKey" "your_key_here"
dotnet user-secrets set "AI:TogetherAI:ApiKey" "your_key_here"
```

**Note:** System works without API keys using built-in heuristic analysis!

### 3. Run Backend
```bash
cd backend
dotnet restore
dotnet run
# API runs on http://localhost:5000
```

### 4. Run Frontend
```bash
cd frontend
npm install
npm run dev
# UI runs on http://localhost:5173
```

### 5. Access Application
Open http://localhost:5173 in your browser

For detailed setup instructions, see [SETUP.md](SETUP.md)  
For deployment instructions, see [DEPLOYMENT.md](DEPLOYMENT.md)

## Configuration

### Security Notice
⚠️ **NEVER commit API keys to version control!**

This repository uses empty API key placeholders in config files. Configure your keys using one of these methods:

1. **User Secrets** (Development): `dotnet user-secrets set "AI:Groq:ApiKey" "your_key"`
2. **Environment Variables** (Production): `export AI__Groq__ApiKey="your_key"`
3. **Local Config File** (Not committed): Create `appsettings.Local.json`

See [SETUP.md](SETUP.md) for detailed instructions.

### AI Enhancement (Optional but Recommended)

The system uses a **smart fallback chain** with multiple free AI providers. If one provider fails or hits rate limits, it automatically tries the next one. If all AI providers fail, it uses built-in heuristic analysis.

**Fallback Chain:**
```
User Request
    ↓
Try Groq (Primary - fastest, 14,400/day)
    ↓ (if fails/limit reached)
Try Hugging Face (Backup 1 - free tier)
    ↓ (if fails)
Try Together AI (Backup 2 - free tier)
    ↓ (if fails)
Use Heuristic Analysis (Always works)
    ↓
Return Enhanced Report
```

**Setup API Keys** in `backend/appsettings.json`:
```json
{
  "AI": {
    "Groq": {
      "ApiKey": "gsk_your_api_key_here"
    },
    "HuggingFace": {
      "ApiKey": "hf_your_token_here"
    },
    "TogetherAI": {
      "ApiKey": "your_api_key_here"
    }
  }
}
```

**All providers are FREE with no credit card required!**
- **Groq**: 14,400 requests/day (fastest, recommended) - Get key at https://console.groq.com/
- **Hugging Face**: Free tier with good models - Get token at https://huggingface.co/settings/tokens
- **Together AI**: Free tier available - Get key at https://api.together.xyz/

**Recommended Setup:**
- **Minimum**: Just add Groq (takes 2 minutes, covers most use cases)
- **Recommended**: Add Groq + Hugging Face (better reliability)
- **Maximum**: Add all three (24/7 AI availability guaranteed)
- **No Setup**: Leave empty - uses heuristic analysis (works great!)

See detailed setup instructions:
- [AI_SETUP.md](backend/AI_SETUP.md) - Complete setup guide for all providers
- [GROQ_SETUP_GUIDE.md](backend/GROQ_SETUP_GUIDE.md) - Step-by-step Groq setup with screenshots
- [API_KEY_CHEATSHEET.md](backend/API_KEY_CHEATSHEET.md) - Quick reference guide

**Note**: System works perfectly without any API keys using built-in heuristic analysis (sentiment analysis, opportunity detection, risk identification, smart recommendations).

### Data Retention Settings
In `backend/appsettings.json`:
```json
{
  "DataRetention": {
    "RetentionDays": 2,
    "CleanupIntervalHours": 1
  }
}
```

- **RetentionDays**: Number of days to keep reports (default: 2)
- **CleanupIntervalHours**: How often to check for old reports (default: 1)

### Switch to PostgreSQL
In `backend/appsettings.json`, set:
```json
{
  "UsePostgres": true,
  "ConnectionStrings": {
    "Postgres": "Host=localhost;Database=autoreport;Username=postgres;Password=yourpassword"
  }
}
```

## API Endpoints

### Reports
| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/generate-report` | Generate & save a report |
| GET | `/api/reports` | List all reports |
| GET | `/api/reports/{id}` | Get single report |
| PUT | `/api/reports/{id}` | Update existing report |
| DELETE | `/api/reports/{id}` | Delete a report |
| DELETE | `/api/reports` | Delete all reports |

### Export
| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/export/pdf/{id}` | Download report as PDF |
| POST | `/api/export/docx/{id}` | Download report as DOCX |
| POST | `/api/summary/export/pdf` | Download summary as PDF |
| POST | `/api/summary/export/docx` | Download summary as DOCX |

### Text Processing
| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/parse` | Parse raw text into structured data |
| POST | `/api/summary/generate` | Generate AI-powered summary report |
| POST | `/api/ocr/extract` | Extract text from image using OCR |

### Templates
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/templates` | List available templates |

## Project Structure

```
auto-report-generator/
├── backend/
│   ├── Controllers/       # API endpoints
│   ├── Services/          # Business logic & background services
│   ├── Models/            # Data models
│   ├── Data/              # Database context
│   ├── DTOs/              # Data transfer objects
│   ├── Repositories/      # Data access layer
│   ├── Migrations/        # EF Core migrations
│   └── tessdata/          # OCR language data
├── frontend/
│   └── src/
│       ├── components/    # Reusable UI components
│       ├── pages/         # Page components
│       └── services/      # API client
└── README.md
```

## Key Features Explained

### Multi-Language Support (i18n)
The application supports 8 languages with automatic detection and RTL support:
- **Languages**: English, Spanish, French, German, Chinese, Japanese, Arabic, Hebrew
- **Auto-Detection**: Automatically detects browser language on first visit
- **RTL Support**: Full right-to-left layout support for Arabic and Hebrew
- **Language Selector**: Easy language switching via dropdown in navbar
- **Persistent**: Language preference saved in localStorage

### Drag & Drop Interface
Intuitive drag and drop functionality throughout the application:
- **Reorder Tasks**: Drag tasks up or down to reorder them visually
- **Drag Documents**: Drag and drop PDF, DOCX, or images for text extraction
- **Visual Feedback**: Clear visual indicators when dragging
- **Touch Support**: Works on touch devices with proper activation constraints
- **Keyboard Accessible**: Full keyboard navigation support for accessibility

### Report Comparison
Compare reports side-by-side to track progress and changes:
- **Side-by-Side View**: View two reports simultaneously for easy comparison
- **Diff Highlighting**: Visual indicators for added, removed, and modified tasks
- **Progress Metrics**: Track completion rate changes, task count changes, and productivity trends
- **Change Summary**: See at a glance what changed between report versions
- **Time-Based Analysis**: Compare reports from different time periods to track progress
- **Selection Mode**: Easy selection of reports to compare from history page

### Smart Suggestions & AI Enhancements
Intelligent features powered by AI with automatic fallback chain:

**AI Provider Chain (Automatic Fallback):**
1. **Groq** (Primary) - Llama 3.1, 14,400 requests/day, 1-2s response
2. **Hugging Face** (Backup 1) - Mistral models, free tier, 3-5s response
3. **Together AI** (Backup 2) - Mixtral models, free tier, 2-4s response
4. **Heuristic Analysis** (Final Fallback) - Always works, instant, no API needed

If Groq hits rate limit or fails, system automatically tries Hugging Face. If that fails, tries Together AI. If all AI providers fail, uses built-in heuristic analysis. Users never see errors - always get insights!

**AI-Powered Features:**
- **Deep Insights**: Context-aware analysis of report content and patterns
- **Opportunity Detection**: Automatically identify growth opportunities and improvements
- **Risk Identification**: Flag potential risks, concerns, and challenges
- **Smart Recommendations**: Context-specific suggestions based on report type
- **Sentiment Analysis**: Detect positive, negative, or neutral tone
- **Priority Scoring**: Calculate importance scores for tasks and items
- **Trend Analysis**: Track productivity patterns and performance over time

**Traditional Features:**
- **Task Suggestions**: Auto-suggest frequently used tasks based on history
- **Productivity Insights**: View completion rates and average tasks per report
- **Pattern Recognition**: Identify most productive days and common categories
- **Smart Categorization**: Auto-categorize tasks (Development, Testing, Documentation, etc.)
- **One-Click Add**: Add suggested tasks with a single click
- **Keyword Extraction**: Discover common themes in your work

**Setup**: All AI providers are free with no credit card required. See [AI_SETUP.md](backend/AI_SETUP.md) for API key setup. Works perfectly without any API keys using heuristic analysis.

### AI Fallback System
The report generator includes a resilient multi-provider AI system that ensures reports are always generated with insights:

**How It Works:**
- System tries AI providers in order: Groq → Hugging Face → Together AI
- If a provider fails (rate limit, timeout, error), automatically tries next
- If all AI providers fail, uses built-in heuristic analysis
- Users never experience failures - always get enhanced reports
- Logs show which provider was used for transparency

**Benefits:**
- **Zero Downtime**: Always generates reports even if AI is unavailable
- **Cost Effective**: All providers are free with generous limits
- **Fast**: Primary provider (Groq) responds in 1-2 seconds
- **Reliable**: Multiple fallback layers ensure 24/7 availability
- **No Setup Required**: Works out of the box with heuristic analysis
- **Scalable**: Add more providers as needed for higher volume

**Example Fallback Flow:**
```
1. Try Groq → Rate limit reached (14,400/day exceeded)
2. Try Hugging Face → Success! Report enhanced in 3 seconds
3. User receives AI-powered insights without any error
```

Or if all AI fails:
```
1. Try Groq → API error
2. Try Hugging Face → Timeout
3. Try Together AI → Service unavailable
4. Use Heuristic Analysis → Success! Report enhanced instantly
5. User receives intelligent insights using built-in analysis
```

### Universal Document Upload
Extract text from multiple document formats:
- **PDF Files**: Extract text from PDF documents (including multi-page)
  - First attempts direct text extraction for PDFs with selectable text
  - Automatically falls back to OCR if PDF contains only images or scanned content
  - Handles certificates, scanned documents, and image-based PDFs
- **DOCX Files**: Extract text from Microsoft Word documents
- **Images**: OCR text extraction from JPG, PNG, BMP, TIFF, WEBP
- **Drag & Drop**: Simply drag files onto upload zones
- **Large File Support**: Handles files up to 10MB
- **Error Handling**: Clear error messages for unsupported or corrupted files

### Automatic Data Deletion
The `DataCleanupService` runs as a background service that:
1. Checks for reports older than the retention period every hour
2. Automatically deletes expired reports and their associated data
3. Logs all cleanup operations for audit purposes
4. Helps maintain GDPR compliance through data minimization

### Cookie Consent
GDPR-compliant cookie consent system that:
- Blocks interaction until user accepts or rejects cookies
- Provides detailed information about cookie types
- Links to Privacy Policy and Cookie Policy
- Stores user preference in localStorage
- Distinguishes between essential and non-essential cookies

### Smart Text Parsing
Two modes for report generation:
1. **Form Input**: Traditional form with fields for name, department, tasks, etc.
2. **Paste Text**: Paste raw notes and automatically extract structured data

### Summary Reports
Generate comprehensive summary reports with:
- Executive summary paragraph
- Activity breakdown by category
- Status distribution charts (pie chart)
- Activity count charts (bar chart)
- Completion rate metrics
- Formatted text output

## License
MIT

## Contributing
Pull requests are welcome. For major changes, please open an issue first.
