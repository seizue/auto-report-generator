# Deploy Frontend to Vercel

## Prerequisites
- GitHub account with the repository pushed
- Vercel account (sign up at https://vercel.com)

## Deployment Steps

### 1. Connect to Vercel

1. Go to https://vercel.com and sign in
2. Click **"Add New..."** → **"Project"**
3. Click **"Import Git Repository"**
4. Select your GitHub repository: `seizue/auto-report-generator`
5. If not listed, click **"Adjust GitHub App Permissions"** to grant access

### 2. Configure Project

When configuring the project:

**Framework Preset:** Vite

**Root Directory:** `frontend` (click "Edit" and enter `frontend`)

**Build Command:** `npm run build` (should be auto-detected)

**Output Directory:** `dist` (should be auto-detected)

**Install Command:** `npm install` (should be auto-detected)

### 3. Add Environment Variable

Before deploying, add the environment variable:

1. Expand **"Environment Variables"** section
2. Add:
   - **Key**: `VITE_API_URL`
   - **Value**: `https://auto-report-generator-5mb0.onrender.com/api`
   - **Environment**: Production (checked)

### 4. Deploy

1. Click **"Deploy"**
2. Wait 2-3 minutes for the build to complete
3. Once deployed, you'll get a URL like: `https://your-project-name.vercel.app`

### 5. Test the Deployment

1. Open your Vercel URL
2. Try creating a report to verify the backend connection works
3. Check that all features work (OCR, export, etc.)

## Custom Domain (Optional)

To add a custom domain:

1. Go to your project in Vercel
2. Click **"Settings"** → **"Domains"**
3. Add your domain and follow the DNS configuration instructions

## Troubleshooting

### Build Fails
- Check the build logs in Vercel dashboard
- Ensure `frontend` is set as the root directory
- Verify all dependencies are in `package.json`

### API Not Working
- Verify `VITE_API_URL` is set correctly
- Check that your Render backend is running
- Open browser console to see API errors

### 404 Errors on Refresh
- Ensure `vercel.json` exists in the frontend folder
- The rewrite rule should handle client-side routing

## Environment Variables Reference

- `VITE_API_URL`: Your backend API URL (Render URL + `/api`)
  - Example: `https://auto-report-generator-5mb0.onrender.com/api`

## Post-Deployment

After successful deployment:

1. Update your README with the live URLs
2. Test all features thoroughly
3. Monitor Vercel analytics for usage
4. Set up custom domain if needed

## Free Tier Limits

Vercel Free Tier includes:
- Unlimited bandwidth
- 100 GB-hours of build time per month
- Automatic HTTPS
- Global CDN
- Instant rollbacks

Your app should stay well within these limits!
