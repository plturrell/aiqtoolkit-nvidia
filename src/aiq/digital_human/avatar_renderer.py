"""
NVIDIA Avatar Renderer Service
"""
from fastapi import FastAPI, WebSocket
from fastapi.middleware.cors import CORSMiddleware
import uvicorn
import json
import os

app = FastAPI(title="NVIDIA Avatar Renderer")

# Enable CORS
app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)

@app.get("/health")
def health():
    return {
        "status": "healthy",
        "service": "NVIDIA Avatar Renderer",
        "nvidia_api_key": os.getenv("NVIDIA_API_KEY", "").startswith("nvapi-")
    }

@app.websocket("/avatar/stream")
async def avatar_stream(websocket: WebSocket):
    await websocket.accept()
    await websocket.send_json({
        "type": "avatar_ready",
        "message": "NVIDIA ACE avatar initialized"
    })
    
    while True:
        try:
            data = await websocket.receive_text()
            # Echo back with avatar response
            await websocket.send_json({
                "type": "avatar_response",
                "data": f"Avatar processed: {data}"
            })
        except Exception:
            break

if __name__ == "__main__":
    uvicorn.run(app, host="0.0.0.0", port=8082)
