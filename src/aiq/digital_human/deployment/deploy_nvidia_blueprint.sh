#!/bin/bash

# Deploy Digital Human with NVIDIA Blueprint Integration
# Uses the downloaded blueprint from /projects/digital-human

set -e

# Colors for output
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
RED='\033[0;31m'
NC='\033[0m'

echo -e "${GREEN}🚀 Deploying Digital Human with NVIDIA Blueprint${NC}"
echo -e "${YELLOW}Blueprint location: /projects/digital-human${NC}"

# Set environment variables
export NVIDIA_API_KEY="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"
export NVIDIA_BLUEPRINT_PATH="/projects/digital-human"

# Check if blueprint exists
if [ ! -d "$NVIDIA_BLUEPRINT_PATH" ]; then
    echo -e "${RED}Error: NVIDIA Blueprint not found at $NVIDIA_BLUEPRINT_PATH${NC}"
    exit 1
fi

# Setup Python environment
echo -e "${YELLOW}Setting up Python environment...${NC}"
cd "$(dirname "$0")/.."

# Create virtual environment if not exists
if [ ! -d "venv" ]; then
    python3 -m venv venv
fi

source venv/bin/activate

# Install requirements
pip install -r requirements.txt

# Install blueprint dependencies if they exist
if [ -f "$NVIDIA_BLUEPRINT_PATH/requirements.txt" ]; then
    echo -e "${YELLOW}Installing blueprint dependencies...${NC}"
    pip install -r "$NVIDIA_BLUEPRINT_PATH/requirements.txt"
fi

# Copy blueprint configuration
echo -e "${YELLOW}Integrating blueprint configuration...${NC}"
cp .env.nvidia .env

# Update Python path to include blueprint
export PYTHONPATH="$NVIDIA_BLUEPRINT_PATH:$PYTHONPATH"

# Run integration test
echo -e "${YELLOW}Testing NVIDIA Blueprint integration...${NC}"
python nvidia_blueprint/blueprint_integration.py

# Start the digital human services
echo -e "${GREEN}Starting Digital Human services...${NC}"

# Option 1: Docker deployment
if command -v docker &> /dev/null; then
    echo -e "${YELLOW}Using Docker deployment...${NC}"
    docker-compose -f deployment/docker-compose.nvidia-blueprint.yml up -d
else
    # Option 2: Direct Python deployment
    echo -e "${YELLOW}Using direct Python deployment...${NC}"
    
    # Start backend services
    python ui/api/unified_backend.py &
    BACKEND_PID=$!
    
    # Start frontend
    cd ui/frontend
    python -m http.server 8080 &
    FRONTEND_PID=$!
    
    cd ../..
fi

echo -e "\n${GREEN}✅ Digital Human with NVIDIA Blueprint deployed!${NC}"
echo -e "===================================================="
echo -e "Frontend: ${YELLOW}http://localhost:8080${NC}"
echo -e "Backend API: ${YELLOW}http://localhost:8000${NC}"
echo -e "Health Check: ${YELLOW}http://localhost:8000/health${NC}"
echo -e "Blueprint Status: ${YELLOW}http://localhost:8000/nvidia/status${NC}"
echo -e "\n${YELLOW}View logs:${NC}"
echo -e "  docker-compose logs -f   (if using Docker)"
echo -e "  tail -f logs/*.log       (if using Python)"

# Create systemd service for production
if [ "$1" == "--production" ]; then
    echo -e "\n${YELLOW}Creating systemd service...${NC}"
    sudo cp deployment/digital-human-blueprint.service /etc/systemd/system/
    sudo systemctl daemon-reload
    sudo systemctl enable digital-human-blueprint
    sudo systemctl start digital-human-blueprint
    echo -e "${GREEN}Production service started${NC}"
fi

echo -e "\n${GREEN}Deployment complete!${NC}"