#!/bin/bash
# Production Deployment Script for Digital Human with NVIDIA Blueprint
# Full-featured implementation with all components

set -e

# Colors
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
RED='\033[0;31m'
BLUE='\033[0;34m'
NC='\033[0m'

echo -e "${GREEN}🚀 Deploying Production Digital Human with NVIDIA Blueprint${NC}"
echo -e "${YELLOW}===============================================${NC}"

# Configuration
export NVIDIA_API_KEY="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"
export NVIDIA_BLUEPRINT_PATH="/projects/digital-human"
export ENVIRONMENT="production"
export LOG_LEVEL="INFO"
export ENABLE_GPU="true"

# Directories
SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
PROJECT_ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"
LOG_DIR="$PROJECT_ROOT/logs"
PID_DIR="$PROJECT_ROOT/pids"

# Create necessary directories
mkdir -p "$LOG_DIR" "$PID_DIR"

# Function to check if service is running
check_service() {
    local service=$1
    local port=$2
    if lsof -Pi :$port -sTCP:LISTEN -t >/dev/null; then
        echo -e "${GREEN}✓ $service already running on port $port${NC}"
        return 0
    else
        return 1
    fi
}

# Function to start service
start_service() {
    local name=$1
    local command=$2
    local port=$3
    local log_file="$LOG_DIR/$name.log"
    
    echo -e "${YELLOW}Starting $name...${NC}"
    
    # Start the service
    eval "$command > $log_file 2>&1 &"
    local pid=$!
    echo $pid > "$PID_DIR/$name.pid"
    
    # Wait for service to start
    local attempts=0
    while [ $attempts -lt 30 ]; do
        if check_service "$name" "$port"; then
            echo -e "${GREEN}✓ $name started successfully (PID: $pid)${NC}"
            return 0
        fi
        sleep 1
        attempts=$((attempts + 1))
    done
    
    echo -e "${RED}✗ Failed to start $name${NC}"
    return 1
}

# Check prerequisites
echo -e "${YELLOW}Checking prerequisites...${NC}"

# Check Python
if ! command -v python3 &> /dev/null; then
    echo -e "${RED}✗ Python 3 not found${NC}"
    exit 1
fi

# Check NVIDIA GPU
if ! nvidia-smi &> /dev/null; then
    echo -e "${YELLOW}⚠ NVIDIA GPU not detected. Performance will be limited.${NC}"
    export ENABLE_GPU="false"
else
    echo -e "${GREEN}✓ NVIDIA GPU detected${NC}"
    nvidia-smi --query-gpu=name,memory.total --format=csv,noheader
fi

# Check Blueprint
if [ -d "$NVIDIA_BLUEPRINT_PATH" ]; then
    echo -e "${GREEN}✓ NVIDIA Blueprint found at $NVIDIA_BLUEPRINT_PATH${NC}"
else
    echo -e "${YELLOW}⚠ NVIDIA Blueprint not found at $NVIDIA_BLUEPRINT_PATH${NC}"
    echo -e "${YELLOW}  Continuing with AIQToolkit implementation${NC}"
fi

# Clean up existing processes
echo -e "${YELLOW}Cleaning up existing processes...${NC}"
pkill -f "digital_human" || true
lsof -ti:8000 | xargs kill -9 2>/dev/null || true
lsof -ti:8001 | xargs kill -9 2>/dev/null || true
lsof -ti:8002 | xargs kill -9 2>/dev/null || true
lsof -ti:8080 | xargs kill -9 2>/dev/null || true

sleep 2

# Setup Python environment
echo -e "${YELLOW}Setting up Python environment...${NC}"
cd "$PROJECT_ROOT"

if [ ! -d "venv" ]; then
    python3 -m venv venv
fi

source venv/bin/activate
pip install -r requirements.txt

# Install additional production dependencies
pip install gunicorn uvloop httptools redis psycopg2-binary

# Start PostgreSQL (if using Docker)
if command -v docker &> /dev/null; then
    echo -e "${YELLOW}Starting PostgreSQL...${NC}"
    docker run -d --name digital-human-postgres \
        -e POSTGRES_DB=digital_human \
        -e POSTGRES_USER=aiqtoolkit \
        -e POSTGRES_PASSWORD=secure_password \
        -p 5432:5432 \
        postgres:15-alpine 2>/dev/null || true
fi

# Start Redis
echo -e "${YELLOW}Starting Redis...${NC}"
docker run -d --name digital-human-redis \
    -p 6379:6379 \
    redis:7-alpine 2>/dev/null || true

# Wait for services
sleep 3

# Start Neural Supercomputer Connector
start_service "neural-connector" \
    "python -m aiq.digital_human.neural.neural_supercomputer_connector \
        --host 0.0.0.0 --port 8002" \
    8002

# Start Financial Engine (MCTS)
start_service "financial-engine" \
    "python -m aiq.digital_human.financial.mcts_analyzer_server \
        --host 0.0.0.0 --port 8003" \
    8003

# Start Model Context Server (RAG)
start_service "context-server" \
    "python -m aiq.digital_human.retrieval.model_context_server \
        --host 0.0.0.0 --port 8004" \
    8004

# Start Conversation Engine (SgLang)
start_service "conversation-engine" \
    "python -m aiq.digital_human.conversation.sglang_engine_server \
        --host 0.0.0.0 --port 8005" \
    8005

