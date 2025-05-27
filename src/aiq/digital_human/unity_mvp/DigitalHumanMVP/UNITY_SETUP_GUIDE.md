# Unity Project Setup Guide

## Prerequisites
1. Unity Hub installed
2. Unity 2022.3 LTS or newer
3. Visual Studio or preferred code editor

## Steps to Set Up Project

### 1. Open Unity Hub
- Click "New Project"
- Select "3D Core" template  
- Name: "DigitalHumanMVP"
- Location: Choose this directory
- Click "Create Project"

### 2. Import Required Packages

#### Ready Player Me
1. Open Package Manager (Window > Package Manager)
2. Click "+" button > "Add package from git URL"
3. Enter: `https://github.com/readyplayerme/rpm-unity-sdk.git`

#### uLipSync
1. Download from Unity Asset Store (free)
2. Import into project

### 3. Configure Project Settings

1. **Player Settings** (Edit > Project Settings > Player)
   - Configuration > API Compatibility Level: .NET Standard 2.1
   - Configuration > Allow 'unsafe' Code: ✓

2. **Graphics Settings** (Edit > Project Settings > Graphics)
   - Add Universal Render Pipeline (if using URP)

### 4. Setup Initial Scene

1. Create new scene: "MainScene"
2. Add empty GameObject: "DigitalHumanMVP"
3. Attach scripts:
   - MVPManager
   - BackendConnector
   - Add child objects for UI

### 5. Configure Scripts

1. **MVPManager**:
   - Avatar URL: Your Ready Player Me avatar URL
   - Backend URL: ws://localhost:8080/ws

2. **BackendConnector**:
   - WebSocket URL: ws://localhost:8080/ws
   - REST API URL: http://localhost:8000
   - Enable Auto Reconnect: ✓

### 6. Build UI

Use the QuickStart window:
- AIQToolkit > Digital Human MVP > Quick Setup
- Click "Create UI" button
- Adjust layout as needed

### 7. Test Connection

1. Start backend services:
   ```bash
   cd /Users/apple/projects/AIQToolkit/src/aiq/digital_human
   ./start_unity_backend.sh
   ```

2. Enter Play mode in Unity
3. Type message in chat
4. Verify avatar responds

## Troubleshooting

### WebSocket Connection Failed
- Check backend is running
- Verify firewall settings
- Try REST API fallback

### Avatar Not Loading  
- Check Ready Player Me URL
- Verify internet connection
- Check console for GLB errors

### No Lip Sync
- Ensure uLipSync is imported
- Check audio source component
- Verify TTS audio is playing

## Next Steps

1. Customize avatar animations
2. Add more interactive features  
3. Implement gesture system
4. Deploy to target platform
