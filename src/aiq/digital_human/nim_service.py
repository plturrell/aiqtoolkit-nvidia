from fastapi import FastAPI
from fastapi.middleware.cors import CORSMiddleware
import uvicorn
import asyncio
import random

app = FastAPI(title="NVIDIA NIM Service")

app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)

@app.get("/health")
async def health():
    return {
        "status": "healthy",
        "service": "NVIDIA NIM",
        "models": ["llama3-70b", "mixtral-8x7b", "codellama-70b"],
        "endpoints": ["chat", "completion", "embedding"]
    }

@app.post("/chat/completions")
async def chat_completions(request: dict):
    # Simulate NVIDIA NIM response
    await asyncio.sleep(0.5)  # Simulate API latency
    
    messages = request.get("messages", [])
    user_message = messages[-1]["content"] if messages else ""
    
    responses = [
        "As an NVIDIA-powered AI, I can help you with advanced financial analysis using GPU-accelerated computing.",
        "NVIDIA's technology enables me to process complex market data and provide real-time insights.",
        "Using NVIDIA NIM, I can analyze your portfolio and suggest optimizations based on current market conditions.",
        "The power of NVIDIA GPUs allows me to run sophisticated Monte Carlo simulations for risk assessment."
    ]
    
    return {
        "id": f"nim-{random.randint(1000, 9999)}",
        "model": "meta/llama3-70b-instruct",
        "choices": [{
            "index": 0,
            "message": {
                "role": "assistant",
                "content": random.choice(responses) + f" Regarding '{user_message}', I'm processing that with NVIDIA's advanced AI models."
            },
            "finish_reason": "stop"
        }],
        "usage": {
            "prompt_tokens": 50,
            "completion_tokens": 100,
            "total_tokens": 150
        }
    }

if __name__ == "__main__":
    uvicorn.run(app, host="0.0.0.0", port=8084)
