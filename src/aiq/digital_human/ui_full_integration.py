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
