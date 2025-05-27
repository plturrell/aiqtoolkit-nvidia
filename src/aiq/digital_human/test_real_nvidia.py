#!/usr/bin/env python3
"""
Test Real NVIDIA APIs
"""

import requests
import json

def test_nvidia_health():
    """Test NVIDIA API connectivity"""
    print("🔍 Testing NVIDIA API Health...")
    response = requests.get("http://localhost:8087/health")
    data = response.json()
    print(f"Status: {data['status']}")
    print(f"NVIDIA API: {data['nvidia_api_status']}")
    print(f"API Key: {data['api_key']}")
    return data['nvidia_api_status'] == 'connected'

def test_nvidia_chat():
    """Test real NVIDIA NIM chat"""
    print("\n💬 Testing NVIDIA NIM Chat...")
    response = requests.post("http://localhost:8087/chat", json={
        "messages": [
            {"role": "system", "content": "You are a helpful NVIDIA AI assistant."},
            {"role": "user", "content": "What is NVIDIA ACE and how does it work?"}
        ]
    })
    
    data = response.json()
    if "choices" in data:
        print("Response:", data["choices"][0]["message"]["content"][:200] + "...")
    else:
        print("Response:", json.dumps(data, indent=2))
    
    return "choices" in data

def test_avatar_creation():
    """Test avatar creation"""
    print("\n🤖 Testing Avatar Creation...")
    response = requests.post("http://localhost:8087/avatar/create")
    data = response.json()
    print("Avatar response:", json.dumps(data, indent=2))
    return "error" not in data

def main():
    print("🚀 Testing Real NVIDIA Digital Human APIs")
    print("=" * 40)
    
    # Test health
    health_ok = test_nvidia_health()
    
    # Test chat if health is OK
    if health_ok:
        chat_ok = test_nvidia_chat()
        
        # Test avatar
        avatar_ok = test_avatar_creation()
        
        print("\n📊 Test Results:")
        print(f"✅ Health Check: {'PASSED' if health_ok else 'FAILED'}")
        print(f"✅ Chat API: {'PASSED' if chat_ok else 'FAILED'}")
        print(f"✅ Avatar API: {'PASSED' if avatar_ok else 'FAILED'}")
    else:
        print("\n❌ API connection failed. Check your NVIDIA API key.")
    
    print(f"\n🌐 UI available at: http://localhost:8088")
    print("Try the following:")
    print("1. Click 'Initialize Real Avatar' to create an NVIDIA avatar")
    print("2. Type a message to chat with the real NVIDIA LLM")
    print("3. Click '🎤 Voice Input' for speech recognition")

if __name__ == "__main__":
    main()