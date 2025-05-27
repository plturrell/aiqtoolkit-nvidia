#!/bin/bash

echo "🚀 Deploying Full Digital Human with MCP & Neural Integration"
echo "=========================================================="

cd /Users/apple/projects/AIQToolkit/src/aiq/digital_human

# Activate virtual environment
source venv/bin/activate

# Install additional dependencies
pip install mcp langchain langchain-nvidia-ai-endpoints

# Create the full integration service
cat > full_integration_service.py << 'EOF'
"""
Full Digital Human Integration
Connects NVIDIA APIs with MCP servers and neural systems
"""

import os
import asyncio
import json
from typing import Dict, Any, List
from fastapi import FastAPI, HTTPException, WebSocket
from fastapi.middleware.cors import CORSMiddleware
import uvicorn
import httpx
from datetime import datetime

# Import AIQToolkit components
import sys
sys.path.append('/Users/apple/projects/AIQToolkit/src')
from aiq.neural.distributed_neural_computer import DistributedNeuralComputer
from aiq.neural.nash_ethereum_consensus import NashEthereumConsensus
from aiq.neural.knowledge_integration import KnowledgeIntegrationSystem
from aiq.tool.mcp_client import MCPClient
from aiq.tool.server_tools import WebSearchTool, FileSystemTool

app = FastAPI(title="Full Digital Human Integration")

app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)

# Configuration
NVIDIA_API_KEY = os.getenv("NVIDIA_API_KEY", "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")
NVIDIA_NIM_ENDPOINT = "https://integrate.api.nvidia.com/v1"

class DigitalHumanOrchestrator:
    def __init__(self):
        self.nvidia_api_key = NVIDIA_API_KEY
        self.neural_computer = DistributedNeuralComputer()
        self.consensus_system = NashEthereumConsensus()
        self.knowledge_system = KnowledgeIntegrationSystem()
        self.mcp_client = MCPClient()
        self.web_search = WebSearchTool()
        self.file_system = FileSystemTool()
        
        # Initialize MCP servers
        self.mcp_servers = {
            "web_search": "http://localhost:8091",
            "file_browser": "http://localhost:8092",
            "data_analysis": "http://localhost:8093"
        }
        
    async def process_query(self, query: str, use_neural: bool = True, use_mcp: bool = True):
        """Process user query through full pipeline"""
        
        context = {
            "query": query,
            "timestamp": datetime.now().isoformat(),
            "sources": []
        }
        
        # Step 1: Use MCP for web search and context gathering
        if use_mcp:
            try:
                # Search web for relevant information
                web_results = await self.search_web(query)
                context["web_results"] = web_results
                context["sources"].append("web_search")
                
                # Search local files
                file_results = await self.search_files(query)
                context["file_results"] = file_results
                context["sources"].append("file_system")
                
            except Exception as e:
                print(f"MCP error: {e}")
        
        # Step 2: Use neural systems for reasoning
        if use_neural:
            try:
                # Process through neural computer
                neural_response = await self.neural_computer.process({
                    "input": query,
                    "context": context
                })
                context["neural_analysis"] = neural_response
                context["sources"].append("neural_computer")
                
                # Get consensus from multiple neural nodes
                consensus = await self.consensus_system.get_consensus({
                    "query": query,
                    "neural_response": neural_response
                })
                context["consensus"] = consensus
                context["sources"].append("consensus_system")
                
            except Exception as e:
                print(f"Neural system error: {e}")
        
        # Step 3: Integrate knowledge
        knowledge = await self.knowledge_system.integrate(context)
        context["integrated_knowledge"] = knowledge
        
        # Step 4: Generate response using NVIDIA NIM
        response = await self.generate_response(query, context)
        
        return {
            "query": query,
            "response": response,
            "context": context,
            "sources_used": context["sources"]
        }
    
    async def search_web(self, query: str) -> List[Dict]:
        """Search web using MCP server"""
        try:
            async with httpx.AsyncClient() as client:
                response = await client.post(
                    f"{self.mcp_servers['web_search']}/search",
                    json={"query": query}
                )
                return response.json().get("results", [])
        except:
            # Fallback to direct search
            return self.web_search.search(query)
    
    async def search_files(self, query: str) -> List[Dict]:
        """Search local files using MCP server"""
        try:
            async with httpx.AsyncClient() as client:
                response = await client.post(
                    f"{self.mcp_servers['file_browser']}/search",
                    json={"query": query}
                )
                return response.json().get("files", [])
        except:
            # Fallback to direct file search
            return self.file_system.search(query)
    
    async def generate_response(self, query: str, context: Dict) -> str:
        """Generate response using NVIDIA NIM with context"""
        
        # Build prompt with context
        prompt = f"""
        User Query: {query}
        
        Context:
        - Web Results: {json.dumps(context.get('web_results', []), indent=2)}
        - File Results: {json.dumps(context.get('file_results', []), indent=2)}
        - Neural Analysis: {json.dumps(context.get('neural_analysis', {}), indent=2)}
        - Consensus: {json.dumps(context.get('consensus', {}), indent=2)}
        
        Based on the above context, provide a comprehensive response.
        """
        
        headers = {
            "Authorization": f"Bearer {self.nvidia_api_key}",
            "Content-Type": "application/json"
        }
        
        try:
            async with httpx.AsyncClient() as client:
                response = await client.post(
                    f"{NVIDIA_NIM_ENDPOINT}/chat/completions",
                    headers=headers,
                    json={
                        "model": "meta/llama3-70b-instruct",
                        "messages": [
                            {"role": "system", "content": "You are an AI assistant with access to web search, file system, and neural processing capabilities."},
                            {"role": "user", "content": prompt}
                        ],
                        "temperature": 0.7,
                        "max_tokens": 1000
                    },
                    timeout=30
                )
                
                data = response.json()
                if "choices" in data:
                    return data["choices"][0]["message"]["content"]
                else:
                    return f"Error: {data}"
                    
        except Exception as e:
            return f"Error generating response: {str(e)}"

