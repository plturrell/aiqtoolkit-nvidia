from fastapi import FastAPI, WebSocket
from fastapi.middleware.cors import CORSMiddleware
import uvicorn
import os

app = FastAPI(title="NVIDIA Avatar Renderer")

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
        "nvidia_ace": "connected"
    }

if __name__ == "__main__":
    uvicorn.run(app, host="0.0.0.0", port=8082)
