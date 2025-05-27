#!/usr/bin/env python3
"""
AIQToolkit Standard API Server
Clean implementation without Digital Human components
"""

import uvicorn
from fastapi import FastAPI, WebSocket, WebSocketDisconnect
from fastapi.middleware.cors import CORSMiddleware
from typing import Dict, List, Optional
from pydantic import BaseModel
import asyncio
import json
import time

# Data models
class Message(BaseModel):
    role: str
    content: str

class ChatRequest(BaseModel):
    messages: List[Message]
    model: Optional[str] = "gpt-3.5-turbo"
    temperature: Optional[float] = 0.7
    stream: Optional[bool] = False

class WorkflowRequest(BaseModel):
    workflow_id: str
    inputs: Dict
    config: Optional[Dict] = {}

# Create FastAPI app
app = FastAPI(
    title="AIQToolkit Standard API",
    description="Standard AIQToolkit API without Digital Human components",
    version="1.0.0"
)

# Add CORS
app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)

# Available models
AVAILABLE_MODELS = [
    "gpt-3.5-turbo",
    "gpt-4",
    "claude-3-opus",
    "claude-3-sonnet",
    "llama-3.1-70b",
    "mixtral-8x7b"
]

# Available workflows
AVAILABLE_WORKFLOWS = [
    {
        "id": "react-agent",
        "name": "ReAct Agent",
        "description": "Reasoning and action agent"
    },
    {
        "id": "tool-calling",
        "name": "Tool Calling Agent",
        "description": "Agent with tool calling capabilities"
    },
    {
        "id": "rag-pipeline",
        "name": "RAG Pipeline",
        "description": "Retrieval augmented generation"
    }
]

# Health check
@app.get("/health")
async def health_check():
    return {
        "status": "healthy",
        "service": "aiqtoolkit-standard",
        "version": "1.0.0"
    }

# List models
@app.get("/v1/models")
async def list_models():
    return {
        "models": [
            {
                "id": model,
                "object": "model",
                "created": int(time.time()),
                "owned_by": "aiqtoolkit"
            }
            for model in AVAILABLE_MODELS
        ]
    }

# List workflows
@app.get("/v1/workflows")
async def list_workflows():
    return {"workflows": AVAILABLE_WORKFLOWS}

# Chat completion
@app.post("/v1/chat/completions")
async def chat_completion(request: ChatRequest):
    # Simple response generation
    user_message = request.messages[-1].content if request.messages else ""
    
    response = {
        "id": f"chat-{int(time.time())}",
        "object": "chat.completion",
        "created": int(time.time()),
        "model": request.model,
        "choices": [{
            "index": 0,
            "message": {
                "role": "assistant",
                "content": f"Response to: {user_message}"
            },
            "finish_reason": "stop"
        }],
        "usage": {
            "prompt_tokens": len(user_message.split()),
            "completion_tokens": 10,
            "total_tokens": len(user_message.split()) + 10
        }
    }
    
    return response

# Run workflow
@app.post("/v1/workflows/run")
async def run_workflow(request: WorkflowRequest):
    # Simulate workflow execution
    await asyncio.sleep(0.1)
    
    return {
        "workflow_id": request.workflow_id,
        "execution_id": f"exec-{int(time.time())}",
        "status": "completed",
        "outputs": {
            "result": f"Workflow {request.workflow_id} completed successfully"
        }
    }

# WebSocket for real-time communication
@app.websocket("/v1/ws")
async def websocket_endpoint(websocket: WebSocket):
    await websocket.accept()
    try:
        while True:
            data = await websocket.receive_json()
            
            # Handle different message types
            if data.get("type") == "chat":
                await websocket.send_json({
                    "type": "response",
                    "content": f"Echo: {data.get('content', '')}"
                })
            elif data.get("type") == "ping":
                await websocket.send_json({
                    "type": "pong",
                    "timestamp": int(time.time())
                })
                
    except WebSocketDisconnect:
        print("Client disconnected")

if __name__ == "__main__":
    print("Starting AIQToolkit Standard API Server")
    print("API Documentation: http://localhost:8000/docs")
    uvicorn.run(app, host="0.0.0.0", port=8000)