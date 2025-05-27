# Unity Digital Human MVP Deployment Guide

## Overview

This guide walks you through deploying the Unity Digital Human MVP that uses:
- Ready Player Me for avatar generation
- uLipSync for lip synchronization  
- LangChain backend for AI responses
- MCP servers for extended functionality

## Prerequisites

1. **Development Environment**
   - Unity Hub installed
   - Unity 2022.3 LTS or newer
   - Visual Studio or VS Code
   - Git

2. **Backend Requirements**
   - Python 3.9+
   - Node.js 16+
   - Docker (optional)

3. **API Keys**
   - Ready Player Me account
   - Google Cloud (for Speech APIs) - optional
   - LangChain API keys

## Step 1: Backend Setup

### Start Backend Services

```bash
cd /Users/apple/projects/AIQToolkit/src/aiq/digital_human
./start_unity_backend.sh
```

This starts:
- WebSocket server (ws://localhost:8080/ws)
- REST API server (http://localhost:8000)
- MCP servers (web search, file browser)
- LangChain integration

### Verify Backend

```bash
python test_unity_backend.py
```

Expected output:
```
✅ Connected to WebSocket at ws://localhost:8080/ws
📤 Sent: Hello, I'm testing the Unity backend connection
📥 Received: {"type": "chat", "data": {"response": "Hello! Connection successful."}}
```

## Step 2: Unity Project Setup

### Create New Project

1. Open Unity Hub
2. Click "New Project"
3. Select "3D Core" template
4. Name: "DigitalHumanMVP"
5. Location: Choose the created directory

### Import Required Packages

1. **Ready Player Me SDK**
   ```
   Window > Package Manager > + > Add package from git URL
   https://github.com/readyplayerme/rpm-unity-sdk.git
   ```

2. **uLipSync**
   - Download from Unity Asset Store (free)
   - Import all assets

3. **TextMeshPro**
   - Window > TextMeshPro > Import TMP Essential Resources

### Configure Project Settings

1. **Player Settings** (Edit > Project Settings > Player)
   - API Compatibility Level: .NET Standard 2.1
   - Allow 'unsafe' Code: ✓

2. **Build Settings** (File > Build Settings)
   - Platform: PC, Mac & Linux Standalone
   - Architecture: x86_64

## Step 3: Scene Setup

### Use Quick Setup Tool

1. Open Quick Setup window:
   ```
   AIQToolkit > Digital Human MVP > Quick Setup
   ```

2. Click buttons in order:
   - Create Basic Scene
   - Setup Camera
   - Create UI
   - Add Avatar Manager
   - Configure Backend

### Manual Setup (Alternative)

1. Create empty GameObject: "DigitalHumanMVP"
2. Add components:
   - MVPManager
   - BackendConnector

3. Configure MVPManager:
   - Avatar URL: `https://models.readyplayerme.com/[your-avatar-id].glb`
   - Backend URL: `ws://localhost:8080/ws`

4. Create UI Canvas with chat interface

## Step 4: Avatar Configuration

### Ready Player Me Setup

1. Create avatar at https://readyplayerme.com
2. Copy GLB URL
3. Paste into AvatarController component

### Lip Sync Configuration

1. Add uLipSync component to avatar
2. Configure phoneme mappings
3. Set audio source for TTS playback

## Step 5: Testing

### Local Testing

1. Start backend services
2. Enter Play mode in Unity
3. Type message in chat
4. Verify:
   - Avatar loads correctly
   - Lip sync works with audio
   - Chat responses appear

### Performance Testing

Monitor:
- Frame rate (target: 60 FPS)
- Memory usage (< 2GB)
- Network latency (< 200ms)

## Step 6: Building for Deployment

### Windows Build

```
File > Build Settings
- Platform: Windows
- Architecture: x86_64
- Build
```

### macOS Build

```
File > Build Settings
- Platform: Mac
- Architecture: Intel 64-bit + Apple silicon
- Build
```

### WebGL Build

```
File > Build Settings
- Platform: WebGL
- Compression: Gzip
- Build
```

## Step 7: Production Deployment

### Backend Deployment

1. **Docker Deployment**
   ```bash
   docker build -t digital-human-backend .
   docker run -p 8080:8080 -p 8000:8000 digital-human-backend
   ```

2. **Cloud Deployment** (AWS/GCP/Azure)
   - Use provided Kubernetes manifests
   - Configure load balancers
   - Setup SSL certificates

### Unity Application Deployment

1. **Desktop Applications**
   - Package with installer (InnoSetup/DMG)
   - Sign executables
   - Distribute via website/store

2. **WebGL Deployment**
   - Upload to web server
   - Configure CORS headers
   - Use CDN for assets

## Troubleshooting

### Common Issues

1. **Avatar Not Loading**
   - Check Ready Player Me URL
   - Verify internet connection
   - Check CORS settings

2. **No Lip Sync**
   - Ensure uLipSync is properly configured
   - Check audio source is playing
   - Verify phoneme mappings

3. **WebSocket Connection Failed**
   - Check backend is running
   - Verify firewall settings
   - Try REST API fallback

### Debug Mode

Enable debug logging:
```csharp
BackendConnector.EnableDebugLogging = true;
MVPManager.EnableVerboseLogging = true;
```

## Configuration Files

### Backend Configuration

```json
{
  "websocket": {
    "url": "ws://localhost:8080/ws",
    "reconnect": true,
    "reconnectInterval": 5000
  },
  "api": {
    "url": "http://localhost:8000",
    "timeout": 30000
  },
  "mcp": {
    "webSearch": true,
    "fileBrowser": true
  }
}
```

### Unity Configuration

```json
{
  "avatar": {
    "url": "https://models.readyplayerme.com/your-avatar.glb",
    "animations": {
      "idle": "Idle",
      "talking": "Talking",
      "thinking": "Thinking"
    }
  },
  "ui": {
    "chatHistory": 50,
    "fontSize": 14,
    "theme": "dark"
  }
}
```

## Performance Optimization

### Unity Optimizations

1. **LOD (Level of Detail)**
   - Setup avatar LODs
   - Reduce polygon count at distance

2. **Texture Optimization**
   - Use texture atlases
   - Compress textures appropriately

3. **Animation Optimization**
   - Use animation compression
   - Optimize bone count

### Backend Optimizations

1. **Caching**
   - Cache LLM responses
   - Store frequently used data

2. **Load Balancing**
   - Distribute WebSocket connections
   - Use multiple backend instances

## Monitoring

### Unity Metrics

- FPS counter
- Memory profiler
- Network statistics

### Backend Metrics

- WebSocket connections
- API response times
- Error rates

## Security Considerations

1. **API Keys**
   - Store securely
   - Use environment variables
   - Rotate regularly

2. **WebSocket Security**
   - Implement authentication
   - Use WSS in production
   - Rate limiting

3. **Data Privacy**
   - Encrypt sensitive data
   - Comply with GDPR/privacy laws
   - Clear session data

## Support Resources

- Unity Forums: https://forum.unity.com
- Ready Player Me Docs: https://docs.readyplayerme.com
- uLipSync Wiki: https://github.com/uezo/uLipSync/wiki
- AIQToolkit Support: support@aiqtoolkit.com

## Next Steps

1. Customize avatar appearance
2. Add gesture system
3. Implement emotion detection
4. Create multiplayer support
5. Deploy to mobile platforms

---

*Version: 1.0*  
*Last Updated: {{ current_date }}*