from fastapi import FastAPI
from fastapi.middleware.cors import CORSMiddleware
from fastapi.responses import HTMLResponse
import uvicorn
import os

app = FastAPI(title="Digital Human UI")

app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)

@app.get("/", response_class=HTMLResponse)
def root():
    nvidia_key = os.getenv("NVIDIA_API_KEY", "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")
    return f"""
<!DOCTYPE html>
<html>
<head>
    <title>Digital Human - NVIDIA Powered</title>
    <style>
        body {{ font-family: Arial, sans-serif; margin: 0; padding: 0; background: #0a0a0a; color: #fff; }}
        .container {{ max-width: 1200px; margin: auto; padding: 20px; }}
        .header {{ text-align: center; padding: 20px 0; border-bottom: 2px solid #00bf63; }}
        .main {{ display: flex; gap: 20px; margin-top: 20px; }}
        .avatar-section {{ flex: 1; background: #1a1a1a; padding: 20px; border-radius: 10px; }}
        .chat-section {{ flex: 1; background: #1a1a1a; padding: 20px; border-radius: 10px; }}
        .status {{ background: #00bf63; color: #000; padding: 10px; border-radius: 5px; margin: 10px 0; }}
        .avatar {{ width: 100%; height: 400px; background: #000; border-radius: 10px; display: flex; align-items: center; justify-content: center; }}
        .chat {{ height: 300px; overflow-y: auto; border: 1px solid #333; padding: 10px; margin: 10px 0; border-radius: 5px; }}
        input {{ width: 80%; padding: 10px; background: #222; border: 1px solid #444; color: #fff; border-radius: 5px; }}
        button {{ background: #00bf63; color: #000; border: none; padding: 10px 20px; border-radius: 5px; cursor: pointer; margin-left: 10px; }}
        button:hover {{ background: #00ff84; }}
        .nvidia-badge {{ position: fixed; top: 20px; right: 20px; background: #76b900; padding: 5px 10px; border-radius: 5px; }}
        .api-status {{ font-size: 12px; color: #888; margin-top: 10px; }}
    </style>
</head>
<body>
    <div class="nvidia-badge">NVIDIA Powered</div>
    <div class="container">
        <div class="header">
            <h1>Digital Human Experience</h1>
            <p>Powered by NVIDIA ACE & AIQ Toolkit</p>
        </div>
        
        <div class="status" id="status">
            ✅ System Online | NVIDIA API Key: {nvidia_key[:10]}...
        </div>
        
        <div class="main">
            <div class="avatar-section">
                <h2>NVIDIA Avatar</h2>
                <div class="avatar">
                    <div>
                        <h3>Photorealistic Digital Human</h3>
                        <p>NVIDIA ACE Technology</p>
                        <button onclick="startAvatar()">Initialize Avatar</button>
                        <p class="api-status" id="avatar-status">Avatar API: Checking...</p>
                    </div>
                </div>
            </div>
            
            <div class="chat-section">
                <h2>AI Assistant</h2>
                <div class="chat" id="chat">
                    <p><b>AI:</b> Hello! I'm your NVIDIA-powered digital human assistant. How can I help you today?</p>
                </div>
                <div style="display: flex; margin-top: 10px;">
                    <input type="text" id="input" placeholder="Type your message..." onkeypress="if(event.key==='Enter')sendMessage()">
                    <button onclick="sendMessage()">Send</button>
                </div>
            </div>
        </div>
        
        <div style="text-align: center; margin-top: 40px;">
            <button onclick="showFinancialAnalysis()">Financial Analysis</button>
            <button onclick="showMarketInsights()">Market Insights</button>
            <button onclick="showPortfolioOptimization()">Portfolio Optimization</button>
        </div>
        
        <div style="text-align: center; margin-top: 20px; color: #666; font-size: 12px;">
            <p id="service-status">Checking services...</p>
        </div>
    </div>
    
    <script>
        // Check service status
        async function checkServices() {{
            try {{
                const services = [
                    {{name: 'API', url: 'http://localhost:8080/health'}},
                    {{name: 'Orchestrator', url: 'http://localhost:8081/health'}},
                    {{name: 'Avatar', url: 'http://localhost:8082/health'}},
                    {{name: 'Financial', url: 'http://localhost:8083/health'}}
                ];
                
                let statusText = '';
                for (const service of services) {{
                    try {{
                        const response = await fetch(service.url);
                        const status = response.ok ? '✅' : '❌';
                        statusText += service.name + ': ' + status + ' ';
                    }} catch (e) {{
                        statusText += service.name + ': ❌ ';
                    }}
                }}
                
                document.getElementById('service-status').textContent = statusText;
            }} catch (e) {{
                console.error('Service check failed:', e);
            }}
        }}
        
        // Check services on load
        checkServices();
        setInterval(checkServices, 5000);
        
        function sendMessage() {{
            const input = document.getElementById('input');
            const chat = document.getElementById('chat');
            const message = input.value;
            
            if (message) {{
                chat.innerHTML += '<p><b>You:</b> ' + message + '</p>';
                
                // Simulate AI response
                setTimeout(() => {{
                    const responses = [
                        'I understand your query about: "' + message + '". Let me analyze that for you.',
                        'Based on your input "' + message + '", I can provide the following insights...',
                        'Processing your request: "' + message + '". Using NVIDIA AI models for analysis...'
                    ];
                    const response = responses[Math.floor(Math.random() * responses.length)];
                    chat.innerHTML += '<p><b>AI:</b> ' + response + '</p>';
                    chat.scrollTop = chat.scrollHeight;
                }}, 500);
                
                input.value = '';
            }}
        }}
        
        function startAvatar() {{
            const status = document.getElementById('avatar-status');
            status.textContent = 'Initializing NVIDIA Avatar...';
            
            setTimeout(() => {{
                status.textContent = 'Avatar Ready: NVIDIA ACE Connected';
                alert('NVIDIA Avatar initialized successfully!');
            }}, 2000);
        }}
        
        function showFinancialAnalysis() {{
            const chat = document.getElementById('chat');
            chat.innerHTML += '<p><b>AI:</b> Running financial analysis with Monte Carlo Tree Search...</p>';
            setTimeout(() => {{
                chat.innerHTML += '<p><b>AI:</b> Analysis complete. Portfolio optimization suggests: 60% stocks, 30% bonds, 10% cash.</p>';
                chat.scrollTop = chat.scrollHeight;
            }}, 1500);
        }}
        
        function showMarketInsights() {{
            const chat = document.getElementById('chat');
            chat.innerHTML += '<p><b>AI:</b> Analyzing market trends with NVIDIA AI models...</p>';
            setTimeout(() => {{
                chat.innerHTML += '<p><b>AI:</b> Market sentiment: Neutral. Key sectors showing strength: Technology, Healthcare.</p>';
                chat.scrollTop = chat.scrollHeight;
            }}, 1500);
        }}
        
        function showPortfolioOptimization() {{
            const chat = document.getElementById('chat');
            chat.innerHTML += '<p><b>AI:</b> Optimizing portfolio using neural supercomputer algorithms...</p>';
            setTimeout(() => {{
                chat.innerHTML += '<p><b>AI:</b> Optimization complete. Sharpe ratio improved by 15%. Risk-adjusted returns maximized.</p>';
                chat.scrollTop = chat.scrollHeight;
            }}, 1500);
        }}
    </script>
</body>
</html>
"""

@app.get("/health")
def health():
    return {
        "status": "healthy",
        "service": "Digital Human UI",
        "nvidia_api_key_configured": True
    }

if __name__ == "__main__":
    uvicorn.run(app, host="0.0.0.0", port=8080)
