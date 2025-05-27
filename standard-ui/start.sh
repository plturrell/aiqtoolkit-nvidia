#!/bin/bash

echo "🚀 Starting AIQToolkit Standard UI"
echo "================================"

# Colors
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m'

# Base directory
BASE_DIR="$(cd "$(dirname "$0")" && pwd)"

# Kill existing processes
echo -e "${YELLOW}Cleaning up existing processes...${NC}"
lsof -ti:8000 | xargs kill -9 2>/dev/null || true
lsof -ti:3000 | xargs kill -9 2>/dev/null || true

# Start backend
echo -e "${GREEN}Starting backend API...${NC}"
cd "$BASE_DIR/backend"
python server.py > ../logs/backend.log 2>&1 &
BACKEND_PID=$!

sleep 3

# Install frontend dependencies if needed
cd "$BASE_DIR/frontend"
if [ ! -d "node_modules" ]; then
    echo -e "${YELLOW}Installing frontend dependencies...${NC}"
    npm install
fi

# Start frontend
echo -e "${GREEN}Starting frontend...${NC}"
npm run dev > ../logs/frontend.log 2>&1 &
FRONTEND_PID=$!

sleep 3

echo -e "\n${GREEN}✓ AIQToolkit Standard UI is running!${NC}"
echo -e "================================"
echo -e "Frontend: ${YELLOW}http://localhost:3000${NC}"
echo -e "Backend API: ${YELLOW}http://localhost:8000${NC}"
echo -e "API Docs: ${YELLOW}http://localhost:8000/docs${NC}"
echo -e "\nPress Ctrl+C to stop"

# Cleanup on exit
cleanup() {
    echo -e "\n${YELLOW}Stopping services...${NC}"
    kill $BACKEND_PID 2>/dev/null
    kill $FRONTEND_PID 2>/dev/null
    echo -e "${GREEN}Services stopped${NC}"
}

trap cleanup EXIT

# Keep running
while true; do
    sleep 1
done