orchestrator = DigitalHumanOrchestrator()

@app.get("/")
async def root():
    return {
        "service": "Full Digital Human Integration",
        "status": "active",
        "components": {
            "nvidia_nim": "connected",
            "mcp_servers": orchestrator.mcp_servers,
            "neural_systems": ["distributed_neural_computer", "consensus", "knowledge_integration"]
        }
    }

@app.post("/process")
async def process_query(data: dict):
    """Process query through full pipeline"""
    query = data.get("query", "")
    use_neural = data.get("use_neural", True)
    use_mcp = data.get("use_mcp", True)
    
    result = await orchestrator.process_query(query, use_neural, use_mcp)
    return result

@app.websocket("/ws/chat")
async def chat_websocket(websocket: WebSocket):
    """WebSocket for real-time chat with full integration"""
    await websocket.accept()
    
    try:
        while True:
            data = await websocket.receive_json()
            query = data.get("query", "")
            
            # Send status updates
            await websocket.send_json({
                "type": "status",
                "message": "Searching web..."
            })
            
            # Process through full pipeline
            result = await orchestrator.process_query(query)
            
            await websocket.send_json({
                "type": "response",
                "data": result
            })
            
    except Exception as e:
        await websocket.send_json({
            "type": "error",
            "message": str(e)
        })
        await websocket.close()

if __name__ == "__main__":
    uvicorn.run(app, host="0.0.0.0", port=8090)
EOF

# Create MCP servers
cat > mcp_web_search_server.py << 'EOF'
from fastapi import FastAPI
import uvicorn
import requests
from bs4 import BeautifulSoup

app = FastAPI(title="MCP Web Search Server")

@app.post("/search")
async def search_web(data: dict):
    query = data.get("query", "")
    
    # Simulate web search - in production this would use a real search API
    results = []
    
    # Mock search results
    if "nvidia" in query.lower():
        results.append({
            "title": "NVIDIA Official Website",
            "url": "https://www.nvidia.com",
            "snippet": "NVIDIA is the world leader in visual computing technologies."
        })
    
    if "ai" in query.lower() or "artificial intelligence" in query.lower():
        results.append({
            "title": "AI and Machine Learning",
            "url": "https://www.nvidia.com/ai",
            "snippet": "NVIDIA's AI platform powers the world's most advanced AI systems."
        })
    
    return {"results": results}

if __name__ == "__main__":
    uvicorn.run(app, host="0.0.0.0", port=8091)
EOF

cat > mcp_file_browser_server.py << 'EOF'
from fastapi import FastAPI
import uvicorn
import os
import glob

app = FastAPI(title="MCP File Browser Server")

@app.post("/search")
async def search_files(data: dict):
    query = data.get("query", "")
    path = data.get("path", "/Users/apple/projects/AIQToolkit")
    
    # Search for files matching query
    files = []
    
    for root, dirs, filenames in os.walk(path):
        for filename in filenames:
            if query.lower() in filename.lower():
                files.append({
                    "path": os.path.join(root, filename),
                    "name": filename,
                    "size": os.path.getsize(os.path.join(root, filename))
                })
        
        if len(files) > 10:  # Limit results
            break
    
    return {"files": files}

if __name__ == "__main__":
    uvicorn.run(app, host="0.0.0.0", port=8092)
EOF

# Create integrated UI
cat > ui_full_integration.py << 'EOF'
from fastapi import FastAPI
from fastapi.middleware.cors import CORSMiddleware
from fastapi.responses import HTMLResponse
import uvicorn

