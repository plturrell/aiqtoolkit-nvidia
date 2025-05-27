#!/bin/bash

# Startup script for Digital Human with NVIDIA Blueprint

set -e

echo "Starting Digital Human with NVIDIA Blueprint Integration"
echo "========================================================="

# Set up environment
export NVIDIA_API_KEY="${NVIDIA_API_KEY:-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}"
export NVIDIA_BLUEPRINT_PATH="${NVIDIA_BLUEPRINT_PATH:-/blueprint}"
export PYTHONPATH="${NVIDIA_BLUEPRINT_PATH}:/app/src:${PYTHONPATH}"

# Log configuration
echo "Configuration:"
echo "  NVIDIA_API_KEY: ${NVIDIA_API_KEY:0:20}..."
echo "  BLUEPRINT_PATH: $NVIDIA_BLUEPRINT_PATH"
echo "  GPU_DEVICES: $CUDA_VISIBLE_DEVICES"

# Check if blueprint exists
if [ -d "$NVIDIA_BLUEPRINT_PATH" ]; then
    echo "✓ NVIDIA Blueprint found at $NVIDIA_BLUEPRINT_PATH"
    ls -la "$NVIDIA_BLUEPRINT_PATH"
else
    echo "⚠ Warning: NVIDIA Blueprint not found at $NVIDIA_BLUEPRINT_PATH"
fi

# Initialize database if needed
if [ "$INIT_DB" = "true" ]; then
    echo "Initializing database..."
    python -m aiq.digital_human.utils.init_db
fi

# Start services
echo "Starting services..."

# Start backend API
cd /app/src/aiq/digital_human
python -m uvicorn nvidia_blueprint.api:app \
    --host 0.0.0.0 \
    --port 8000 \
    --workers 4 \
    --log-level info &

# Start WebSocket server
python -m aiq.digital_human.ui.websocket.websocket_server \
    --host 0.0.0.0 \
    --port 8001 &

# Start monitoring endpoint
python -m aiq.digital_human.monitoring.health_server \
    --host 0.0.0.0 \
    --port 8002 &

echo "All services started"
echo "===================="
echo "API: http://localhost:8000"
echo "WebSocket: ws://localhost:8001"
echo "Health: http://localhost:8002/health"

# Keep container running
tail -f /dev/null