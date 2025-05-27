#!/usr/bin/env python3
"""
Simple Digital Human Server
Creates a basic digital human with NVIDIA integration
"""

import os
import uvicorn
from fastapi import FastAPI, WebSocket, WebSocketDisconnect
from fastapi.middleware.cors import CORSMiddleware
from fastapi.responses import FileResponse
from typing import Dict, Any
import json

# Set NVIDIA API key
os.environ["NVIDIA_API_KEY"] = "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"

app = FastAPI(title="Digital Human Server")

# Add CORS
app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)

# Simple digital human state
digital_human_state = {
    "name": "Aria",
    "role": "Financial Advisor",
    "emotion": "professional",
    "avatar_type": "2d-photorealistic",
    "nvidia_enabled": True
}

@app.get("/")
async def root():
    return FileResponse("ui/frontend/digital_human_interface.html")

@app.get("/health")
async def health():
    return {
        "status": "healthy",
        "digital_human": digital_human_state,
        "nvidia_api_configured": bool(os.getenv("NVIDIA_API_KEY"))
    }

@app.post("/chat")
async def chat(message: Dict[str, Any]):
    text = message.get("message", "")
    
    # Simple response for testing
    response = {
        "text": f"I understand you said: '{text}'. As your financial advisor, I can help with investment strategies, portfolio management, and financial planning.",
        "emotion": "professional",
        "avatar": {
            "expression": "speaking",
            "mouth_shape": "neutral"
        }
    }
    
    # Add financial context
    if "invest" in text.lower():
        response["text"] = "Based on your risk profile, I recommend a diversified portfolio with 60% stocks, 30% bonds, and 10% alternative investments."
        response["emotion"] = "confident"
    elif "portfolio" in text.lower():
        response["text"] = "Your current portfolio shows strong performance. Would you like me to analyze specific sectors?"
        response["emotion"] = "analytical"
    
    return response

@app.websocket("/ws")
async def websocket_endpoint(websocket: WebSocket):
    await websocket.accept()
    
    try:
        # Send initial greeting
        await websocket.send_json({
            "type": "greeting",
            "text": "Hello! I'm Aria, your AI financial advisor. How can I help you today?",
            "emotion": "welcoming",
            "avatar": {
                "expression": "smile",
                "gesture": "wave"
            }
        })
        
        while True:
            data = await websocket.receive_text()
            message = json.loads(data)
            
            # Process message
            response = await chat({"message": message.get("text", "")})
            
            # Send response
            await websocket.send_json({
                "type": "response",
                **response
            })
            
    except WebSocketDisconnect:
        print("Client disconnected")

@app.get("/avatar/config")
async def get_avatar_config():
    return {
        "model": "digital-human-2d",
        "nvidia_ace_enabled": True,
        "features": {
            "facial_animation": True,
            "lip_sync": True,
            "emotion_mapping": True,
            "gesture_generation": True
        },
        "resolution": [1920, 1080],
        "fps": 60
    }

if __name__ == "__main__":
    print("Starting Digital Human Server...")
    print("NVIDIA API Key configured:", bool(os.getenv("NVIDIA_API_KEY")))
    print("Access at: http://localhost:8000")
    
    uvicorn.run(app, host="0.0.0.0", port=8000)