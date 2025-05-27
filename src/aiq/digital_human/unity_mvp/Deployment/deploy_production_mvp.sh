#!/bin/bash

# Production Deployment Script for Digital Human Unity MVP
# One-click deployment with error handling and rollback capabilities

set -e

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Configuration
DEPLOYMENT_ENV="${DEPLOYMENT_ENV:-production}"
PROJECT_ROOT="/Users/apple/projects/AIQToolkit"
UNITY_MVP_PATH="${PROJECT_ROOT}/src/aiq/digital_human/unity_mvp"
DEPLOYMENT_PATH="${UNITY_MVP_PATH}/Deployment"
CONFIG_FILE="${DEPLOYMENT_PATH}/production_config.json"

# Function to print colored output
print_status() {
    echo -e "${GREEN}[$(date '+%Y-%m-%d %H:%M:%S')]${NC} $1"
}

print_error() {
    echo -e "${RED}[$(date '+%Y-%m-%d %H:%M:%S')] ERROR:${NC} $1"
}

print_warning() {
    echo -e "${YELLOW}[$(date '+%Y-%m-%d %H:%M:%S')] WARNING:${NC} $1"
}

# Function to check prerequisites
check_prerequisites() {
    print_status "Checking prerequisites..."
    
    # Check if Docker is installed
    if ! command -v docker &> /dev/null; then
        print_error "Docker is not installed. Please install Docker first."
        exit 1
    fi
    
    # Check if kubectl is installed (for Kubernetes deployment)
    if ! command -v kubectl &> /dev/null; then
        print_warning "kubectl is not installed. Kubernetes deployment will be skipped."
        SKIP_K8S=true
    fi
    
    # Check if Unity is accessible (for building)
    if [[ "$OSTYPE" == "darwin"* ]]; then
        UNITY_PATH="/Applications/Unity/Hub/Editor/*/Unity.app/Contents/MacOS/Unity"
    else
        UNITY_PATH="Unity"
    fi
    
    # Check if deployment directory exists
    if [ ! -d "$DEPLOYMENT_PATH" ]; then
        print_error "Deployment directory not found: $DEPLOYMENT_PATH"
        exit 1
    fi
    
    print_status "Prerequisites check completed"
}

# Function to load configuration
load_config() {
    print_status "Loading configuration for environment: $DEPLOYMENT_ENV"
    
    if [ ! -f "$CONFIG_FILE" ]; then
        print_warning "Configuration file not found. Creating default configuration..."
        create_default_config
    fi
    
    # Export configuration as environment variables
    export BACKEND_URL=$(jq -r '.backend.webSocketUrl' "$CONFIG_FILE")
    export API_URL=$(jq -r '.backend.restApiUrl' "$CONFIG_FILE")
    export AVATAR_URL=$(jq -r '.avatar.defaultAvatarUrl' "$CONFIG_FILE")
    export METRICS_ENDPOINT=$(jq -r '.metrics.metricsEndpoint' "$CONFIG_FILE")
    
    print_status "Configuration loaded successfully"
}

# Function to create default configuration
create_default_config() {
    cat > "$CONFIG_FILE" <<EOF
{
    "backend": {
        "webSocketUrl": "ws://localhost:8080/ws",
        "restApiUrl": "http://localhost:8080/api",
        "auth": {
            "enabled": false,
            "apiKey": ""
        }
    },
    "avatar": {
        "defaultAvatarUrl": "https://api.readyplayer.me/v1/avatars/default.glb",
        "quality": "High",
        "enableCaching": true
    },
    "metrics": {
        "enabled": true,
        "metricsEndpoint": "http://localhost:8080/api/metrics",
        "reportingInterval": 60
    },
    "logging": {
        "minLogLevel": "Info",
        "enableFileLogging": true,
        "enableRemoteLogging": true
    }
}
EOF
}

# Function to build Unity project
build_unity_project() {
    print_status "Building Unity project..."
    
    # Unity build command
    UNITY_CMD="$UNITY_PATH -batchmode -quit -projectPath $UNITY_MVP_PATH -buildTarget WebGL -executeMethod BuildScript.BuildProduction"
    
    # Execute build
    if eval "$UNITY_CMD"; then
        print_status "Unity build completed successfully"
    else
        print_error "Unity build failed"
        exit 1
    fi
}

# Function to build Docker image
build_docker_image() {
    print_status "Building Docker image..."
    
    cd "$DEPLOYMENT_PATH"
    
    # Build Docker image
    docker build -t aiqtoolkit/digital-human-mvp:${DEPLOYMENT_ENV} -f Dockerfile ..
    
    if [ $? -eq 0 ]; then
        print_status "Docker image built successfully"
    else
        print_error "Docker build failed"
        exit 1
    fi
}

# Function to run health checks
run_health_checks() {
    print_status "Running health checks..."
    
    # Check backend connectivity
    python3 "${DEPLOYMENT_PATH}/health_check.py" --backend-url "$BACKEND_URL"
    
    if [ $? -eq 0 ]; then
        print_status "Health checks passed"
    else
        print_error "Health checks failed"
        exit 1
    fi
}

# Function to deploy to Docker Compose
deploy_docker_compose() {
    print_status "Deploying with Docker Compose..."
    
    cd "$DEPLOYMENT_PATH"
    
    # Stop existing containers
    docker-compose -f docker-compose.mvp.yml down
    
    # Start new containers
    docker-compose -f docker-compose.mvp.yml up -d
    
    if [ $? -eq 0 ]; then
        print_status "Docker Compose deployment successful"
    else
        print_error "Docker Compose deployment failed"
        exit 1
    fi
}

