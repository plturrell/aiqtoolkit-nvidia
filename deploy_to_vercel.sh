#!/bin/bash

echo "🚀 Deploying AIQToolkit NVIDIA UI to Vercel"
echo "=========================================="

# Your GitHub username
GITHUB_USER="plturrell"

# Repository name (you can change this)
REPO_NAME="aiqtoolkit-nvidia-ui"

echo "1. First, create a new GitHub repository:"
echo "   Go to: https://github.com/new"
echo "   Repository name: $REPO_NAME"
echo "   Make it public or private as you prefer"
echo ""
echo "Press Enter when you've created the repository..."
read

# Set up the GitHub remote
echo "2. Setting up GitHub remote..."
cd /Users/apple/projects/AIQToolkit/web-ui

# Initialize git if not already
git init

# Add files
git add .
git commit -m "Initial commit: AIQToolkit NVIDIA UI"

# Add GitHub remote
git remote add origin https://github.com/$GITHUB_USER/$REPO_NAME.git

echo ""
echo "3. Now push to GitHub:"
echo "   Run: git push -u origin main"
echo ""
echo "   If it asks for credentials, use:"
echo "   Username: $GITHUB_USER"
echo "   Password: Your GitHub personal access token"
echo ""
echo "Press Enter after pushing to GitHub..."
read

echo ""
echo "4. Deploy to Vercel:"
echo "   Go to: https://vercel.com/new"
echo "   Click 'Import from Git'"
echo "   Select your GitHub account and the '$REPO_NAME' repository"
echo ""
echo "5. Configure the deployment:"
echo "   - Framework Preset: Other"
echo "   - Root Directory: ./"
echo "   - Build Command: (leave empty)"
echo "   - Output Directory: ./"
echo ""
echo "6. Add Environment Variables:"
echo "   NVIDIA_ENDPOINT = https://jupyter0-s1ondnjfx.brevlab.com"
echo ""
echo "7. Click 'Deploy'"
echo ""
echo "Your app will be live at: https://$REPO_NAME.vercel.app"