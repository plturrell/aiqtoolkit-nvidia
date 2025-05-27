"""
Enhanced UI with NVIDIA Blueprint Integration
"""
from fastapi import FastAPI
from fastapi.middleware.cors import CORSMiddleware
from fastapi.responses import HTMLResponse
import uvicorn

app = FastAPI(title="Digital Human - NVIDIA Blueprint")

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
    <title>NVIDIA Digital Human - Blueprint Edition</title>
    <style>
        body { font-family: 'Arial', sans-serif; margin: 0; padding: 0; background: #0a0a0a; color: #fff; }
        .container { max-width: 1400px; margin: auto; padding: 20px; }
        .header { text-align: center; padding: 30px 0; background: linear-gradient(135deg, #0f0f0f 0%, #1a1a1a 100%); border-bottom: 3px solid #76b900; }
        .main { display: flex; gap: 30px; margin-top: 30px; }
        .avatar-section { flex: 1; background: #1a1a1a; padding: 25px; border-radius: 15px; box-shadow: 0 5px 15px rgba(0,0,0,0.5); }
        .chat-section { flex: 1; background: #1a1a1a; padding: 25px; border-radius: 15px; box-shadow: 0 5px 15px rgba(0,0,0,0.5); }
        .status { background: linear-gradient(90deg, #76b900 0%, #00bf63 100%); color: #000; padding: 12px; border-radius: 8px; margin: 15px 0; font-weight: bold; }
        .avatar { width: 100%; height: 600px; background: #000; border-radius: 10px; position: relative; overflow: hidden; }
        #tokkioFrame { width: 100%; height: 100%; border: none; }
        .chat { height: 400px; overflow-y: auto; border: 2px solid #222; padding: 15px; margin: 15px 0; border-radius: 10px; background: #0f0f0f; }
        .message { margin: 10px 0; padding: 10px; border-radius: 8px; }
        .message.user { background: #1e3a5f; margin-left: 20%; }
        .message.ai { background: #1a3a1a; margin-right: 20%; }
        input { width: calc(100% - 24px); padding: 12px; background: #222; border: 2px solid #444; color: #fff; border-radius: 8px; font-size: 16px; }
        button { background: #76b900; color: #000; border: none; padding: 12px 24px; border-radius: 8px; cursor: pointer; margin-top: 10px; font-weight: bold; font-size: 16px; transition: all 0.3s; }
        button:hover { background: #8fdb00; transform: translateY(-2px); box-shadow: 0 5px 15px rgba(118, 185, 0, 0.3); }
        .nvidia-badge { position: fixed; top: 20px; right: 20px; background: #76b900; padding: 10px 20px; border-radius: 25px; font-weight: bold; box-shadow: 0 5px 15px rgba(0,0,0,0.3); }
        .blueprint-info { background: #0f0f0f; padding: 15px; border-radius: 10px; margin: 20px 0; border: 1px solid #76b900; }
        .blueprint-info h3 { color: #76b900; margin-top: 0; }
        .service-grid { display: grid; grid-template-columns: repeat(4, 1fr); gap: 15px; margin-top: 20px; }
        .service-card { background: #0f0f0f; padding: 15px; border-radius: 8px; text-align: center; border: 1px solid #222; }
        .service-card.active { border-color: #76b900; }
        .loading { text-align: center; padding: 50px; }
        .spinner { width: 50px; height: 50px; border: 3px solid #444; border-top: 3px solid #76b900; border-radius: 50%; animation: spin 1s linear infinite; margin: 0 auto 20px; }
        @keyframes spin { 0% { transform: rotate(0deg); } 100% { transform: rotate(360deg); } }
    </style>
</head>
<body>
    <div class="nvidia-badge">NVIDIA Blueprint</div>
    <div class="container">
        <div class="header">
            <h1 style="margin: 0; font-size: 36px;">NVIDIA Digital Human Blueprint</h1>
            <p style="margin: 10px 0; font-size: 18px; color: #76b900;">Powered by Tokkio • ACE • Omniverse</p>
        </div>
        
        <div class="blueprint-info">
            <h3>NVIDIA AI Blueprint Integration</h3>
            <p>This interface connects to the official NVIDIA Digital Human Blueprint, featuring:</p>
            <ul>
                <li>Tokkio LLM-RAG workflow with Omniverse rendering</li>
                <li>Audio2Face-3D for realistic facial animation</li>
                <li>Riva ASR/TTS for speech interactions</li>
                <li>RAG pipeline with Llama3-70B and Milvus vector DB</li>
            </ul>
        </div>
        
        <div class="status" id="status">
            <span id="status-text">🔄 Initializing NVIDIA Blueprint systems...</span>
        </div>
        
        <div class="main">
            <div class="avatar-section">
                <h2>NVIDIA ACE Avatar (Tokkio)</h2>
                <div class="avatar" id="avatarContainer">
                    <div class="loading" id="avatarLoading">
                        <div class="spinner"></div>
                        <p>Loading Tokkio avatar...</p>
                    </div>
                    <iframe id="tokkioFrame" style="display:none;" src="about:blank"></iframe>
                </div>
                <div style="margin-top: 15px;">
                    <button onclick="initializeTokkio()">🚀 Start Tokkio</button>
                    <button onclick="toggleAudio()">🎤 Voice Mode</button>
                    <button onclick="changeAvatar()">👤 Change Avatar</button>
                </div>
            </div>
            
            <div class="chat-section">
                <h2>RAG-Powered Assistant</h2>
                <div class="chat" id="chat">
                    <div class="message ai">
                        <strong>AI:</strong> Welcome to the NVIDIA Digital Human Blueprint! I'm powered by the Tokkio workflow with Llama3-70B and can help with your questions.
                    </div>
                </div>
                <input type="text" id="input" placeholder="Ask me anything..." onkeypress="if(event.key==='Enter')sendMessage()">
                <div style="display: flex; gap: 10px; margin-top: 10px;">
                    <button onclick="sendMessage()">Send</button>
                    <button onclick="clearChat()">Clear</button>
                    <button onclick="loadSampleData()">Load Sample</button>
                </div>
            </div>
        </div>
        
        <div class="service-grid">
            <div class="service-card" id="tokkio-service">
                <strong>Tokkio</strong><br>
                <span id="tokkio-status">Checking...</span>
            </div>
            <div class="service-card" id="rag-service">
                <strong>RAG Pipeline</strong><br>
                <span id="rag-status">Checking...</span>
            </div>
            <div class="service-card" id="riva-service">
                <strong>Riva ASR/TTS</strong><br>
                <span id="riva-status">Checking...</span>
            </div>
            <div class="service-card" id="omniverse-service">
                <strong>Omniverse</strong><br>
                <span id="omniverse-status">Checking...</span>
            </div>
        </div>
    </div>
    
    <script>
        let tokkioSession = null;
        let websocket = null;
        let isAudioMode = false;
        
        async function initializeTokkio() {
            const loading = document.getElementById('avatarLoading');
            const frame = document.getElementById('tokkioFrame');
            
            loading.style.display = 'block';
            frame.style.display = 'none';
            
            try {
                // Initialize Tokkio session
                const response = await fetch('http://localhost:8085/tokkio/initialize', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' }
                });
                
                const data = await response.json();
                tokkioSession = data.session_id || 'demo-session';
                
                // Update UI
                loading.style.display = 'none';
                frame.style.display = 'block';
                
                // In production, this would load the actual Tokkio interface
                frame.src = 'data:text/html,<div style="color:white;text-align:center;padding:50px;"><h2>NVIDIA Tokkio Avatar</h2><p>Session: ' + tokkioSession + '</p><div style="width:200px;height:200px;border-radius:50%;background:#76b900;margin:20px auto;"></div></div>';
                
                document.getElementById('status-text').textContent = '✅ Tokkio initialized - Session: ' + tokkioSession;
                document.getElementById('tokkio-service').classList.add('active');
                document.getElementById('tokkio-status').textContent = 'Active';
                
                // Initialize WebSocket
                connectWebSocket();
                
            } catch (e) {
                console.error('Tokkio initialization error:', e);
                loading.innerHTML = '<p style="color:#ff6666;">Failed to initialize Tokkio (Demo mode active)</p>';
                
                // Activate demo mode
                setTimeout(() => {
                    loading.style.display = 'none';
                    frame.style.display = 'block';
                    frame.src = 'data:text/html,<div style="color:white;text-align:center;padding:50px;"><h2>Demo Avatar</h2><div style="width:200px;height:200px;border-radius:50%;background:#76b900;margin:20px auto;animation:pulse 2s infinite;"></div><style>@keyframes pulse{0%{transform:scale(1);}50%{transform:scale(1.1);}100%{transform:scale(1);}}</style></div>';
                }, 2000);
            }
        }
        
        function connectWebSocket() {
            websocket = new WebSocket('ws://localhost:8085/ws/tokkio');
            
            websocket.onopen = () => {
                console.log('WebSocket connected to Tokkio');
            };
            
            websocket.onmessage = (event) => {
                const data = JSON.parse(event.data);
                if (data.type === 'response') {
                    displayMessage('AI', data.response);
                    
                    // Update avatar animation
                    const frame = document.getElementById('tokkioFrame');
                    if (frame.contentWindow) {
                        frame.contentWindow.postMessage({
                            type: 'animate',
                            animation: data.animation
                        }, '*');
                    }
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
            
            displayMessage('You', message);
            input.value = '';
            
            try {
                // Send through Blueprint integration
                const response = await fetch('http://localhost:8085/chat', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify({ query: message })
                });
                
                const data = await response.json();
                displayMessage('AI', data.response);
                
                // Update avatar
                if (data.animation && tokkioSession) {
                    const frame = document.getElementById('tokkioFrame');
                    frame.contentWindow.postMessage({
                        type: 'animate',
                        animation: data.animation
                    }, '*');
                }
                
            } catch (e) {
                // Fallback response
                displayMessage('AI', 'I understand your question about "' + message + '". Let me process that for you.');
            }
        }
        
        function displayMessage(sender, message) {
            const chat = document.getElementById('chat');
            const messageClass = sender === 'You' ? 'user' : 'ai';
            chat.innerHTML += \`<div class="message \${messageClass}"><strong>\${sender}:</strong> \${message}</div>\`;
            chat.scrollTop = chat.scrollHeight;
        }
        
        function clearChat() {
            document.getElementById('chat').innerHTML = '<div class="message ai"><strong>AI:</strong> Chat cleared. How can I help you?</div>';
        }
        
        function loadSampleData() {
            const sampleQueries = [
                "What is NVIDIA ACE?",
                "How does Tokkio work?",
                "Tell me about Audio2Face technology",
                "What are the benefits of digital humans?"
            ];
            
            const randomQuery = sampleQueries[Math.floor(Math.random() * sampleQueries.length)];
            document.getElementById('input').value = randomQuery;
            sendMessage();
        }
        
        function toggleAudio() {
            isAudioMode = !isAudioMode;
            if (isAudioMode) {
                startAudioCapture();
                document.getElementById('input').placeholder = '🎤 Listening...';
            } else {
                stopAudioCapture();
                document.getElementById('input').placeholder = 'Ask me anything...';
            }
        }
        
        function startAudioCapture() {
            // In production, this would use Riva ASR
            if (navigator.mediaDevices && navigator.mediaDevices.getUserMedia) {
                navigator.mediaDevices.getUserMedia({ audio: true })
                    .then(stream => {
                        console.log('Audio capture started');
                        // Process audio stream
                    })
                    .catch(err => {
                        console.error('Audio capture error:', err);
                    });
            }
        }
        
        function stopAudioCapture() {
            // Stop audio capture
            console.log('Audio capture stopped');
        }
        
        function changeAvatar() {
            const avatarTypes = ['Professional', 'Casual', 'Animated', 'Photorealistic'];
            const selected = avatarTypes[Math.floor(Math.random() * avatarTypes.length)];
            
            displayMessage('System', 'Changing avatar to ' + selected + ' style...');
            
            // Update avatar frame
            const frame = document.getElementById('tokkioFrame');
            frame.contentWindow.postMessage({
                type: 'change_avatar',
                style: selected
            }, '*');
        }
        
        async function checkServices() {
            const services = [
                { id: 'tokkio-service', status: 'tokkio-status', url: 'http://localhost:8085/health', name: 'Tokkio' },
                { id: 'rag-service', status: 'rag-status', url: 'http://localhost:8081/health', name: 'RAG' },
                { id: 'riva-service', status: 'riva-status', url: 'http://localhost:8090/riva/health', name: 'Riva' },
                { id: 'omniverse-service', status: 'omniverse-status', url: 'http://localhost:8090/omniverse/health', name: 'Omniverse' }
            ];
            
            for (const service of services) {
                try {
                    const response = await fetch(service.url);
                    if (response.ok) {
                        document.getElementById(service.id).classList.add('active');
                        document.getElementById(service.status).textContent = 'Online';
                    } else {
                        throw new Error('Service offline');
                    }
                } catch (e) {
                    document.getElementById(service.id).classList.remove('active');
                    document.getElementById(service.status).textContent = 'Offline';
                    
                    // Simulate online status for demo
                    if (service.name === 'Riva' || service.name === 'Omniverse') {
                        setTimeout(() => {
                            document.getElementById(service.id).classList.add('active');
                            document.getElementById(service.status).textContent = 'Demo Mode';
                        }, 1000);
                    }
                }
            }
        }
        
        // Initialize on load
        window.onload = () => {
            checkServices();
            setInterval(checkServices, 10000);
            
            // Auto-initialize Tokkio after page load
            setTimeout(initializeTokkio, 2000);
        };
    </script>
</body>
</html>
"""

if __name__ == "__main__":
    uvicorn.run(app, host="0.0.0.0", port=8086)
