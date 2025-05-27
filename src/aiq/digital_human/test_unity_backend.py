#!/usr/bin/env python3
"""Test Unity backend connection and demonstrate chat functionality."""

import asyncio
import websockets
import json
import sys
from datetime import datetime

class UnityBackendTester:
    def __init__(self, ws_url="ws://localhost:8080/ws", api_url="http://localhost:8000"):
        self.ws_url = ws_url
        self.api_url = api_url
        self.websocket = None
        
    async def connect(self):
        """Connect to the WebSocket server."""
        try:
            self.websocket = await websockets.connect(self.ws_url)
            print(f"✅ Connected to WebSocket at {self.ws_url}")
            return True
        except Exception as e:
            print(f"❌ Failed to connect: {e}")
            return False
    
    async def send_chat_message(self, message):
        """Send a chat message and receive response."""
        if not self.websocket:
            print("❌ Not connected to WebSocket")
            return None
            
        try:
            # Send message
            request = {
                "type": "chat",
                "data": {
                    "query": message,
                    "session_id": "unity-test-session",
                    "user_id": "unity-tester",
                    "timestamp": datetime.now().isoformat()
                }
            }
            
            await self.websocket.send(json.dumps(request))
            print(f"📤 Sent: {message}")
            
            # Receive response
            response = await self.websocket.recv()
            data = json.loads(response)
            print(f"📥 Received: {data}")
            
            return data
            
        except Exception as e:
            print(f"❌ Error during chat: {e}")
            return None
    
    async def test_mcp_tools(self):
        """Test MCP tool functionality."""
        print("\n🔧 Testing MCP Tools...")
        
        # Test web search
        web_search_msg = "Search for information about Unity game engine"
        response = await self.send_chat_message(web_search_msg)
        
        # Test file browser
        file_msg = "List files in the current directory"
        response = await self.send_chat_message(file_msg)
        
    async def interactive_chat(self):
        """Run interactive chat session."""
        print("\n💬 Starting interactive chat (type 'quit' to exit)")
        
        while True:
            try:
                message = input("\nYou: ")
                if message.lower() in ['quit', 'exit']:
                    break
                    
                response = await self.send_chat_message(message)
                
                if response and 'data' in response:
                    print(f"AI: {response['data'].get('response', 'No response')}")
                    
            except KeyboardInterrupt:
                break
            except Exception as e:
                print(f"Error: {e}")
    
    async def run_tests(self):
        """Run all tests."""
        print("🧪 Unity Backend Connection Test")
        print("================================")
        
        # Connect
        if not await self.connect():
            return
        
        # Test basic chat
        print("\n📝 Testing basic chat...")
        response = await self.send_chat_message("Hello, I'm testing the Unity backend connection")
        
        # Test MCP tools
        await self.test_mcp_tools()
        
        # Interactive mode
        await self.interactive_chat()
        
        # Cleanup
        if self.websocket:
            await self.websocket.close()
            print("\n👋 Connection closed")

async def main():
    """Main test function."""
    tester = UnityBackendTester()
    
    # Allow command line arguments for custom URLs
    if len(sys.argv) > 1:
        tester.ws_url = sys.argv[1]
    if len(sys.argv) > 2:
        tester.api_url = sys.argv[2]
    
    await tester.run_tests()

if __name__ == "__main__":
    print("🚀 Unity Backend Tester")
    print("Usage: python test_unity_backend.py [ws_url] [api_url]")
    print(f"Default: ws://localhost:8080/ws http://localhost:8000\n")
    
    asyncio.run(main())