# Start Avatar Renderer
start_service "avatar-renderer" \
    "python -m aiq.digital_human.avatar.avatar_renderer_server \
        --host 0.0.0.0 --port 8006" \
    8006

# Start Main Orchestrator
start_service "orchestrator" \
    "python -m uvicorn aiq.digital_human.orchestrator.digital_human_orchestrator:app \
        --host 0.0.0.0 --port 8000 --workers 4" \
    8000

# Start WebSocket Server
start_service "websocket" \
    "python -m aiq.digital_human.ui.websocket.websocket_server \
        --host 0.0.0.0 --port 8001" \
    8001

# Start Frontend UI (Production Build)
echo -e "${YELLOW}Building frontend...${NC}"
cd "$PROJECT_ROOT/ui/frontend"

if [ -d "node_modules" ]; then
    npm run build
else
    npm install && npm run build
fi

# Serve frontend with NGINX or Python
start_service "frontend" \
    "python -m http.server 8080 --directory $PROJECT_ROOT/ui/frontend" \
    8080

cd "$PROJECT_ROOT"

# Start Monitoring (Prometheus + Grafana)
if command -v docker &> /dev/null; then
    echo -e "${YELLOW}Starting monitoring stack...${NC}"
    
    # Prometheus
    docker run -d --name digital-human-prometheus \
        -p 9090:9090 \
        -v "$PROJECT_ROOT/deployment/monitoring/prometheus.yml:/etc/prometheus/prometheus.yml" \
        prom/prometheus 2>/dev/null || true
    
    # Grafana
    docker run -d --name digital-human-grafana \
        -p 3000:3000 \
        -e GF_SECURITY_ADMIN_PASSWORD=admin \
        grafana/grafana 2>/dev/null || true
fi

# Health check
echo -e "${YELLOW}Performing health check...${NC}"
sleep 5

# Check all services
services=(
    "Orchestrator:8000"
    "WebSocket:8001"
    "Neural-Connector:8002"
    "Financial-Engine:8003"
    "Context-Server:8004"
    "Conversation-Engine:8005"
    "Avatar-Renderer:8006"
    "Frontend:8080"
)

echo -e "${YELLOW}Service Status:${NC}"
for service in "${services[@]}"; do
    IFS=':' read -ra ADDR <<< "$service"
    name="${ADDR[0]}"
    port="${ADDR[1]}"
    
    if check_service "$name" "$port"; then
        echo -e "${GREEN}✓ $name: Running on port $port${NC}"
    else
        echo -e "${RED}✗ $name: Not running${NC}"
    fi
done

# Final status
echo -e "\n${GREEN}🎉 Production Digital Human Deployed!${NC}"
echo -e "${YELLOW}=====================================	{NC}"
echo -e "${BLUE}Main Application:${NC}"
echo -e "  Frontend: ${GREEN}http://localhost:8080${NC}"
echo -e "  API: ${GREEN}http://localhost:8000${NC}"
echo -e "  WebSocket: ${GREEN}ws://localhost:8001${NC}"
echo -e ""
echo -e "${BLUE}Services:${NC}"
echo -e "  Neural Connector: ${GREEN}http://localhost:8002${NC}"
echo -e "  Financial Engine: ${GREEN}http://localhost:8003${NC}"
echo -e "  Context Server: ${GREEN}http://localhost:8004${NC}"
echo -e "  Conversation Engine: ${GREEN}http://localhost:8005${NC}"
echo -e "  Avatar Renderer: ${GREEN}http://localhost:8006${NC}"
echo -e ""
echo -e "${BLUE}Monitoring:${NC}"
echo -e "  Prometheus: ${GREEN}http://localhost:9090${NC}"
echo -e "  Grafana: ${GREEN}http://localhost:3000${NC} (admin/admin)"
echo -e ""
echo -e "${BLUE}NVIDIA Services:${NC}"
echo -e "  API Key: Configured ✓"
echo -e "  Blueprint: ${NVIDIA_BLUEPRINT_PATH}"
echo -e "  GPU: ${ENABLE_GPU}"
echo -e ""
echo -e "${YELLOW}Logs:${NC}"
echo -e "  tail -f $LOG_DIR/*.log"
echo -e ""
echo -e "${YELLOW}To stop all services:${NC}"
echo -e "  $SCRIPT_DIR/stop_all.sh"

# Create stop script
cat > "$SCRIPT_DIR/stop_all.sh" << 'EOF'
#!/bin/bash
echo "Stopping all Digital Human services..."

# Read PIDs and stop services
for pid_file in pids/*.pid; do
    if [ -f "$pid_file" ]; then
        pid=$(cat "$pid_file")
        kill $pid 2>/dev/null || true
        rm "$pid_file"
    fi
done

# Stop Docker containers
docker stop digital-human-postgres digital-human-redis digital-human-prometheus digital-human-grafana 2>/dev/null || true
docker rm digital-human-postgres digital-human-redis digital-human-prometheus digital-human-grafana 2>/dev/null || true

echo "All services stopped."
EOF

chmod +x "$SCRIPT_DIR/stop_all.sh"

echo -e "\n${GREEN}Deployment complete! Access the Digital Human at http://localhost:8080${NC}"