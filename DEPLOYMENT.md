# Deployment Guide

## Deployment Options

### Option 1: Docker (Recommended)

#### Create Dockerfile for Backend
```dockerfile
# backend/Dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5000

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["AutoReportGenerator.csproj", "./"]
RUN dotnet restore
COPY . .
RUN dotnet build -c Release -o /app/build

FROM build AS publish
RUN dotnet publish -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
COPY tessdata ./tessdata
ENTRYPOINT ["dotnet", "AutoReportGenerator.dll"]
```

#### Create Dockerfile for Frontend
```dockerfile
# frontend/Dockerfile
FROM node:18-alpine AS build
WORKDIR /app
COPY package*.json ./
RUN npm ci
COPY . .
RUN npm run build

FROM nginx:alpine
COPY --from=build /app/dist /usr/share/nginx/html
COPY nginx.conf /etc/nginx/conf.d/default.conf
EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]
```

#### Create docker-compose.yml
```yaml
version: '3.8'

services:
  backend:
    build: ./backend
    ports:
      - "5000:5000"
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - AI__Groq__ApiKey=${GROQ_API_KEY}
      - AI__HuggingFace__ApiKey=${HF_API_KEY}
      - AI__TogetherAI__ApiKey=${TOGETHER_API_KEY}
      - UsePostgres=true
      - ConnectionStrings__Postgres=Host=db;Database=autoreport;Username=postgres;Password=${DB_PASSWORD}
    depends_on:
      - db
    volumes:
      - ./backend/reports.db:/app/reports.db

  frontend:
    build: ./frontend
    ports:
      - "80:80"
    depends_on:
      - backend

  db:
    image: postgres:15-alpine
    environment:
      - POSTGRES_DB=autoreport
      - POSTGRES_PASSWORD=${DB_PASSWORD}
    volumes:
      - postgres_data:/var/lib/postgresql/data

volumes:
  postgres_data:
```

#### Create .env file (not committed)
```bash
GROQ_API_KEY=your_groq_key
HF_API_KEY=your_hf_key
TOGETHER_API_KEY=your_together_key
DB_PASSWORD=your_secure_password
```

#### Deploy with Docker
```bash
docker-compose up -d
```

---

### Option 2: Azure App Service

#### Backend Deployment
```bash
# Login to Azure
az login

# Create resource group
az group create --name auto-report-rg --location eastus

# Create App Service plan
az appservice plan create --name auto-report-plan --resource-group auto-report-rg --sku B1 --is-linux

# Create web app
az webapp create --resource-group auto-report-rg --plan auto-report-plan --name auto-report-backend --runtime "DOTNET|8.0"

# Configure app settings
az webapp config appsettings set --resource-group auto-report-rg --name auto-report-backend --settings \
  AI__Groq__ApiKey="your_key" \
  AI__HuggingFace__ApiKey="your_key" \
  AI__TogetherAI__ApiKey="your_key"

# Deploy
cd backend
dotnet publish -c Release
cd bin/Release/net8.0/publish
zip -r deploy.zip .
az webapp deployment source config-zip --resource-group auto-report-rg --name auto-report-backend --src deploy.zip
```

#### Frontend Deployment (Azure Static Web Apps)
```bash
# Build frontend
cd frontend
npm run build

# Deploy to Azure Static Web Apps
az staticwebapp create --name auto-report-frontend --resource-group auto-report-rg --source ./dist --location eastus
```

---

### Option 3: AWS (Elastic Beanstalk + S3)

#### Backend (Elastic Beanstalk)
```bash
# Install EB CLI
pip install awsebcli

# Initialize
cd backend
eb init -p "64bit Amazon Linux 2 v2.6.0 running .NET Core" auto-report-backend

# Create environment
eb create auto-report-env

# Set environment variables
eb setenv AI__Groq__ApiKey="your_key" AI__HuggingFace__ApiKey="your_key" AI__TogetherAI__ApiKey="your_key"

# Deploy
eb deploy
```

#### Frontend (S3 + CloudFront)
```bash
# Build
cd frontend
npm run build

# Create S3 bucket
aws s3 mb s3://auto-report-frontend

# Enable static website hosting
aws s3 website s3://auto-report-frontend --index-document index.html

# Upload files
aws s3 sync dist/ s3://auto-report-frontend --acl public-read

# Create CloudFront distribution (optional, for HTTPS)
aws cloudfront create-distribution --origin-domain-name auto-report-frontend.s3.amazonaws.com
```

---

### Option 4: Heroku

#### Backend
```bash
# Login
heroku login

# Create app
heroku create auto-report-backend

# Set environment variables
heroku config:set AI__Groq__ApiKey="your_key" -a auto-report-backend
heroku config:set AI__HuggingFace__ApiKey="your_key" -a auto-report-backend
heroku config:set AI__TogetherAI__ApiKey="your_key" -a auto-report-backend

# Add buildpack
heroku buildpacks:set heroku/dotnet -a auto-report-backend

# Deploy
git push heroku main
```

