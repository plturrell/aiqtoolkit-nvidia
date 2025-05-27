#!/usr/bin/env python3
"""
Test NVIDIA Avatar functionality
"""

import requests
import json
import time

def test_avatar_session():
    """Test creating an avatar session"""
    print("🔄 Creating avatar session...")
    response = requests.post("http://localhost:8082/avatar/session")
    data = response.json()
    print(f"✅ Session created: {data['session_id']}")
    return data['session_id']

def test_nim_chat(message):
    """Test NIM chat completion"""
    print(f"💬 Sending message to NIM: {message}")
    response = requests.post("http://localhost:8084/chat/completions", json={
        "model": "meta/llama3-70b-instruct",
        "messages": [
            {"role": "system", "content": "You are a helpful NVIDIA-powered AI assistant."},
            {"role": "user", "content": message}
        ],
        "temperature": 0.7,
        "max_tokens": 500
    })
    data = response.json()
    ai_response = data['choices'][0]['message']['content']
    print(f"🤖 AI Response: {ai_response}")
    return ai_response

def test_financial_analysis():
    """Test financial analysis endpoint"""
    print("📊 Testing financial analysis...")
    response = requests.get("http://localhost:8083/health")
    data = response.json()
    print(f"✅ Financial service status: {data['status']}")
    print(f"   Available algorithms: {', '.join(data['algorithms'])}")

def main():
    print("🚀 Testing NVIDIA Digital Human System")
    print("=====================================\n")
    
    # Test avatar session creation
    session_id = test_avatar_session()
    print()
    
    # Test NIM chat
    test_nim_chat("Tell me about NVIDIA's AI capabilities")
    print()
    
    # Test financial analysis
    test_financial_analysis()
    print()
    
    print("✅ All tests completed successfully!")
    print(f"\n🌐 Access the UI at: http://localhost:8080")

if __name__ == "__main__":
    main()