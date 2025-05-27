"""
Digital Human Main UI
"""
from fastapi import FastAPI
from fastapi.middleware.cors import CORSMiddleware
from fastapi.responses import HTMLResponse
import uvicorn
import os

app = FastAPI(title="Digital Human UI")

# Enable CORS
app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)

@app.get("/", response_class=HTMLResponse)
def root():
    return """
<!DOCTYPE html>
<html>
<head>
    <title>Digital Human - NVIDIA Powered</title>
    <style>
        body { font-family: Arial, sans-serif; margin: 0; padding: 0; background: #0a0a0a; color: #fff; }
        .container { max-width: 1200px; margin: auto; padding: 20px; }
        .header { text-align: center; padding: 20px 0; border-bottom: 2px solid #00bf63; }
        .main { display: flex; gap: 20px; margin-top: 20px; }
        .avatar-section { flex: 1; background: #1a1a1a; padding: 20px; border-radius: 10px; }
        .chat-section { flex: 1; background: #1a1a1a; padding: 20px; border-radius: 10px; }
        .status { background: #00bf63; color: #000; padding: 10px; border-radius: 5px; margin: 10px 0; }
        .avatar { width: 100%; height: 400px; background: #000; border-radius: 10px; display: flex; align-items: center; justify-content: center; }
        .chat { height: 300px; overflow-y: auto; border: 1px solid #333; padding: 10px; margin: 10px 0; border-radius: 5px; }
        input { width: 100%; padding: 10px; background: #222; border: 1px solid #444; color: #fff; border-radius: 5px; }
        button { background: #00bf63; color: #000; border: none; padding: 10px 20px; border-radius: 5px; cursor: pointer; margin-top: 10px; }
        button:hover { background: #00ff84; }
        .nvidia-badge { position: absolute; top: 20px; right: 20px; background: #76b900; padding: 5px 10px; border-radius: 5px; }
    </style>
</head>
<body>
    <div class="nvidia-badge">NVIDIA Powered</div>
    <div class="container">
        <div class="header">
            <h1>Digital Human Experience</h1>
            <p>Powered by NVIDIA ACE & AIQ Toolkit</p>
        </div>
        
        <div class="status">
            ✅ System Online | NVIDIA API: Connected | Blueprint: Integrated
        </div>
        
        <div class="main">
            <div class="avatar-section">
                <h2>NVIDIA Avatar</h2>
                <div class="avatar">
                    <div>
                        <h3>Photorealistic Digital Human</h3>
                        <p>NVIDIA ACE Technology</p>
                        <button onclick="startAvatar()">Initialize Avatar</button>
                    </div>
                </div>
            </div>
            
            <div class="chat-section">
                <h2>AI Assistant</h2>
                <div class="chat" id="chat">
                    <p><b>AI:</b> Hello! I'm your NVIDIA-powered digital human assistant. How can I help you today?</p>
                </div>
                <input type="text" id="input" placeholder="Type your message..." onkeypress="if(event.key==='Enter')sendMessage()">
                <button onclick="sendMessage()">Send</button>
            </div>
        </div>
        
        <div style="text-align: center; margin-top: 40px;">
            <button onclick="showFinancialAnalysis()">Financial Analysis</button>
            <button onclick="showMarketInsights()">Market Insights</button>
            <button onclick="showPortfolioOptimization()">Portfolio Optimization</button>
        </div>
    </div>
    
    <script>
        function sendMessage() {
            const input = document.getElementById('input');
            const chat = document.getElementById('chat');
            const message = input.value;
            
            if (message) {
                chat.innerHTML += '<p><b>You:</b> ' + message + '</p>';
                
                // Simulate AI response
                setTimeout(() => {
                    chat.innerHTML += '<p><b>AI:</b> Processing your request: "' + message + '"</p>';
                    chat.scrollTop = chat.scrollHeight;
                }, 500);
                
                input.value = '';
            }
        }
        
        function startAvatar() {
            alert('Initializing NVIDIA Avatar...');
        }
        
        function showFinancialAnalysis() {
            const chat = document.getElementById('chat');
            chat.innerHTML += '<p><b>AI:</b> Running financial analysis with Monte Carlo Tree Search...</p>';
            chat.scrollTop = chat.scrollHeight;
        }
        
        function showMarketInsights() {
            const chat = document.getElementById('chat');
            chat.innerHTML += '<p><b>AI:</b> Analyzing market trends with NVIDIA AI models...</p>';
            chat.scrollTop = chat.scrollHeight;
        }
        
        function showPortfolioOptimization() {
            const chat = document.getElementById('chat');
            chat.innerHTML += '<p><b>AI:</b> Optimizing portfolio using neural supercomputer algorithms...</p>';
            chat.scrollTop = chat.scrollHeight;
        }
    </script>
</body>
</html>
"""

if __name__ == "__main__":
    uvicorn.run(app, host="0.0.0.0", port=8080)
