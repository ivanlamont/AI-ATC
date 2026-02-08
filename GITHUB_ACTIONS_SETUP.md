# GitHub Actions CI/CD Setup for Azure Static Web Apps

## Overview
This guide explains how to set up automatic deployment to Azure Static Web Apps using GitHub Actions.

## What's Included
- GitHub Actions workflow file: `.github/workflows/deploy-azure-static-web-app.yml`
- Automatic deployment on push to `main` or `master` branch
- Pull request preview deployments
- Builds Blazor WebAssembly app and deploys to Azure

## Setup Steps

### 1. Add GitHub Repository Secret

You need to add your Azure Static Web Apps deployment token as a GitHub secret:

**Your Deployment Token:**
```
26c60759c133f65dbe888da20b0530827315bb05e91d3219550e6be6ac37912a02-0bb243df-c100-4527-9f5b-eaf373a9124f00f251206689200f
```

**Steps to add the secret:**

1. Go to your GitHub repository: `https://github.com/YOUR_USERNAME/AI-ATC`
2. Navigate to **Settings** → **Secrets and variables** → **Actions**
3. Click **New repository secret**
4. Name: `AZURE_STATIC_WEB_APPS_API_TOKEN`
5. Value: Paste the deployment token above
6. Click **Add secret**

### 2. Push to GitHub

Once the secret is configured, the workflow will activate on the next push:

```powershell
cd ~\source\repos\AI-ATC

# Add all files
git add .

# Commit
git commit -m "Add GitHub Actions CI/CD pipeline for Azure Static Web Apps deployment"

# Push to main branch
git push origin main
```

Or if using `master`:
```powershell
git push origin master
```

### 3. Monitor Deployment

1. Go to your GitHub repository
2. Click the **Actions** tab
3. You'll see the workflow running
4. Once complete, your app will be live at: `https://polite-glacier-06689200f.2.azurestaticapps.net`

## How It Works

**On Every Push to Main/Master:**
- Checks out your code
- Sets up .NET 10.0
- Restores NuGet packages
- Builds the Blazor WebAssembly app in Release mode
- Deploys the output to Azure Static Web Apps

**On Pull Requests:**
- Creates a staging deployment preview
- Allows you to test changes before merging
- Automatically cleans up when PR is closed

## Workflow File Details

The workflow is defined in `.github/workflows/deploy-azure-static-web-app.yml` and includes:

- **Triggers:** Push to `main`/`master`, Pull requests
- **Build:** .NET 10.0, Blazor WebAssembly compilation
- **Deploy:** Azure/static-web-apps-deploy@v1 action
- **Environment Variables:**
  - `DOTNET_VERSION`: 10.0.x
  - `APP_LOCATION`: src/AIATC.Web
  - `OUTPUT_LOCATION`: dist/wwwroot

## Troubleshooting

**If deployment fails:**

1. Check the workflow logs in GitHub Actions
2. Ensure the secret `AZURE_STATIC_WEB_APPS_API_TOKEN` is correctly set
3. Verify the token hasn't expired
4. Check that the app builds locally with: `dotnet publish -c Release -o dist`

**To refresh the token:**

```powershell
az staticwebapp secrets list --name aiatc-web --resource-group aiatc-rg
```

Then update the GitHub secret with the new token.

## Manual Deployment (Alternative)

If you want to deploy manually without pushing to GitHub, you can use:

```powershell
# Prepare files
powershell -ExecutionPolicy Bypass -File deploy-prepare-manual.ps1

# Then manually upload via Azure Portal, or use the curl command provided
```

## Next Steps

1. Add the GitHub secret (see Step 1 above)
2. Push your code to GitHub
3. Watch the Actions tab for the deployment to complete
4. Your site will be automatically deployed!

---

**App URL:** https://polite-glacier-06689200f.2.azurestaticapps.net

For more information about Azure Static Web Apps, see: https://docs.microsoft.com/azure/static-web-apps/
