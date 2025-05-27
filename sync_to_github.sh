#!/bin/bash

# AIQToolkit GitHub Sync Script
# Run this after creating a new GitHub repository

echo "🚀 AIQToolkit GitHub Sync Script"
echo "================================="

# Check if repository URL is provided
if [ -z "$1" ]; then
    echo "❌ Error: Please provide your GitHub repository URL"
    echo "Usage: ./sync_to_github.sh https://github.com/YOUR_USERNAME/YOUR_REPO_NAME.git"
    echo ""
    echo "📝 Steps to create repository:"
    echo "1. Go to https://github.com/new"
    echo "2. Name your repository: 'aiqtoolkit-nvidia'"
    echo "3. Make it Public"
    echo "4. Don't initialize with README"
    echo "5. Copy the repository URL and run this script"
    exit 1
fi

REPO_URL=$1

echo "📦 Repository URL: $REPO_URL"
echo ""

# Remove existing origin if it exists
echo "🔧 Removing existing remote..."
git remote remove origin 2>/dev/null || true

# Add new remote
echo "🔗 Adding new remote repository..."
git remote add origin "$REPO_URL"

# Verify we have commits to push
COMMIT_COUNT=$(git rev-list --count HEAD)
echo "📊 Found $COMMIT_COUNT commits to sync"

if [ "$COMMIT_COUNT" -eq "0" ]; then
    echo "❌ No commits found. Please ensure you're in the correct directory."
    exit 1
fi

# Show latest commits
echo ""
echo "📝 Latest commits to be synced:"
git log --oneline -5

echo ""
read -p "🤔 Proceed with sync? (y/n): " -n 1 -r
echo ""

if [[ $REPLY =~ ^[Yy]$ ]]; then
    echo "🚀 Syncing to GitHub..."
    
    # Push to remote
    if git push -u origin main; then
        echo ""
        echo "✅ Successfully synced to GitHub!"
        echo ""
        echo "🎉 Next steps:"
        echo "1. Visit your repository: ${REPO_URL%%.git}"
        echo "2. Deploy to Vercel:"
        echo "   - Go to https://vercel.com/new"
        echo "   - Import your GitHub repository"
        echo "   - Set root directory: external/aiqtoolkit-opensource-ui"
        echo "   - Framework: Next.js"
        echo "   - Deploy!"
        echo ""
        echo "📚 Documentation: See DEPLOYMENT_README.md for detailed instructions"
    else
        echo ""
        echo "❌ Sync failed. Please check:"
        echo "1. Repository exists and you have write access"
        echo "2. Repository URL is correct"
        echo "3. You're authenticated with GitHub"
    fi
else
    echo "🚫 Sync cancelled."
fi