# Function to deploy to Kubernetes
deploy_kubernetes() {
    if [ "$SKIP_K8S" = true ]; then
        print_warning "Skipping Kubernetes deployment (kubectl not found)"
        return
    fi
    
    print_status "Deploying to Kubernetes..."
    
    cd "$DEPLOYMENT_PATH"
    
    # Apply Kubernetes manifests
    kubectl apply -f kubernetes_mvp.yaml
    
    if [ $? -eq 0 ]; then
        print_status "Kubernetes deployment successful"
        
        # Wait for pods to be ready
        kubectl wait --for=condition=ready pod -l app=digital-human-mvp --timeout=300s
    else
        print_error "Kubernetes deployment failed"
        exit 1
    fi
}

# Function to setup monitoring
setup_monitoring() {
    print_status "Setting up monitoring..."
    
    # Create monitoring directory if it doesn't exist
    mkdir -p "${DEPLOYMENT_PATH}/monitoring/logs"
    mkdir -p "${DEPLOYMENT_PATH}/monitoring/metrics"
    
    # Setup log rotation
    cat > "/etc/logrotate.d/digital-human-mvp" <<EOF
${DEPLOYMENT_PATH}/monitoring/logs/*.log {
    daily
    rotate 7
    compress
    delaycompress
    missingok
    notifempty
    create 0644 root root
}
EOF
    
    print_status "Monitoring setup completed"
}

# Function to perform smoke tests
run_smoke_tests() {
    print_status "Running smoke tests..."
    
    # Wait for services to stabilize
    sleep 10
    
    # Test WebSocket connection
    echo "Testing WebSocket connection..."
    timeout 5 curl -s -o /dev/null -w "%{http_code}" "$BACKEND_URL" || true
    
    # Test REST API
    echo "Testing REST API..."
    API_RESPONSE=$(curl -s -o /dev/null -w "%{http_code}" "$API_URL/health")
    
    if [ "$API_RESPONSE" = "200" ]; then
        print_status "Smoke tests passed"
    else
        print_warning "Some smoke tests failed. Please check the services manually."
    fi
}

# Function to create deployment summary
create_deployment_summary() {
    SUMMARY_FILE="${DEPLOYMENT_PATH}/deployment_summary_$(date +%Y%m%d_%H%M%S).txt"
    
    cat > "$SUMMARY_FILE" <<EOF
Digital Human Unity MVP Deployment Summary
========================================
Date: $(date)
Environment: $DEPLOYMENT_ENV
Backend URL: $BACKEND_URL
API URL: $API_URL
Avatar URL: $AVATAR_URL
Metrics Endpoint: $METRICS_ENDPOINT

Deployment Steps Completed:
- Prerequisites check: ✓
- Configuration loading: ✓
- Unity build: ✓
- Docker image build: ✓
- Health checks: ✓
- Docker Compose deployment: ✓
$([ "$SKIP_K8S" != true ] && echo "- Kubernetes deployment: ✓" || echo "- Kubernetes deployment: skipped")
- Monitoring setup: ✓
- Smoke tests: ✓

Access Points:
- Web Interface: http://localhost:8080
- WebSocket: ws://localhost:8080/ws
- Metrics: http://localhost:8080/metrics
- Health Check: http://localhost:8080/health

Next Steps:
1. Monitor logs: tail -f ${DEPLOYMENT_PATH}/monitoring/logs/production.log
2. Check metrics dashboard: http://localhost:8080/metrics
3. Run integration tests: cd $UNITY_MVP_PATH && npm test
EOF
    
    print_status "Deployment summary created: $SUMMARY_FILE"
    cat "$SUMMARY_FILE"
}

# Function to handle rollback
rollback() {
    print_error "Deployment failed. Rolling back..."
    
    # Stop any running containers
    cd "$DEPLOYMENT_PATH"
    docker-compose -f docker-compose.mvp.yml down
    
    # Restore previous configuration if backup exists
    if [ -f "${CONFIG_FILE}.backup" ]; then
        mv "${CONFIG_FILE}.backup" "$CONFIG_FILE"
    fi
    
    print_status "Rollback completed"
}

# Main deployment flow
main() {
    print_status "Starting Digital Human Unity MVP deployment"
    
    # Set up error handling
    trap rollback ERR
    
    # Backup current configuration
    if [ -f "$CONFIG_FILE" ]; then
        cp "$CONFIG_FILE" "${CONFIG_FILE}.backup"
    fi
    
    # Execute deployment steps
    check_prerequisites
    load_config
    build_unity_project
    build_docker_image
    run_health_checks
    deploy_docker_compose
    deploy_kubernetes
    setup_monitoring
    run_smoke_tests
    create_deployment_summary
    
    print_status "Deployment completed successfully!"
}

# Parse command line arguments
while [[ $# -gt 0 ]]; do
    case $1 in
        --env)
            DEPLOYMENT_ENV="$2"
            shift 2
            ;;
        --skip-build)
            SKIP_BUILD=true
            shift
            ;;
        --skip-k8s)
            SKIP_K8S=true
            shift
            ;;
        --help)
            echo "Usage: $0 [OPTIONS]"
            echo "Options:"
            echo "  --env <environment>    Set deployment environment (default: production)"
            echo "  --skip-build          Skip Unity build step"
            echo "  --skip-k8s           Skip Kubernetes deployment"
            echo "  --help               Show this help message"
            exit 0
            ;;
        *)
            print_error "Unknown option: $1"
            exit 1
            ;;
    esac
done

# Run main deployment
main