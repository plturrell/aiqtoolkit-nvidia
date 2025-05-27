#!/usr/bin/env python3
"""
Test script for NVIDIA Blueprint integration
"""

import os
import sys
import json
import requests
from pathlib import Path

# Set up environment
os.environ["NVIDIA_API_KEY"] = "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"
os.environ["NVIDIA_BLUEPRINT_PATH"] = "/projects/digital-human"

# Import the integration
from blueprint_integration import NVIDIABlueprintIntegration


def test_blueprint_loading():
    """Test if the blueprint loads correctly"""
    print("Testing NVIDIA Blueprint loading...")
    
    integration = NVIDIABlueprintIntegration()
    
    print(f"API Key configured: {bool(integration.api_key)}")
    print(f"Blueprint path: {integration.blueprint_path}")
    print(f"Blueprint exists: {integration.blueprint_path.exists()}")
    
    if integration.blueprint_config:
        print("Blueprint configuration loaded:")
        print(json.dumps(integration.blueprint_config, indent=2))
    
    return integration


def test_services():
    """Test NVIDIA service connections"""
    print("\nTesting NVIDIA services...")
    
    integration = NVIDIABlueprintIntegration()
    
    # Test ACE
    if hasattr(integration, 'ace_client'):
        print("✓ ACE client initialized")
    else:
        print("✗ ACE client not available")
    
    # Test NIM
    if hasattr(integration, 'nim_client'):
        print("✓ NIM client initialized")
    else:
        print("✗ NIM client not available")
    
    # Test Riva
    if hasattr(integration, 'riva_client'):
        print("✓ Riva client initialized")
    else:
        print("✗ Riva client not available")


def test_digital_human_creation():
    """Test creating a digital human instance"""
    print("\nTesting digital human creation...")
    
    integration = NVIDIABlueprintIntegration()
    
    try:
        digital_human = integration.create_digital_human()
        print("✓ Digital human created successfully")
        return digital_human
    except Exception as e:
        print(f"✗ Failed to create digital human: {e}")
        return None


def test_api_endpoints():
    """Test API endpoints if running"""
    print("\nTesting API endpoints...")
    
    base_url = "http://localhost:8000"
    
    # Test health
    try:
        response = requests.get(f"{base_url}/health")
        print(f"Health check: {response.json()}")
    except:
        print("API not running. Start with: python nvidia_blueprint/api.py")
        return
    
    # Test NVIDIA status
    try:
        response = requests.get(f"{base_url}/nvidia/status")
        print(f"NVIDIA status: {response.json()}")
    except Exception as e:
        print(f"Error checking NVIDIA status: {e}")


def main():
    """Run all tests"""
    print("NVIDIA Blueprint Integration Test")
    print("=================================")
    
    # Test 1: Blueprint loading
    test_blueprint_loading()
    
    # Test 2: Service connections
    test_services()
    
    # Test 3: Digital human creation
    test_digital_human_creation()
    
    # Test 4: API endpoints
    test_api_endpoints()
    
    print("\nTest complete!")


if __name__ == "__main__":
    main()