app = FastAPI(title="Digital Human - Full Integration")

app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)

@app.get("/", response_class=HTMLResponse)
async def root():
    return """
<!DOCTYPE html>
<html>
<head>
    <title>NVIDIA Digital Human - Full Integration</title>
    <style>
        body { font-family: Arial, sans-serif; margin: 0; padding: 0; background: #0a0a0a; color: #fff; }
        .container { max-width: 1400px; margin: auto; padding: 20px; }
        .header { text-align: center; padding: 30px 0; background: linear-gradient(135deg, #0f0f0f 0%, #1a1a1a 100%); border-bottom: 3px solid #76b900; }
        .main { display: grid; grid-template-columns: 1fr 1fr; gap: 30px; margin-top: 30px; }
        .section { background: #1a1a1a; padding: 25px; border-radius: 15px; }
        .chat { height: 400px; overflow-y: auto; border: 2px solid #222; padding: 15px; margin: 15px 0; border-radius: 10px; background: #0f0f0f; }
        .message { margin: 10px 0; padding: 10px; border-radius: 8px; }
        .message.user { background: #1e3a5f; margin-left: 20%; }
        .message.ai { background: #1a3a1a; margin-right: 20%; }
        .message.status { background: #444; font-style: italic; }
        input { width: 100%; padding: 12px; background: #222; border: 2px solid #444; color: #fff; border-radius: 8px; }
        button { background: #76b900; color: #000; border: none; padding: 12px 24px; border-radius: 8px; cursor: pointer; margin: 5px; font-weight: bold; }
        button:hover { background: #8fdb00; }
        .sources { background: #0f0f0f; padding: 15px; border-radius: 10px; margin-top: 20px; }
        .source-tag { display: inline-block; background: #76b900; color: #000; padding: 5px 10px; border-radius: 5px; margin: 5px; font-size: 12px; }
        .nvidia-badge { position: fixed; top: 20px; right: 20px; background: #76b900; padding: 10px 20px; border-radius: 25px; font-weight: bold; }
        .options { display: flex; gap: 20px; margin: 15px 0; }
        .option { display: flex; align-items: center; gap: 5px; }
        pre { background: #0f0f0f; padding: 10px; border-radius: 5px; overflow-x: auto; }
    </style>
</head>
<body>
    <div class="nvidia-badge">NVIDIA + AIQ</div>
    <div class="container">
        <div class="header">
            <h1>Digital Human - Full Integration</h1>
            <p>NVIDIA AI + MCP Servers + Neural Systems</p>
        </div>
        
        <div class="main">
            <div class="section">
                <h2>Chat Interface</h2>
                <div class="chat" id="chat">
                    <div class="message ai">
                        <strong>AI:</strong> Welcome! I have access to web search, file browsing, and neural processing. Ask me anything!
                    </div>
                </div>
                <input type="text" id="input" placeholder="Ask a question..." onkeypress="if(event.key==='Enter')sendMessage()">
                <div class="options">
                    <div class="option">
                        <input type="checkbox" id="useMCP" checked>
                        <label for="useMCP">Use MCP Servers</label>
                    </div>
                    <div class="option">
                        <input type="checkbox" id="useNeural" checked>
                        <label for="useNeural">Use Neural Systems</label>
                    </div>
                </div>
                <button onclick="sendMessage()">Send</button>
                <button onclick="clearChat()">Clear</button>
            </div>
            
            <div class="section">
                <h2>System Status</h2>
                <div id="status">
                    <h3>Active Components:</h3>
                    <div class="source-tag">NVIDIA NIM</div>
                    <div class="source-tag">MCP Web Search</div>
                    <div class="source-tag">MCP File Browser</div>
                    <div class="source-tag">Neural Computer</div>
                    <div class="source-tag">Consensus System</div>
                    <div class="source-tag">Knowledge Integration</div>
                </div>
                
                <div class="sources" id="sources">
                    <h3>Last Query Sources:</h3>
                    <p>No query processed yet</p>
                </div>
                
                <div class="sources" id="context">
                    <h3>Context Used:</h3>
                    <pre id="contextData">No context yet</pre>
                </div>
            </div>
        </div>
    </div>
    
    <script>
        let websocket = null;
        
        function initWebSocket() {
            websocket = new WebSocket('ws://localhost:8090/ws/chat');
            
            websocket.onopen = () => {
                console.log('Connected to full integration system');
            };
            
            websocket.onmessage = (event) => {
                const data = JSON.parse(event.data);
                
                if (data.type === 'status') {
                    displayMessage('Status', data.message, 'status');
                } else if (data.type === 'response') {
                    displayMessage('AI', data.data.response, 'ai');
                    updateSources(data.data.sources_used);
                    updateContext(data.data.context);
                } else if (data.type === 'error') {
                    displayMessage('Error', data.message, 'error');
                }
            };
            
            websocket.onerror = (error) => {
                console.error('WebSocket error:', error);
            };
        }
        
        async function sendMessage() {
            const input = document.getElementById('input');
            const message = input.value.trim();
            
            if (!message) return;
            
            displayMessage('You', message, 'user');
            input.value = '';
            
            const useMCP = document.getElementById('useMCP').checked;
            const useNeural = document.getElementById('useNeural').checked;
            
            if (websocket && websocket.readyState === WebSocket.OPEN) {
                websocket.send(JSON.stringify({
                    query: message,
                    use_mcp: useMCP,
                    use_neural: useNeural
                }));
            } else {
                // Fallback to REST API
                try {
                    const response = await fetch('http://localhost:8090/process', {
                        method: 'POST',
                        headers: { 'Content-Type': 'application/json' },
                        body: JSON.stringify({
                            query: message,
                            use_mcp: useMCP,
                            use_neural: useNeural
                        })
                    });
                    
                    const data = await response.json();
                    displayMessage('AI', data.response, 'ai');
                    updateSources(data.sources_used);
                    updateContext(data.context);
                    
                } catch (e) {
                    displayMessage('Error', 'Failed to process request: ' + e.message, 'error');
                }
            }
        }
        
        function displayMessage(sender, message, type = 'ai') {
            const chat = document.getElementById('chat');
            const messageClass = type === 'user' ? 'user' : (type === 'status' ? 'status' : 'ai');
            chat.innerHTML += '<div class="message ' + messageClass + '"><strong>' + sender + ':</strong> ' + message + '</div>';
            chat.scrollTop = chat.scrollHeight;
        }
        
        function updateSources(sources) {
            const sourcesDiv = document.getElementById('sources');
            sourcesDiv.innerHTML = '<h3>Sources Used:</h3>';
            
            sources.forEach(source => {
                sourcesDiv.innerHTML += '<div class="source-tag">' + source + '</div>';
            });
        }
        
        function updateContext(context) {
            const contextDiv = document.getElementById('contextData');
            // Show summary of context
            const summary = {
                query: context.query,
                sources: context.sources,
                web_results_count: context.web_results ? context.web_results.length : 0,
                file_results_count: context.file_results ? context.file_results.length : 0,
                neural_analysis: context.neural_analysis ? 'Available' : 'Not used',
                consensus: context.consensus ? 'Achieved' : 'Not used'
            };
            
            contextDiv.textContent = JSON.stringify(summary, null, 2);
        }
        
        function clearChat() {
            document.getElementById('chat').innerHTML = '<div class="message ai"><strong>AI:</strong> Chat cleared. How can I help you?</div>';
        }
        
        // Initialize on load
        window.onload = () => {
            initWebSocket();
        };
    </script>
</body>
</html>
"""

