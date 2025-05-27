"""
Full Digital Human Integration
Connects NVIDIA APIs with MCP servers and neural systems
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

# Import AIQToolkit components
import sys
sys.path.append('/Users/apple/projects/AIQToolkit/src')
from aiq.neural.distributed_neural_computer import DistributedNeuralComputer
from aiq.neural.nash_ethereum_consensus import NashEthereumConsensus
from aiq.neural.knowledge_integration import KnowledgeIntegrationSystem
from aiq.tool.mcp_client import MCPClient
from aiq.tool.server_tools import WebSearchTool, FileSystemTool

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

class DigitalHumanOrchestrator:
    def __init__(self):
        self.nvidia_api_key = NVIDIA_API_KEY
        self.neural_computer = DistributedNeuralComputer()
        self.consensus_system = NashEthereumConsensus()
        self.knowledge_system = KnowledgeIntegrationSystem()
        self.mcp_client = MCPClient()
        self.web_search = WebSearchTool()
        self.file_system = FileSystemTool()
        
        # Initialize MCP servers
        self.mcp_servers = {
            "web_search": "http://localhost:8091",
            "file_browser": "http://localhost:8092",
            "data_analysis": "http://localhost:8093"
        }
        
    async def process_query(self, query: str, use_neural: bool = True, use_mcp: bool = True):
        """Process user query through full pipeline"""
        
        context = {
            "query": query,
            "timestamp": datetime.now().isoformat(),
            "sources": []
        }
        
        # Step 1: Use MCP for web search and context gathering
        if use_mcp:
            try:
                # Search web for relevant information
                web_results = await self.search_web(query)
                context["web_results"] = web_results
                context["sources"].append("web_search")
                
                # Search local files
                file_results = await self.search_files(query)
                context["file_results"] = file_results
                context["sources"].append("file_system")
                
            except Exception as e:
                print(f"MCP error: {e}")
        
        # Step 2: Use neural systems for reasoning
        if use_neural:
            try:
                # Process through neural computer
                neural_response = await self.neural_computer.process({
                    "input": query,
                    "context": context
                })
                context["neural_analysis"] = neural_response
                context["sources"].append("neural_computer")
                
                # Get consensus from multiple neural nodes
                consensus = await self.consensus_system.get_consensus({
                    "query": query,
                    "neural_response": neural_response
                })
                context["consensus"] = consensus
                context["sources"].append("consensus_system")
                
            except Exception as e:
                print(f"Neural system error: {e}")
        
        # Step 3: Integrate knowledge
        knowledge = await self.knowledge_system.integrate(context)
        context["integrated_knowledge"] = knowledge
        
        # Step 4: Generate response using NVIDIA NIM
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
                    json={"query": query}
                )
                return response.json().get("results", [])
        except:
            # Fallback to direct search
            return self.web_search.search(query)
    
    async def search_files(self, query: str) -> List[Dict]:
        """Search local files using MCP server"""
        try:
            async with httpx.AsyncClient() as client:
                response = await client.post(
                    f"{self.mcp_servers['file_browser']}/search",
                    json={"query": query}
                )
                return response.json().get("files", [])
        except:
            # Fallback to direct file search
            return self.file_system.search(query)
    
    async def generate_response(self, query: str, context: Dict) -> str:
        """Generate response using NVIDIA NIM with context"""
        
        # Build prompt with context
        prompt = f"""
        User Query: {query}
        
        Context:
        - Web Results: {json.dumps(context.get('web_results', []), indent=2)}
        - File Results: {json.dumps(context.get('file_results', []), indent=2)}
        - Neural Analysis: {json.dumps(context.get('neural_analysis', {}), indent=2)}
        - Consensus: {json.dumps(context.get('consensus', {}), indent=2)}
        
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
                            {"role": "system", "content": "You are an AI assistant with access to web search, file system, and neural processing capabilities."},
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
                    return f"Error: {data}"
                    
        except Exception as e:
            return f"Error generating response: {str(e)}"

orchestrator = DigitalHumanOrchestrator()

@app.get("/")
async def root():
    return {
        "service": "Full Digital Human Integration",
        "status": "active",
        "components": {
            "nvidia_nim": "connected",
            "mcp_servers": orchestrator.mcp_servers,
            "neural_systems": ["distributed_neural_computer", "consensus", "knowledge_integration"]
        }
    }

@app.post("/process")
async def process_query(data: dict):
    """Process query through full pipeline"""
    query = data.get("query", "")
    use_neural = data.get("use_neural", True)
    use_mcp = data.get("use_mcp", True)
    
    result = await orchestrator.process_query(query, use_neural, use_mcp)
    return result

@app.websocket("/ws/chat")
async def chat_websocket(websocket: WebSocket):
    """WebSocket for real-time chat with full integration"""
    await websocket.accept()
    
    try:
        while True:
            data = await websocket.receive_json()
            query = data.get("query", "")
            
            # Send status updates
            await websocket.send_json({
                "type": "status",
                "message": "Searching web..."
            })
            
            # Process through full pipeline
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
