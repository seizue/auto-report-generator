# Deployment Guide: Vercel + Render

## Prerequisites
- GitHub account
- Vercel account (sign up at https://vercel.com)
- Render account (sign up at https://render.com)

---

## Step 1: Push to GitHub

```bash
# Make sure everything is committed
git status

# Push to GitHub (if not already done)
git push -u origin main
```

---

## Step 2: Deploy Backend to Render

### 2.1 Create Web Service
1. Go to https://dashboard.render.com
2. Click **"New +"** → **"Web Service"**
3. Click **"Connect GitHub"** and authorize Render
4. Select your repository: `auto-report-generator`

### 2.2 Configure Service
Fill in these settings:

- **Name**: `auto-report-backend` (or your choice)
- **Region**: Choose closest to you
- **Branch**: `main`
- **Root Directory**: `backend`
- **Runtime**: `.NET`
- **Build Command**: 
  ```
  dotnet publish -c Release -o out
  ```
- **Start Command**: 
  ```
  cd out && ./AutoReportGenerator
  ```

### 2.3 Add Environment Variables
Click **"Advanced"** → **"Add Environment Variable"**

Add these:

```
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://0.0.0.0:$PORT
UsePostgres=true

# AI Keys (optional - works without them)
AI__Groq__ApiKey=your_groq_key_here
AI__HuggingFace__ApiKey=your_hf_key_here
AI__TogetherAI__ApiKey=your_together_key_here
```

### 2.4 Add PostgreSQL Database
1. Scroll down to **"Add Database"**
2. Select **"PostgreSQL"**
3. Render will automatically create a database and set the connection string

### 2.5 Deploy
1. Click **"Create Web Service"**
2. Wait 5-10 minutes for deployment
3. Copy your backend URL (e.g., `https://auto-report-backend.onrender.com`)

---

## Step 3: Deploy Frontend to Vercel

### Option A: Using Vercel Dashboard (Easiest)

1. Go to https://vercel.com/new
2. Click **"Import Git Repository"**
3. Select your `auto-report-generator` repo
4. Configure:
   - **Framework Preset**: Vite
   - **Root Directory**: `frontend`
   - **Build Command**: `npm run build`
   - **Output Directory**: `dist`
5. Add Environment Variable:
   - **Name**: `VITE_API_URL`
   - **Value**: `https://your-backend.onrender.com/api` (use your Render URL)
6. Click **"Deploy"**
7. Wait 2-3 minutes
8. Your app is live! 🎉

### Option B: Using Vercel CLI

```bash
# Install Vercel CLI
npm i -g vercel

# Navigate to frontend
cd frontend

# Login to Vercel
vercel login

# Deploy
vercel

# Follow prompts:
# - Set up and deploy? Yes
# - Which scope? (select your account)
# - Link to existing project? No
# - Project name? auto-report-generator
# - Directory? ./
# - Override settings? No

# Add environment variable
vercel env add VITE_API_URL

# When prompted, enter: https://your-backend.onrender.com/api
# Select: Production

# Deploy to production
vercel --prod
```

---

## Step 4: Test Your Deployment

1. Open your Vercel URL (e.g., `https://auto-report-generator.vercel.app`)
2. Try generating a report
3. First request may take 30 seconds (Render cold start)
4. Subsequent requests will be fast!

---

## Step 5: Add Custom Domain (Optional)

### On Vercel:
1. Go to your project → Settings → Domains
2. Add your domain
3. Update DNS records as instructed

### On Render:
1. Go to your service → Settings → Custom Domain
2. Add your domain
3. Update DNS records as instructed

---

## Troubleshooting

### Backend not responding
- Check Render logs: Dashboard → Your Service → Logs
- Verify environment variables are set
- Make sure PostgreSQL is connected

### Frontend can't connect to backend
- Verify `VITE_API_URL` is set correctly in Vercel
- Check CORS is enabled in backend (already configured)
- Open browser console for errors

### Database errors
- Render automatically runs migrations on startup
- Check Render logs for migration errors
- Verify PostgreSQL connection string

---

## Costs

- **Vercel**: FREE (unlimited bandwidth, unlimited builds)
- **Render**: FREE (with cold starts after 15 min inactivity)
- **Total**: $0/month 🎉

---

## Updating Your App

```bash
# Make changes locally
git add .
git commit -m "Your changes"
git push

# Vercel and Render will automatically redeploy!
```

---

## Support

- Vercel Docs: https://vercel.com/docs
- Render Docs: https://render.com/docs
- Your app logs: Check respective dashboards
