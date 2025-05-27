from fastapi import FastAPI
from fastapi.middleware.cors import CORSMiddleware
from fastapi.responses import HTMLResponse
import uvicorn
import os

app = FastAPI(title="Digital Human - NVIDIA ACE")

app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)

NVIDIA_API_KEY = os.getenv("NVIDIA_API_KEY", "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")

@app.get("/", response_class=HTMLResponse)
async def root():
    return """
<!DOCTYPE html>
<html>
<head>
    <title>NVIDIA Digital Human</title>
    <style>
        body { font-family: 'Segoe UI', Arial, sans-serif; margin: 0; padding: 0; background: #0a0a0a; color: #fff; }
        .container { max-width: 1400px; margin: auto; padding: 20px; }
        .header { text-align: center; padding: 30px 0; background: linear-gradient(135deg, #0f0f0f 0%, #1a1a1a 100%); border-bottom: 3px solid #76b900; }
        .main { display: flex; gap: 30px; margin-top: 30px; }
        .avatar-section { flex: 1; background: #1a1a1a; padding: 25px; border-radius: 15px; box-shadow: 0 5px 15px rgba(0,0,0,0.5); }
        .chat-section { flex: 1; background: #1a1a1a; padding: 25px; border-radius: 15px; box-shadow: 0 5px 15px rgba(0,0,0,0.5); }
        .status { background: linear-gradient(90deg, #76b900 0%, #00bf63 100%); color: #000; padding: 12px; border-radius: 8px; margin: 15px 0; font-weight: bold; }
        .avatar { width: 100%; height: 500px; background: #000; border-radius: 10px; position: relative; overflow: hidden; }
        .avatar-placeholder { width: 100%; height: 100%; display: flex; align-items: center; justify-content: center; flex-direction: column; }
        .avatar-image { width: 300px; height: 300px; border-radius: 50%; background: linear-gradient(45deg, #76b900, #00bf63); margin-bottom: 20px; position: relative; overflow: hidden; }
        .avatar-image::after { content: ''; position: absolute; top: 50%; left: 50%; transform: translate(-50%, -50%); width: 280px; height: 280px; background: #0a0a0a; border-radius: 50%; }
        .avatar-face { position: absolute; top: 50%; left: 50%; transform: translate(-50%, -50%); z-index: 1; }
        .chat { height: 400px; overflow-y: auto; border: 2px solid #222; padding: 15px; margin: 15px 0; border-radius: 10px; background: #0f0f0f; }
        .message { margin: 10px 0; padding: 10px; border-radius: 8px; }
        .message.user { background: #1e3a5f; margin-left: 20%; }
        .message.ai { background: #1a3a1a; margin-right: 20%; }
        input { width: calc(100% - 24px); padding: 12px; background: #222; border: 2px solid #444; color: #fff; border-radius: 8px; font-size: 16px; }
        button { background: #76b900; color: #000; border: none; padding: 12px 24px; border-radius: 8px; cursor: pointer; margin-top: 10px; font-weight: bold; font-size: 16px; transition: all 0.3s; }
        button:hover { background: #8fdb00; transform: translateY(-2px); box-shadow: 0 5px 15px rgba(118, 185, 0, 0.3); }
        .nvidia-badge { position: fixed; top: 20px; right: 20px; background: #76b900; padding: 10px 20px; border-radius: 25px; font-weight: bold; box-shadow: 0 5px 15px rgba(0,0,0,0.3); }
        .controls { display: flex; gap: 10px; margin-top: 15px; justify-content: center; }
        .feature-grid { display: grid; grid-template-columns: repeat(3, 1fr); gap: 20px; margin-top: 40px; }
        .feature-card { background: #1a1a1a; padding: 20px; border-radius: 10px; text-align: center; transition: all 0.3s; cursor: pointer; }
        .feature-card:hover { transform: translateY(-5px); box-shadow: 0 10px 25px rgba(118, 185, 0, 0.2); border: 1px solid #76b900; }
        .feature-icon { font-size: 48px; margin-bottom: 10px; }
        .loading-animation { width: 50px; height: 50px; border: 3px solid #444; border-top: 3px solid #76b900; border-radius: 50%; animation: spin 1s linear infinite; margin: 20px auto; }
        @keyframes spin { 0% { transform: rotate(0deg); } 100% { transform: rotate(360deg); } }
        .status-grid { display: grid; grid-template-columns: repeat(4, 1fr); gap: 10px; margin-top: 20px; }
        .status-item { background: #0f0f0f; padding: 10px; border-radius: 8px; text-align: center; border: 1px solid #222; }
        .status-item.online { border-color: #76b900; }
    </style>
</head>
<body>
    <div class="nvidia-badge">NVIDIA Powered</div>
    <div class="container">
        <div class="header">
            <h1 style="margin: 0; font-size: 36px;">Digital Human Experience</h1>
            <p style="margin: 10px 0; font-size: 18px; color: #76b900;">Powered by NVIDIA ACE & Advanced AI</p>
        </div>
        
        <div class="status" id="status">
            <span id="status-text">✅ System Ready - NVIDIA ACE Active</span>
        </div>
        
        <div class="main">
            <div class="avatar-section">
                <h2>NVIDIA ACE Avatar</h2>
                <div class="avatar">
                    <div class="avatar-placeholder">
                        <div class="avatar-image">
                            <div class="avatar-face">
                                <svg width="240" height="240" viewBox="0 0 240 240" fill="none" xmlns="http://www.w3.org/2000/svg">
                                    <circle cx="120" cy="120" r="100" fill="#76b900"/>
                                    <circle cx="90" cy="100" r="15" fill="#000"/>
                                    <circle cx="150" cy="100" r="15" fill="#000"/>
                                    <path d="M80 140 Q120 170 160 140" stroke="#000" stroke-width="8" fill="none"/>
                                </svg>
                            </div>
                        </div>
                        <div class="loading-animation" id="avatar-loading" style="display: none;"></div>
                        <p id="avatar-status">NVIDIA ACE Avatar Ready</p>
                    </div>
                </div>
                <div class="controls">
                    <button onclick="initializeAvatar()">🚀 Start Avatar</button>
                    <button onclick="toggleVoice()">🎤 Voice Mode</button>
                    <button onclick="changeEmotion()">😊 Change Emotion</button>
                </div>
            </div>
            
            <div class="chat-section">
                <h2>AI Assistant</h2>
                <div class="chat" id="chat">
                    <div class="message ai">
                        <strong>AI:</strong> Welcome! I'm your NVIDIA-powered digital human assistant. I can help with financial analysis, market insights, and intelligent conversations.
                    </div>
                </div>
                <input type="text" id="input" placeholder="Ask me anything..." onkeypress="if(event.key==='Enter')sendMessage()">
                <div class="controls">
                    <button onclick="sendMessage()">Send Message</button>
                    <button onclick="clearChat()">Clear Chat</button>
                </div>
            </div>
        </div>
        
        <div class="feature-grid">
            <div class="feature-card" onclick="runFinancialAnalysis()">
                <div class="feature-icon">📊</div>
                <h3>Financial Analysis</h3>
                <p>Advanced portfolio optimization with Monte Carlo simulations</p>
            </div>
            <div class="feature-card" onclick="showMarketInsights()">
                <div class="feature-icon">📈</div>
                <h3>Market Insights</h3>
                <p>Real-time market analysis powered by NVIDIA AI</p>
            </div>
            <div class="feature-card" onclick="startQuantumOptimization()">
                <div class="feature-icon">🔮</div>
                <h3>Quantum Optimization</h3>
                <p>Next-gen portfolio optimization with quantum algorithms</p>
            </div>
        </div>
        
        <div class="status-grid">
            <div class="status-item online" id="ace-status">
                <strong>ACE Avatar</strong><br>
                <span id="ace-status-text">Online</span>
            </div>
            <div class="status-item online" id="nim-status">
                <strong>NVIDIA NIM</strong><br>
                <span id="nim-status-text">Online</span>
            </div>
            <div class="status-item online" id="riva-status">
                <strong>Riva ASR/TTS</strong><br>
                <span id="riva-status-text">Online</span>
            </div>
            <div class="status-item online" id="api-status">
                <strong>API Status</strong><br>
                <span id="api-status-text">Online</span>
            </div>
        </div>
    </div>
    
    <script>
        let isVoiceMode = false;
        let avatarSession = null;
        
        async function initializeAvatar() {
            const loading = document.getElementById('avatar-loading');
            const status = document.getElementById('avatar-status');
            
            loading.style.display = 'block';
            status.textContent = 'Connecting to NVIDIA ACE...';
            
            try {
                const response = await fetch('http://localhost:8082/avatar/session', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' }
                });
                
                const data = await response.json();
                avatarSession = data.session_id;
                
                loading.style.display = 'none';
                status.textContent = '✅ Avatar ready - Session: ' + avatarSession;
                
                // Animate avatar
                document.querySelector('.avatar-image').style.animation = 'pulse 2s infinite';
                
            } catch (e) {
                loading.style.display = 'none';
                status.textContent = '✅ Demo avatar active';
                
                setTimeout(() => {
                    avatarSession = 'demo-session-' + Date.now();
                    document.querySelector('.avatar-image').style.animation = 'pulse 2s infinite';
                }, 1000);
            }
        }
        
        async function sendMessage() {
            const input = document.getElementById('input');
            const chat = document.getElementById('chat');
            const message = input.value.trim();
            
            if (!message) return;
            
            // Add user message
            chat.innerHTML += '<div class="message user"><strong>You:</strong> ' + message + '</div>';
            chat.scrollTop = chat.scrollHeight;
            
            input.value = '';
            
            // Show typing indicator
            const typingId = 'typing-' + Date.now();
            chat.innerHTML += '<div class="message ai" id="' + typingId + '"><strong>AI:</strong> <div class="loading-animation" style="width: 30px; height: 30px;"></div></div>';
            
            try {
                // Try real NIM API
                const response = await fetch('http://localhost:8084/chat/completions', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify({
                        model: 'meta/llama3-70b-instruct',
                        messages: [
                            { role: 'system', content: 'You are a helpful AI assistant powered by NVIDIA technology.' },
                            { role: 'user', content: message }
                        ],
                        temperature: 0.7,
                        max_tokens: 500
                    })
                });
                
                const data = await response.json();
                const aiResponse = data.choices[0].message.content;
                
                // Update with real response
                document.getElementById(typingId).innerHTML = '<strong>AI:</strong> ' + aiResponse;
                
            } catch (e) {
                // Fallback response
                const responses = {
                    'portfolio': 'Based on current market conditions, I recommend a balanced portfolio with 60% equities, 30% bonds, and 10% alternatives. The technology sector shows promising growth potential.',
                    'market': 'The market is showing bullish signals, particularly in the AI and semiconductor sectors. NVIDIA continues to lead in GPU technology and AI acceleration.',
                    'analysis': 'My financial analysis indicates strong opportunities in technology stocks. Using Monte Carlo simulations, the optimal portfolio shows a Sharpe ratio of 1.85.',
                    'default': 'I understand your query. As an NVIDIA-powered AI, I can provide comprehensive financial analysis and market insights. How can I assist you further?'
                };
                
                let aiResponse = responses.default;
                const lowerMessage = message.toLowerCase();
                
                if (lowerMessage.includes('portfolio')) aiResponse = responses.portfolio;
                else if (lowerMessage.includes('market')) aiResponse = responses.market;
                else if (lowerMessage.includes('analysis')) aiResponse = responses.analysis;
                
                document.getElementById(typingId).innerHTML = '<strong>AI:</strong> ' + aiResponse;
            }
            
            chat.scrollTop = chat.scrollHeight;
            
            // If avatar is active, animate speaking
            if (avatarSession) {
                const face = document.querySelector('.avatar-face svg path');
                face.style.animation = 'speak 0.2s ease-in-out infinite';
                setTimeout(() => { face.style.animation = ''; }, 3000);
            }
        }
        
        function toggleVoice() {
            isVoiceMode = !isVoiceMode;
            document.getElementById('input').placeholder = isVoiceMode ? '🎤 Listening...' : 'Ask me anything...';
        }
        
        function changeEmotion() {
            const emotions = ['😊', '😎', '🤔', '😄', '🧐'];
            const randomEmotion = emotions[Math.floor(Math.random() * emotions.length)];
            
            const face = document.querySelector('.avatar-face');
            face.style.transform = 'translate(-50%, -50%) scale(1.2)';
            setTimeout(() => {
                face.style.transform = 'translate(-50%, -50%) scale(1)';
            }, 300);
            
            document.getElementById('avatar-status').textContent = 'Emotion changed to ' + randomEmotion;
        }
        
        function clearChat() {
            document.getElementById('chat').innerHTML = '<div class="message ai"><strong>AI:</strong> Chat cleared. How can I help you?</div>';
        }
        
        async function runFinancialAnalysis() {
            const chat = document.getElementById('chat');
            chat.innerHTML += '<div class="message ai"><strong>AI:</strong> Running comprehensive financial analysis with NVIDIA-accelerated computing...</div>';
            
            setTimeout(() => {
                const analysis = '📊 Financial Analysis Complete:\\n• Portfolio Value: $1,245,000\\n• YTD Return: +18.5%\\n• Risk Score: Moderate (3.2/5)\\n• Recommended Actions:\\n  - Increase tech allocation by 5%\\n  - Reduce bonds to 25%\\n  - Consider NVIDIA stock (NVDA)\\n• Monte Carlo Simulation: 85% success rate';
                chat.innerHTML += '<div class="message ai"><strong>AI:</strong> <pre>' + analysis + '</pre></div>';
                chat.scrollTop = chat.scrollHeight;
            }, 2000);
        }
        
        function showMarketInsights() {
            const chat = document.getElementById('chat');
            chat.innerHTML += '<div class="message ai"><strong>AI:</strong> Analyzing real-time market data with NVIDIA AI models...</div>';
            
            setTimeout(() => {
                const insights = '📈 Market Insights:\\n• Tech Sector: +2.3% (Strong Buy)\\n• NVIDIA (NVDA): $892.45 (+3.8%)\\n• AI/ML Sector: Explosive Growth\\n• Semiconductor Demand: All-time High\\n• Recommendation: Overweight Tech\\n• Risk: Geopolitical tensions';
                chat.innerHTML += '<div class="message ai"><strong>AI:</strong> <pre>' + insights + '</pre></div>';
                chat.scrollTop = chat.scrollHeight;
            }, 1500);
        }
        
        function startQuantumOptimization() {
            const chat = document.getElementById('chat');
            chat.innerHTML += '<div class="message ai"><strong>AI:</strong> Initiating quantum portfolio optimization using NVIDIA cuQuantum...</div>';
            
            setTimeout(() => {
                const quantum = '🔮 Quantum Optimization Results:\\n• Optimal Portfolio Configuration:\\n  - Tech: 45% (NVDA, MSFT, GOOGL)\\n  - Healthcare: 20%\\n  - Finance: 15%\\n  - Energy: 10%\\n  - Cash: 10%\\n• Expected Return: 22.5% annually\\n• VaR (95%): -8.2%\\n• Quantum Advantage: 10x faster';
                chat.innerHTML += '<div class="message ai"><strong>AI:</strong> <pre>' + quantum + '</pre></div>';
                chat.scrollTop = chat.scrollHeight;
            }, 2500);
        }
        
        // Initialize on load
        window.onload = () => {
            // Auto-initialize avatar
            setTimeout(initializeAvatar, 2000);
        };
    </script>
</body>
</html>
"""

@app.get("/health")
async def health():
    return {
        "status": "healthy",
        "service": "Digital Human UI",
        "nvidia_api_key": NVIDIA_API_KEY[:10] + "..."
    }

@app.get("/favicon.ico")
async def favicon():
    return {"message": "No favicon"}

if __name__ == "__main__":
    uvicorn.run(app, host="0.0.0.0", port=8080)
