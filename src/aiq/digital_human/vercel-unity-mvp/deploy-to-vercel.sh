#!/bin/bash
# Deploy Unity Digital Human MVP to Vercel

echo "🚀 Deploying Unity Digital Human MVP to Vercel"
echo "==========================================="

# Check if vercel CLI is installed
if ! command -v vercel &> /dev/null; then
    echo "Installing Vercel CLI..."
    npm install -g vercel
fi

# Check for Unity build files
UNITY_BUILD_DIR="public/unity"
if [ ! -f "$UNITY_BUILD_DIR/DigitalHumanMVP.loader.js" ]; then
    echo "⚠️  Unity build files not found in $UNITY_BUILD_DIR"
    echo "Please build your Unity project and copy the WebGL build files to:"
    echo "  - $UNITY_BUILD_DIR/DigitalHumanMVP.loader.js"
    echo "  - $UNITY_BUILD_DIR/DigitalHumanMVP.data"
    echo "  - $UNITY_BUILD_DIR/DigitalHumanMVP.framework.js"
    echo "  - $UNITY_BUILD_DIR/DigitalHumanMVP.wasm"
    
    # Create placeholder files for initial deployment
    echo "Creating placeholder files for initial deployment..."
    mkdir -p $UNITY_BUILD_DIR
    echo "// Placeholder - Replace with actual Unity build" > "$UNITY_BUILD_DIR/DigitalHumanMVP.loader.js"
    echo "// Placeholder - Replace with actual Unity build" > "$UNITY_BUILD_DIR/DigitalHumanMVP.framework.js"
    touch "$UNITY_BUILD_DIR/DigitalHumanMVP.data"
    touch "$UNITY_BUILD_DIR/DigitalHumanMVP.wasm"
fi

# Install dependencies
echo "📦 Installing dependencies..."
npm install

# Build the project
echo "🔨 Building project..."
npm run build

# Set up environment variables
echo "🔐 Setting up environment variables..."
cat > .env.production << EOF
# Production environment variables
# These will be configured in Vercel dashboard
BACKEND_URL=\${BACKEND_URL}
BREV_API_KEY=\${BREV_API_KEY}
BREV_SHELL_ID=\${BREV_SHELL_ID}
LANGCHAIN_API_KEY=\${LANGCHAIN_API_KEY}
OPENAI_API_KEY=\${OPENAI_API_KEY}
NEXT_PUBLIC_WS_URL=\${NEXT_PUBLIC_WS_URL}
NEXT_PUBLIC_API_URL=\${NEXT_PUBLIC_API_URL}
EOF

# Deploy to Vercel
echo "🚀 Deploying to Vercel..."
vercel --prod

# Get deployment URL
DEPLOYMENT_URL=$(vercel ls --json | jq -r '.[0].url')

echo ""
echo "✅ Deployment Complete!"
echo "===================="
echo "URL: https://$DEPLOYMENT_URL"
echo ""
echo "Next Steps:"
echo "1. Configure environment variables in Vercel dashboard"
echo "2. Upload Unity WebGL build files to $UNITY_BUILD_DIR"
echo "3. Set up custom domain (optional)"
echo "4. Configure Brev integration"
echo ""
echo "Environment Variables to Set:"
echo "- BACKEND_URL: Your backend server URL"
echo "- BREV_API_KEY: Your Brev API key"
echo "- BREV_SHELL_ID: langchain-structured-report-generation-6d35aa"
echo "- LANGCHAIN_API_KEY: Your LangChain API key"
echo "- OPENAI_API_KEY: Your OpenAI API key"
echo "- NEXT_PUBLIC_WS_URL: WebSocket server URL"
echo "- NEXT_PUBLIC_API_URL: Public API URL"
echo ""