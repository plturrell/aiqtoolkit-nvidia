#!/usr/bin/env python3
"""
AIQToolkit-compatible backend server
Provides the necessary endpoints for the AIQToolkit UI
"""

import uvicorn
import json
import time
import asyncio
from typing import Dict, List, Optional
from fastapi import FastAPI, WebSocket, WebSocketDisconnect, HTTPException
from fastapi.middleware.cors import CORSMiddleware
from pydantic import BaseModel
from datetime import datetime

# Data models
class ChatMessage(BaseModel):
    role: str
    content: str

class ChatRequest(BaseModel):
    model: Optional[str] = "aiq-toolkit"
    messages: List[ChatMessage]
    stream: Optional[bool] = False
    temperature: Optional[float] = 0.7

class GenerateRequest(BaseModel):
    prompt: str
    model: Optional[str] = "aiq-toolkit"
    stream: Optional[bool] = False

class WorkflowRequest(BaseModel):
    workflow_id: str
    inputs: Dict
    
# Create FastAPI app
app = FastAPI(
    title="AIQToolkit Backend",
    description="AIQToolkit-compatible API server",
    version="1.0.0"
)

# Add CORS middleware
app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)

# Health endpoint
@app.get("/health")
async def health():
    return {"status": "healthy", "service": "aiqtoolkit-backend"}

# Chat completion endpoint (OpenAI-style)
@app.post("/v1/chat/completions")
async def chat_completions(request: ChatRequest):
    # Simulate processing
    await asyncio.sleep(0.1)
    
    # Generate a simple response
    response_content = f"I received your message: '{request.messages[-1].content}'. This is a test response from AIQToolkit backend."
    
    return {
        "id": f"chat-{int(time.time())}",
        "object": "chat.completion",
        "created": int(time.time()),
        "model": request.model,
        "choices": [{
            "index": 0,
            "message": {
                "role": "assistant",
                "content": response_content
            },
            "finish_reason": "stop"
        }],
        "usage": {
            "prompt_tokens": 10,
            "completion_tokens": 15,
            "total_tokens": 25
        }
    }

# Generate endpoint
@app.post("/v1/generate")
async def generate(request: GenerateRequest):
    # Simulate processing
    await asyncio.sleep(0.1)
    
    response = f"Generated from prompt: '{request.prompt}'"
    
    return {
        "id": f"gen-{int(time.time())}",
        "object": "generation",
        "created": int(time.time()),
        "model": request.model,
        "text": response,
        "usage": {
            "prompt_tokens": 5,
            "completion_tokens": 10,
            "total_tokens": 15
        }
    }

# Workflow run endpoint
@app.post("/v1/workflows/run")
async def run_workflow(request: WorkflowRequest):
    # Simulate workflow execution
    await asyncio.sleep(0.1)
    
    return {
        "workflow_id": request.workflow_id,
        "status": "completed",
        "outputs": {
            "result": f"Workflow {request.workflow_id} completed with inputs: {request.inputs}"
        },
        "duration_ms": 100
    }

# Streaming endpoint
@app.post("/v1/stream")
async def stream(request: ChatRequest):
    # For now, redirect to non-streaming endpoint
    return await chat_completions(request)

# WebSocket endpoint
@app.websocket("/v1/ws")
async def websocket_endpoint(websocket: WebSocket):
    await websocket.accept()
    try:
        while True:
            # Receive message from client
            data = await websocket.receive_json()
            
            # Handle different message types
            message_type = data.get("type", "chat")
            
            if message_type == "chat":
                # Send typing indicator
                await websocket.send_json({
                    "type": "typing",
                    "status": "started"
                })
                
                # Simulate processing
                await asyncio.sleep(0.5)
                
                # Send response
                await websocket.send_json({
                    "type": "message",
                    "role": "assistant",
                    "content": f"WebSocket response: {data.get('content', 'No content')}",
                    "timestamp": datetime.now().isoformat()
                })
                
                # Send typing stopped
                await websocket.send_json({
                    "type": "typing",
                    "status": "stopped"
                })
            
            elif message_type == "ping":
                await websocket.send_json({
                    "type": "pong",
                    "timestamp": datetime.now().isoformat()
                })
                
    except WebSocketDisconnect:
        print("Client disconnected")
    except Exception as e:
        print(f"WebSocket error: {e}")

# Models endpoint
@app.get("/v1/models")
async def list_models():
    return {
        "models": [
            {
                "id": "aiq-toolkit",
                "object": "model",
                "owned_by": "aiqtoolkit",
                "permission": []
            },
            {
                "id": "gpt-3.5-turbo",
                "object": "model",
                "owned_by": "openai",
                "permission": []
            }
        ]
    }

if __name__ == "__main__":
    print("Starting AIQToolkit Backend on http://localhost:8000")
    print("API docs available at http://localhost:8000/docs")
    uvicorn.run(app, host="0.0.0.0", port=8000)