#### Frontend (Netlify)
```bash
# Install Netlify CLI
npm install -g netlify-cli

# Build
cd frontend
npm run build

# Deploy
netlify deploy --prod --dir=dist
```

---

### Option 5: VPS (DigitalOcean, Linode, etc.)

#### Setup Server
```bash
# SSH into server
ssh root@your-server-ip

# Install .NET 8
wget https://dot.net/v1/dotnet-install.sh
chmod +x dotnet-install.sh
./dotnet-install.sh --channel 8.0

# Install Node.js
curl -fsSL https://deb.nodesource.com/setup_18.x | bash -
apt-get install -y nodejs

# Install Nginx
apt-get install -y nginx

# Install PostgreSQL (optional)
apt-get install -y postgresql postgresql-contrib
```

#### Deploy Backend
```bash
# Clone repo
git clone https://github.com/your-username/auto-report-generator.git
cd auto-report-generator/backend

# Set environment variables
export AI__Groq__ApiKey="your_key"
export AI__HuggingFace__ApiKey="your_key"
export AI__TogetherAI__ApiKey="your_key"

# Build and run
dotnet publish -c Release
cd bin/Release/net8.0/publish
dotnet AutoReportGenerator.dll
```

#### Deploy Frontend
```bash
cd ../frontend
npm install
npm run build

# Copy to Nginx
cp -r dist/* /var/www/html/
```

#### Configure Nginx
```nginx
# /etc/nginx/sites-available/auto-report
server {
    listen 80;
    server_name your-domain.com;

    location / {
        root /var/www/html;
        try_files $uri $uri/ /index.html;
    }

    location /api {
        proxy_pass http://localhost:5000;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection keep-alive;
        proxy_set_header Host $host;
        proxy_cache_bypass $http_upgrade;
    }
}
```

```bash
# Enable site
ln -s /etc/nginx/sites-available/auto-report /etc/nginx/sites-enabled/
nginx -t
systemctl restart nginx
```

#### Setup SSL (Let's Encrypt)
```bash
apt-get install -y certbot python3-certbot-nginx
certbot --nginx -d your-domain.com
```

---

## Environment Variables Reference

### Required
- `AI__Groq__ApiKey` - Groq API key (or leave empty for heuristic mode)
- `AI__HuggingFace__ApiKey` - Hugging Face token
- `AI__TogetherAI__ApiKey` - Together AI key

### Optional
- `UsePostgres` - Set to `true` to use PostgreSQL instead of SQLite
- `ConnectionStrings__Postgres` - PostgreSQL connection string
- `DataRetention__RetentionDays` - Days to keep reports (default: 2)
- `DataRetention__CleanupIntervalHours` - Cleanup frequency (default: 1)

---

## Security Checklist

- [ ] API keys stored in environment variables or secrets manager
- [ ] HTTPS enabled (SSL certificate)
- [ ] CORS configured for production domain
- [ ] Database credentials secured
- [ ] Firewall configured (only ports 80, 443 open)
- [ ] Regular backups enabled
- [ ] Monitoring and logging configured
- [ ] Rate limiting enabled
- [ ] Security headers configured

---

## Monitoring

### Application Insights (Azure)
```bash
dotnet add package Microsoft.ApplicationInsights.AspNetCore
```

### CloudWatch (AWS)
```bash
dotnet add package AWS.Logger.AspNetCore
```

### Custom Logging
Check logs in:
- Docker: `docker logs <container-id>`
- Azure: Application Insights
- AWS: CloudWatch
- VPS: `/var/log/nginx/` and application logs

---

## Backup Strategy

### Database Backup (PostgreSQL)
```bash
# Backup
pg_dump -U postgres autoreport > backup.sql

# Restore
psql -U postgres autoreport < backup.sql
```

### SQLite Backup
```bash
# Backup
cp backend/reports.db backup/reports-$(date +%Y%m%d).db

# Restore
cp backup/reports-20240101.db backend/reports.db
```

---

## Scaling Considerations

- Use PostgreSQL for production (better concurrency)
- Enable caching (Redis) for API responses
- Use CDN for frontend assets
- Implement rate limiting
- Add load balancer for multiple backend instances
- Use managed database service (AWS RDS, Azure SQL)
- Implement queue system for long-running tasks

---

## Cost Estimates

### Free Tier Options
- **Heroku**: Free tier available (limited hours)
- **Netlify**: Free tier for frontend
- **Vercel**: Free tier for frontend
- **Railway**: Free tier with limits

### Paid Options
- **DigitalOcean**: $5-10/month (VPS)
- **Azure**: $10-50/month (App Service)
- **AWS**: $10-50/month (Elastic Beanstalk)
- **Docker on VPS**: $5-20/month

### AI Costs
- **Groq**: FREE (14,400 requests/day)
- **Hugging Face**: FREE (with rate limits)
- **Together AI**: FREE tier available
- **Fallback**: Heuristic analysis (always free)

---

## Support

For deployment issues, check:
1. Application logs
2. Server logs
3. Database connectivity
4. API key configuration
5. CORS settings
6. Firewall rules

Open an issue on GitHub if you need help!
