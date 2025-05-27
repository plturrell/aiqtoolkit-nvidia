#!/bin/bash
# Start Unity MVP Backend Services

echo "🚀 Starting Unity MVP Backend Services..."

# Check if deployment directory exists
DEPLOY_DIR="/Users/apple/projects/AIQToolkit/src/aiq/digital_human/deployment"
if [ ! -d "$DEPLOY_DIR" ]; then
    echo "❌ Deployment directory not found: $DEPLOY_DIR"
    exit 1
fi

# Navigate to deployment directory
cd "$DEPLOY_DIR"

# Start the full integration services for Unity backend
echo "🔧 Starting full integration services..."
if [ -f "deploy_full_integration.sh" ]; then
    echo "Running deploy_full_integration.sh..."
    bash deploy_full_integration.sh
else
    echo "⚠️ Full integration script not found, starting minimal services..."
    
    # Start minimal required services
    echo "📡 Starting WebSocket server..."
    python -m aiq.digital_human.ui.consensus_websocket_handler &
    WEBSOCKET_PID=$!
    
    echo "🌐 Starting API server..."
    python -m aiq.digital_human.ui.api_server &
    API_PID=$!
    
    echo "🧠 Starting LangChain backend..."
    python -m aiq.digital_human.full_integration_simple &
    LANGCHAIN_PID=$!
    
    echo "✅ Services started:"
    echo "   WebSocket Server: PID $WEBSOCKET_PID"
    echo "   API Server: PID $API_PID"
    echo "   LangChain Backend: PID $LANGCHAIN_PID"
fi

# Create Unity connection test endpoint
cat > test_unity_connection.py << 'EOF'
import asyncio
import websockets
import json

async def test_connection():
    uri = "ws://localhost:8080/ws"
    try:
        async with websockets.connect(uri) as websocket:
            # Send test message
            test_msg = json.dumps({
                "type": "chat",
                "data": {"query": "Hello from Unity test"}
            })
            await websocket.send(test_msg)
            
            # Wait for response
            response = await websocket.recv()
            data = json.loads(response)
            print(f"✅ Connection successful! Response: {data}")
            return True
    except Exception as e:
        print(f"❌ Connection failed: {e}")
        return False

if __name__ == "__main__":
    asyncio.run(test_connection())
EOF

# Wait for services to start
echo "⏳ Waiting for services to start..."
sleep 5

# Test the connection
echo "🧪 Testing Unity backend connection..."
python test_unity_connection.py

# Display connection information
echo ""
echo "🎮 Unity Backend Services Started!"
echo ""
echo "Connection Details:"
echo "==================="
echo "WebSocket URL: ws://localhost:8080/ws"
echo "REST API URL: http://localhost:8000"
echo "MCP Web Search: Available"
echo "MCP File Browser: Available"
echo ""
echo "Use these URLs in your Unity BackendConnector configuration."
echo ""
echo "To stop services, press Ctrl+C"
echo ""

# Keep script running
wait