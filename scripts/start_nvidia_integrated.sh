#!/bin/bash

# Start AIQToolkit with NVIDIA report generator integration

echo "🚀 Starting AIQToolkit with NVIDIA Report Generator Integration"
echo "============================================================"

# Colors
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m'

# Base directory
BASE_DIR="$(cd "$(dirname "$0")/.." && pwd)"

# Kill any existing processes
echo -e "${YELLOW}Cleaning up existing processes...${NC}"
lsof -ti:8000 | xargs kill -9 2>/dev/null || true
lsof -ti:3000 | xargs kill -9 2>/dev/null || true

# Start the backend with NVIDIA integration
echo -e "${GREEN}Starting AIQToolkit backend...${NC}"
cd "$BASE_DIR"
source .venv/bin/activate
python scripts/aiq_backend_server.py > logs/backend.log 2>&1 &
BACKEND_PID=$!

sleep 3

# Start the web UI
echo -e "${GREEN}Starting web UI...${NC}"
cd "$BASE_DIR/web-ui"
python -m http.server 3000 > ../logs/web-ui.log 2>&1 &
UI_PID=$!

sleep 2

echo -e "\n${GREEN}✓ AIQToolkit NVIDIA Integration Ready!${NC}"
echo -e "======================================"
echo -e "Local Web UI: ${YELLOW}http://localhost:3000${NC}"
echo -e "Backend API: ${YELLOW}http://localhost:8000${NC}"
echo -e "API Docs: ${YELLOW}http://localhost:8000/docs${NC}"
echo -e "\nNVIDIA Brev Instance: ${YELLOW}https://jupyter0-s1ondnjfx.brevlab.com${NC}"
echo -e "\n${YELLOW}To deploy to Vercel:${NC}"
echo -e "1. cd $BASE_DIR/web-ui"
echo -e "2. vercel"
echo -e "\nPress Ctrl+C to stop all services"

# Cleanup function
cleanup() {
    echo -e "\n${YELLOW}Stopping services...${NC}"
    kill $BACKEND_PID 2>/dev/null
    kill $UI_PID 2>/dev/null
    echo -e "${GREEN}Services stopped${NC}"
}

trap cleanup EXIT

# Keep running
while true; do
    sleep 1
done