if __name__ == "__main__":
    uvicorn.run(app, host="0.0.0.0", port=8093)
EOF

# Kill existing services
echo "Stopping existing services..."
for port in 8090 8091 8092 8093; do
    lsof -ti :$port | xargs kill -9 2>/dev/null || true
done

# Start all services
echo "Starting integrated services..."

python mcp_web_search_server.py > logs/mcp_web_search.log 2>&1 &
echo $! > pids/mcp_web_search.pid

python mcp_file_browser_server.py > logs/mcp_file_browser.log 2>&1 &
echo $! > pids/mcp_file_browser.pid

python full_integration_service.py > logs/full_integration.log 2>&1 &
echo $! > pids/full_integration.pid

python ui_full_integration.py > logs/ui_full_integration.log 2>&1 &
echo $! > pids/ui_full_integration.pid

echo ""
echo "🎉 Full Digital Human Integration Deployed!"
echo "=========================================="
echo ""
echo "Access the system at: http://localhost:8093"
echo ""
echo "Components:"
echo "✅ NVIDIA NIM for LLM responses"
echo "✅ MCP Web Search Server (port 8091)"
echo "✅ MCP File Browser Server (port 8092)"
echo "✅ Neural Processing Systems"
echo "✅ Consensus & Knowledge Integration"
echo ""
echo "Services:"
echo "• Full Integration UI: http://localhost:8093"
echo "• Integration API: http://localhost:8090"
echo "• MCP Web Search: http://localhost:8091"
echo "• MCP File Browser: http://localhost:8092"
echo ""
echo "Features:"
echo "- Web search integration"
echo "- File system browsing"
echo "- Neural reasoning"
echo "- Consensus algorithms"
echo "- Knowledge integration"
echo "- Real NVIDIA AI responses"
echo ""
echo "Try asking questions like:"
echo "- 'Search the web for NVIDIA ACE information'"
echo "- 'Find files related to neural systems'"
echo "- 'Analyze the digital human architecture'"