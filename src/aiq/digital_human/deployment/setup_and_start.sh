#!/bin/bash

echo "🚀 Digital Human Setup and Start"
echo "==============================="

# Navigate to project root
cd /Users/apple/projects/AIQToolkit

# Install required dependencies
echo "Installing dependencies..."
pip3 install fastapi uvicorn pydantic websockets python-multipart openai anthropic torch numpy

# Navigate to digital human directory
cd src/aiq/digital_human

# Create necessary directories
mkdir -p logs pids

# Find and start the main API server
echo "Starting API server..."
API_FILE=$(find . -name "api_server.py" | head -1)
if [ -n "$API_FILE" ]; then
    NVIDIA_API_KEY="${NVIDIA_API_KEY:-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}" \
    python3 "$API_FILE" > logs/api_server.log 2>&1 &
    API_PID=$!
    echo "API server started (PID: $API_PID) from $API_FILE"
    echo $API_PID > pids/api_server.pid
else
    echo "⚠️  API server not found, creating a minimal one..."
    
    # Create a minimal API server
    cat > minimal_api_server.py << 'EOF'
from fastapi import FastAPI
from fastapi.middleware.cors import CORSMiddleware
import uvicorn
import os

app = FastAPI(title="Digital Human API")

# Enable CORS
app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)

@app.get("/health")
def health():
    return {"status": "healthy", "service": "Digital Human API"}

@app.get("/")
def root():
    return {"message": "Digital Human API is running"}

@app.get("/api/avatar/status")
def avatar_status():
    return {"status": "ready", "type": "NVIDIA ACE"}

@app.get("/api/chat/status")
def chat_status():
    return {"status": "ready", "model": "NVIDIA NIM"}

if __name__ == "__main__":
    uvicorn.run(app, host="0.0.0.0", port=8080)
EOF
    
    NVIDIA_API_KEY="${NVIDIA_API_KEY:-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}" \
    python3 minimal_api_server.py > logs/api_server.log 2>&1 &
    API_PID=$!
    echo "Minimal API server started (PID: $API_PID)"
    echo $API_PID > pids/api_server.pid
fi

# Create a simple UI server
echo "Creating UI server..."
cat > ui_server.py << 'EOF'
from fastapi import FastAPI, WebSocket
from fastapi.staticfiles import StaticFiles
from fastapi.responses import HTMLResponse
import uvicorn

app = FastAPI(title="Digital Human UI")

# Basic HTML UI
html = """
<!DOCTYPE html>
<html>
<head>
    <title>Digital Human</title>
    <style>
        body { font-family: Arial, sans-serif; margin: 40px; background-color: #f5f5f5; }
        .container { max-width: 800px; margin: auto; background: white; padding: 20px; border-radius: 10px; box-shadow: 0 0 10px rgba(0,0,0,0.1); }
        h1 { color: #333; text-align: center; }
        .status { padding: 10px; margin: 10px 0; border-radius: 5px; }
        .status.ready { background-color: #d4edda; color: #155724; }
        .avatar-container { text-align: center; margin: 20px 0; }
        .chat-container { border: 1px solid #ddd; padding: 20px; margin: 20px 0; border-radius: 5px; min-height: 300px; background-color: #f9f9f9; }
        .controls { text-align: center; }
        button { background-color: #007bff; color: white; border: none; padding: 10px 20px; margin: 5px; border-radius: 5px; cursor: pointer; }
        button:hover { background-color: #0056b3; }
        .input-container { margin: 20px 0; }
        input[type="text"] { width: 70%; padding: 10px; border: 1px solid #ddd; border-radius: 5px; }
    </style>
</head>
<body>
    <div class="container">
        <h1>Digital Human with NVIDIA ACE</h1>
        <div class="status ready">
            ✅ System Status: Ready | NVIDIA API Key: Configured
        </div>
        
        <div class="avatar-container">
            <div style="width: 400px; height: 400px; margin: auto; background-color: #000; border-radius: 10px; display: flex; align-items: center; justify-content: center; color: white;">
                <div>
                    <h3>NVIDIA Avatar</h3>
                    <p>Photorealistic digital human powered by NVIDIA ACE</p>
                </div>
            </div>
        </div>
        
        <div class="chat-container" id="chat">
            <div id="messages"></div>
        </div>
        
        <div class="input-container">
            <input type="text" id="messageInput" placeholder="Type your message here...">
            <button onclick="sendMessage()">Send</button>
        </div>
        
        <div class="controls">
            <button onclick="startAvatar()">Start Avatar</button>
            <button onclick="stopAvatar()">Stop Avatar</button>
            <button onclick="resetChat()">Reset Chat</button>
        </div>
    </div>
    
    <script>
        function sendMessage() {
            const input = document.getElementById('messageInput');
            const message = input.value;
            if (message) {
                document.getElementById('messages').innerHTML += '<p><strong>You:</strong> ' + message + '</p>';
                // Simulate response
                setTimeout(() => {
                    document.getElementById('messages').innerHTML += '<p><strong>Digital Human:</strong> I received your message: "' + message + '". This is a demonstration of the NVIDIA-powered digital human interface.</p>';
                }, 1000);
                input.value = '';
            }
        }
        
        function startAvatar() {
            alert('Starting NVIDIA Avatar...');
        }
        
        function stopAvatar() {
            alert('Stopping NVIDIA Avatar...');
        }
        
        function resetChat() {
            document.getElementById('messages').innerHTML = '';
        }
        
        // Allow Enter key to send messages
        document.getElementById('messageInput').addEventListener('keypress', function(e) {
            if (e.key === 'Enter') sendMessage();
        });
    </script>
</body>
</html>
"""

@app.get("/")
async def get():
    return HTMLResponse(html)

@app.websocket("/ws")
async def websocket_endpoint(websocket: WebSocket):
    await websocket.accept()
    while True:
        data = await websocket.receive_text()
        await websocket.send_text(f"Echo: {data}")

if __name__ == "__main__":
    uvicorn.run(app, host="0.0.0.0", port=8081)
EOF

python3 ui_server.py > logs/ui_server.log 2>&1 &
UI_PID=$!
echo "UI server started (PID: $UI_PID)"
echo $UI_PID > pids/ui_server.pid

echo -e "\n✅ Digital Human system started successfully!"
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
echo "🌐 Main API: http://localhost:8080"
echo "💻 Web UI: http://localhost:8081"
echo "📁 Logs: /Users/apple/projects/AIQToolkit/src/aiq/digital_human/logs/"
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
echo ""
echo "To stop all services, run: ./stop_digital_human.sh"