"""
Simplified Full Digital Human Integration
Works without complex neural systems
"""

import os
import asyncio
import json
from typing import Dict, Any, List
from fastapi import FastAPI, HTTPException, WebSocket
from fastapi.middleware.cors import CORSMiddleware
import uvicorn
import httpx
from datetime import datetime

app = FastAPI(title="Full Digital Human Integration")

app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)

# Configuration
NVIDIA_API_KEY = os.getenv("NVIDIA_API_KEY", "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")
NVIDIA_NIM_ENDPOINT = "https://integrate.api.nvidia.com/v1"

class SimpleOrchestrator:
    def __init__(self):
        self.nvidia_api_key = NVIDIA_API_KEY
        self.mcp_servers = {
            "web_search": "http://localhost:8091",
            "file_browser": "http://localhost:8092",
        }
        
    async def process_query(self, query: str, use_mcp: bool = True):
        """Process user query through simplified pipeline"""
        
        context = {
            "query": query,
            "timestamp": datetime.now().isoformat(),
            "sources": []
        }
        
        # Step 1: Use MCP for context gathering
        if use_mcp:
            try:
                # Search web
                web_results = await self.search_web(query)
                context["web_results"] = web_results
                context["sources"].append("web_search")
                
                # Search files
                file_results = await self.search_files(query)
                context["file_results"] = file_results
                context["sources"].append("file_system")
                
            except Exception as e:
                print(f"MCP error: {e}")
        
        # Step 2: Generate response using NVIDIA NIM
        response = await self.generate_response(query, context)
        
        return {
            "query": query,
            "response": response,
            "context": context,
            "sources_used": context["sources"]
        }
    
    async def search_web(self, query: str) -> List[Dict]:
        """Search web using MCP server"""
        try:
            async with httpx.AsyncClient() as client:
                response = await client.post(
                    f"{self.mcp_servers['web_search']}/search",
                    json={"query": query},
                    timeout=5
                )
                return response.json().get("results", [])
        except Exception as e:
            return [{"error": str(e)}]
    
    async def search_files(self, query: str) -> List[Dict]:
        """Search local files using MCP server"""
        try:
            async with httpx.AsyncClient() as client:
                response = await client.post(
                    f"{self.mcp_servers['file_browser']}/search",
                    json={"query": query},
                    timeout=5
                )
                return response.json().get("files", [])
        except Exception as e:
            return [{"error": str(e)}]
    
    async def generate_response(self, query: str, context: Dict) -> str:
        """Generate response using NVIDIA NIM with context"""
        
        # Build prompt with context
        prompt = f"""
User Query: {query}

Context from searches:
- Web Results: {json.dumps(context.get('web_results', [])[:3], indent=2)}
- File Results: {json.dumps(context.get('file_results', [])[:3], indent=2)}

Based on the above context, provide a comprehensive response.
"""
        
        headers = {
            "Authorization": f"Bearer {self.nvidia_api_key}",
            "Content-Type": "application/json"
        }
        
        try:
            async with httpx.AsyncClient() as client:
                response = await client.post(
                    f"{NVIDIA_NIM_ENDPOINT}/chat/completions",
                    headers=headers,
                    json={
                        "model": "meta/llama3-70b-instruct",
                        "messages": [
                            {"role": "system", "content": "You are an AI assistant with access to web search and file system."},
                            {"role": "user", "content": prompt}
                        ],
                        "temperature": 0.7,
                        "max_tokens": 1000
                    },
                    timeout=30
                )
                
                data = response.json()
                if "choices" in data:
                    return data["choices"][0]["message"]["content"]
                else:
                    return f"API Response: {data}"
                    
        except Exception as e:
            return f"Error generating response: {str(e)}"

orchestrator = SimpleOrchestrator()

@app.get("/")
async def root():
    return {
        "service": "Full Digital Human Integration (Simplified)",
        "status": "active",
        "components": {
            "nvidia_nim": "connected",
            "mcp_servers": orchestrator.mcp_servers
        }
    }

@app.post("/process")
async def process_query(data: dict):
    """Process query through pipeline"""
    query = data.get("query", "")
    use_mcp = data.get("use_mcp", True)
    
    result = await orchestrator.process_query(query, use_mcp)
    return result

@app.websocket("/ws/chat")
async def chat_websocket(websocket: WebSocket):
    """WebSocket for real-time chat"""
    await websocket.accept()
    
    try:
        while True:
            data = await websocket.receive_json()
            query = data.get("query", "")
            
            # Send status
            await websocket.send_json({
                "type": "status",
                "message": "Processing query..."
            })
            
            # Process
            result = await orchestrator.process_query(query)
            
            await websocket.send_json({
                "type": "response",
                "data": result
            })
            
    except Exception as e:
        await websocket.send_json({
            "type": "error",
            "message": str(e)
        })
        await websocket.close()

if __name__ == "__main__":
    uvicorn.run(app, host="0.0.0.0", port=8090)
