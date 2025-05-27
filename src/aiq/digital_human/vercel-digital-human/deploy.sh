#!/bin/bash

echo "🚀 Deploying Digital Human to Vercel"
echo "==================================="

# Check if Vercel CLI is installed
if ! command -v vercel &> /dev/null; then
    echo "Installing Vercel CLI..."
    npm i -g vercel
fi

# Install dependencies
echo "Installing dependencies..."
npm install

# Initialize git repository if needed
if [ ! -d ".git" ]; then
    echo "Initializing git repository..."
    git init
    git add .
    git commit -m "Initial commit: Digital Human Vercel deployment"
fi

# Deploy to Vercel
echo "Deploying to Vercel..."
vercel --prod

echo ""
echo "✅ Deployment complete!"
echo ""
echo "Next steps:"
echo "1. Set environment variables in Vercel dashboard:"
echo "   - NVIDIA_API_KEY"
echo "   - BREV_API_KEY" 
echo "   - BREV_API_ENDPOINT"
echo "   - LANGCHAIN_ENDPOINT"
echo ""
echo "2. Update your Brev environment to accept requests from Vercel"
echo "3. Test the deployment at your Vercel URL"