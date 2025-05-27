#!/bin/bash
echo "🎮 Starting Digital Human MVP..."

# Check if Unity is in PATH
if ! command -v unity &> /dev/null; then
    echo "❌ Unity not found in PATH"
    echo "Please add Unity to your PATH or use Unity Hub"
    exit 1
fi

# Start backend services
echo "🚀 Starting backend services..."
cd /Users/apple/projects/AIQToolkit/src/aiq/digital_human
./start_unity_backend.sh &
BACKEND_PID=$!

# Wait for backend
sleep 5

# Open Unity project
echo "🎮 Opening Unity project..."
unity -projectPath .

# Cleanup on exit
trap "kill $BACKEND_PID" EXIT

echo "✅ MVP running. Press Ctrl+C to stop."
wait
