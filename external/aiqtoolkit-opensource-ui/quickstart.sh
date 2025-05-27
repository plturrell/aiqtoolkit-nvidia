#!/bin/bash

echo "🚀 AIQToolkit UI Quickstart"
echo "=========================="

# Check if node is installed
if ! command -v node &> /dev/null; then
    echo "❌ Node.js is not installed. Please install Node.js 16+ first."
    exit 1
fi

# Check if npm is installed
if ! command -v npm &> /dev/null; then
    echo "❌ npm is not installed. Please install npm first."
    exit 1
fi

echo "✅ Node.js and npm are installed"

# Install dependencies
echo "📦 Installing dependencies..."
npm install

# Create .env.local if it doesn't exist
if [ ! -f .env.local ]; then
    echo "🔧 Creating .env.local from template..."
    cp .env.example .env.local
    echo "⚠️  Please update .env.local with your backend URL configuration"
fi

# Build the project to check for TypeScript errors
echo "🔨 Building project (checking for TypeScript errors)..."
npm run build

if [ $? -eq 0 ]; then
    echo "✅ Build completed successfully"
else
    echo "❌ Build failed. Please fix TypeScript errors before continuing."
    exit 1
fi

# Start the development server
echo "🚀 Starting development server..."
echo "📱 Open http://localhost:3000 in your browser"
echo ""
echo "⚠️  Make sure your AIQToolkit backend is running on the configured port!"
echo ""

npm run dev