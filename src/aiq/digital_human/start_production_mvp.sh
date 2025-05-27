#!/bin/bash
# Quick Start Script for Production Unity MVP

echo "🚀 Starting Production Unity Digital Human MVP"
echo "==========================================="

# Navigate to MVP directory
cd /Users/apple/projects/AIQToolkit/src/aiq/digital_human/unity_mvp

# Check if Docker is running
if ! docker info > /dev/null 2>&1; then
    echo "❌ Docker is not running. Please start Docker Desktop."
    exit 1
fi

# Start services with Docker Compose
echo "📦 Starting services with Docker Compose..."
cd Deployment
docker-compose -f docker-compose.mvp.yml up -d

# Wait for services to start
echo "⏳ Waiting for services to initialize..."
sleep 10

# Check health
echo "🏥 Checking service health..."
python3 monitoring/health_check.py

# Display access URLs
echo ""
echo "✅ Production MVP is running!"
echo ""
echo "🌐 Access URLs:"
echo "===================="
echo "Unity Web App: http://localhost:8080"
echo "Backend API: http://localhost:8000"
echo "WebSocket: ws://localhost:8088/ws"
echo "Grafana Dashboard: http://localhost:3000"
echo "Prometheus: http://localhost:9090"
echo ""
echo "📊 Default Credentials:"
echo "Grafana: admin/admin"
echo ""
echo "🛑 To stop: docker-compose -f docker-compose.mvp.yml down"
echo ""

# Monitor logs
echo "📜 Showing live logs (Ctrl+C to exit)..."
docker-compose -f docker-compose.mvp.yml logs -f