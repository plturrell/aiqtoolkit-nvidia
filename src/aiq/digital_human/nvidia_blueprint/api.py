"""
FastAPI server for NVIDIA Blueprint Digital Human
"""

from fastapi import FastAPI, WebSocket, HTTPException
from fastapi.middleware.cors import CORSMiddleware
from typing import Dict, Any, Optional
import asyncio
import os

from .blueprint_integration import NVIDIABlueprintIntegration
from ..orchestrator.digital_human_orchestrator import DigitalHumanOrchestrator

app = FastAPI(
    title="Digital Human with NVIDIA Blueprint",
    description="AIQToolkit Digital Human integrated with NVIDIA Blueprint",
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

# Initialize integration
blueprint_integration = NVIDIABlueprintIntegration()
orchestrator = DigitalHumanOrchestrator()

# Global digital human instance
digital_human = None


@app.on_event("startup")
async def startup_event():
    """Initialize digital human on startup"""
    global digital_human
    digital_human = blueprint_integration.create_digital_human()
    await orchestrator.initialize()


@app.get("/health")
async def health_check():
    """Health check endpoint"""
    return {
        "status": "healthy",
        "service": "digital-human-blueprint",
        "nvidia_api_configured": bool(os.getenv("NVIDIA_API_KEY")),
        "blueprint_loaded": blueprint_integration.blueprint_config is not None
    }


@app.get("/nvidia/status")
async def nvidia_status():
    """Check NVIDIA services status"""
    status = {
        "api_key_configured": bool(blueprint_integration.api_key),
        "blueprint_path": str(blueprint_integration.blueprint_path),
        "blueprint_exists": blueprint_integration.blueprint_path.exists(),
        "services": {}
    }
    
    # Check individual services
    if hasattr(blueprint_integration, 'ace_client'):
        status["services"]["ace"] = "connected"
    if hasattr(blueprint_integration, 'nim_client'):
        status["services"]["nim"] = "connected"
    if hasattr(blueprint_integration, 'riva_client'):
        status["services"]["riva"] = "connected"
        
    return status


@app.post("/chat")
async def chat(request: Dict[str, Any]):
    """Chat with the digital human"""
    if not digital_human:
        raise HTTPException(status_code=503, detail="Digital human not initialized")
        
    text = request.get("text", "")
    emotion = request.get("emotion", "neutral")
    
    try:
        response = await digital_human.interact(
            text=text,
            emotion=emotion,
            stream=request.get("stream", False)
        )
        
        return {
            "response": response.text,
            "emotion": response.emotion,
            "avatar_data": response.avatar_data,
            "audio_url": response.audio_url
        }
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))


@app.websocket("/ws")
async def websocket_endpoint(websocket: WebSocket):
    """WebSocket for real-time interaction"""
    await websocket.accept()
    
    try:
        while True:
            data = await websocket.receive_json()
            
            # Process the message
            response = await digital_human.interact(
                text=data.get("text", ""),
                emotion=data.get("emotion", "neutral"),
                stream=True
            )
            
            # Stream response back
            async for chunk in response:
                await websocket.send_json({
                    "type": "response_chunk",
                    "text": chunk.text,
                    "emotion": chunk.emotion,
                    "avatar_frame": chunk.avatar_frame,
                    "audio_chunk": chunk.audio_chunk
                })
                
    except Exception as e:
        await websocket.send_json({
            "type": "error",
            "message": str(e)
        })
        await websocket.close()


@app.post("/configure")
async def configure(config: Dict[str, Any]):
    """Update configuration"""
    global digital_human
    
    # Update configuration
    blueprint_integration.blueprint_config.update(config)
    
    # Recreate digital human with new config
    digital_human = blueprint_integration.create_digital_human(config)
    
    return {"status": "configured", "config": blueprint_integration.blueprint_config}


@app.get("/models")
async def list_models():
    """List available models"""
    return {
        "avatar_models": ["digital-human-2d", "aria", "james"],
        "language_models": ["nemotron-4-340b-instruct", "llama-3.1-70b"],
        "voice_models": ["fastpitch", "tacotron2"],
        "emotion_models": ["emotion-2d", "facial-emotion-recognition"]
    }


if __name__ == "__main__":
    import uvicorn
    uvicorn.run(app, host="0.0.0.0", port=8000)