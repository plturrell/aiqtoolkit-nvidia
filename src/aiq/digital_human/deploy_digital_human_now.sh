#!/bin/bash

# Deploy Digital Human with NVIDIA API
set -e

echo "🤖 Deploying Digital Human System"
echo "================================="

# Set environment
export NVIDIA_API_KEY="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"
export PYTHONPATH="/Users/apple/projects/AIQToolkit/src:$PYTHONPATH"

# Create necessary directories
mkdir -p logs

# Kill any existing processes
pkill -f "digital_human" || true
pkill -f "8000" || true
pkill -f "8080" || true

echo "Starting Digital Human services..."

# 1. Start the unified backend API
echo "Starting backend API..."
cd /Users/apple/projects/AIQToolkit/src/aiq/digital_human
python -m uvicorn ui.api.unified_backend:app --host 0.0.0.0 --port 8000 --reload > logs/backend.log 2>&1 &
BACKEND_PID=$!

# Wait for backend to start
sleep 5

# 2. Start the WebSocket server
echo "Starting WebSocket server..."
python ui/websocket/websocket_server.py > logs/websocket.log 2>&1 &
WS_PID=$!

# 3. Start the frontend
echo "Starting frontend UI..."
cd ui/frontend
python -m http.server 8080 > ../../logs/frontend.log 2>&1 &
FRONTEND_PID=$!

cd ../..

echo ""
echo "✅ Digital Human Deployed!"
echo "========================="
echo "Backend API: http://localhost:8000"
echo "Frontend UI: http://localhost:8080"
echo "WebSocket: ws://localhost:8765"
echo ""
echo "Access the Digital Human at:"
echo "http://localhost:8080/digital_human_interface.html"
echo ""
echo "Process IDs:"
echo "Backend: $BACKEND_PID"
echo "WebSocket: $WS_PID"
echo "Frontend: $FRONTEND_PID"
echo ""
echo "To stop: kill $BACKEND_PID $WS_PID $FRONTEND_PID"
echo ""
echo "Logs:"
echo "tail -f logs/backend.log"
echo "tail -f logs/websocket.log"
echo "tail -f logs/frontend.log"