#!/bin/bash
# Setup Unity Project with Required Packages

echo "🎮 Unity MVP Project Setup"
echo "========================="

# Create project structure
PROJECT_DIR="DigitalHumanMVP"
echo "📁 Creating Unity project structure in: $PROJECT_DIR"

# Create directories
mkdir -p "$PROJECT_DIR/Assets/Scripts"
mkdir -p "$PROJECT_DIR/Assets/Materials"
mkdir -p "$PROJECT_DIR/Assets/Prefabs"
mkdir -p "$PROJECT_DIR/Assets/UI"
mkdir -p "$PROJECT_DIR/Packages"

# Copy our scripts to the project
echo "📋 Copying MVP scripts..."
cp MVPManager.cs "$PROJECT_DIR/Assets/Scripts/"
cp AvatarController.cs "$PROJECT_DIR/Assets/Scripts/"
cp LipSyncController.cs "$PROJECT_DIR/Assets/Scripts/"
cp ChatUIController.cs "$PROJECT_DIR/Assets/Scripts/"
cp BackendConnector.cs "$PROJECT_DIR/Assets/Scripts/"
cp QuickStart.cs "$PROJECT_DIR/Assets/Scripts/Editor/"

# Create Unity package manifest
cat > "$PROJECT_DIR/Packages/manifest.json" << 'EOF'
{
  "dependencies": {
    "com.unity.textmeshpro": "3.0.6",
    "com.unity.ui": "1.0.0",
    "com.unity.nuget.newtonsoft-json": "3.0.2",
    "com.readyplayerme.avatarloader": "5.0.0",
    "com.readyplayerme.gltfast": "5.0.0"
  }
}
EOF

# Create Project Settings file for WebSocket support
cat > "$PROJECT_DIR/ProjectSettings/Player.asset" << 'EOF'
# Unity Player Settings - Enable WebSocket Support
# This is a template - actual settings will be configured in Unity Editor
EOF

# Create basic scene setup script
cat > "$PROJECT_DIR/Assets/Scripts/SceneSetup.cs" << 'EOF'
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SceneSetup : MonoBehaviour
{
    void Start()
    {
        Debug.Log("Digital Human MVP Scene Setup");
        SetupCamera();
        SetupLighting();
        SetupUI();
    }

    void SetupCamera()
    {
        Camera mainCamera = Camera.main;
        if (mainCamera != null)
        {
            mainCamera.transform.position = new Vector3(0, 1.6f, -2f);
            mainCamera.transform.LookAt(new Vector3(0, 1.2f, 0));
            mainCamera.fieldOfView = 60f;
        }
    }

    void SetupLighting()
    {
        // Ambient lighting
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
        RenderSettings.ambientSkyColor = new Color(0.5f, 0.7f, 0.8f);
        RenderSettings.ambientEquatorColor = new Color(0.4f, 0.5f, 0.6f);
        RenderSettings.ambientGroundColor = new Color(0.2f, 0.2f, 0.2f);

        // Main directional light
        GameObject lightGO = new GameObject("Directional Light");
        Light light = lightGO.AddComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = 1f;
        light.transform.rotation = Quaternion.Euler(45f, -30f, 0);
    }

    void SetupUI()
    {
        // Create Canvas
        GameObject canvasGO = new GameObject("UI Canvas");
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasGO.AddComponent<CanvasScaler>();
        canvasGO.AddComponent<GraphicRaycaster>();

        // Add EventSystem
        GameObject eventSystem = new GameObject("EventSystem");
        eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
        eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
    }
}
EOF

# Create instructions for Unity setup
cat > "$PROJECT_DIR/UNITY_SETUP_GUIDE.md" << 'EOF'
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
EOF

# Create run script
cat > "$PROJECT_DIR/run_mvp.sh" << 'EOF'
#!/bin/bash
echo "🎮 Starting Digital Human MVP..."

# Check if Unity is in PATH
if ! command -v unity &> /dev/null; then
    echo "❌ Unity not found in PATH"
    echo "Please add Unity to your PATH or use Unity Hub"
    exit 1
fi

# Start backend services
echo "🚀 Starting backend services..."
cd /Users/apple/projects/AIQToolkit/src/aiq/digital_human
./start_unity_backend.sh &
BACKEND_PID=$!

# Wait for backend
sleep 5

# Open Unity project
echo "🎮 Opening Unity project..."
unity -projectPath .

# Cleanup on exit
trap "kill $BACKEND_PID" EXIT

echo "✅ MVP running. Press Ctrl+C to stop."
wait
EOF

chmod +x "$PROJECT_DIR/run_mvp.sh"

echo ""
echo "✅ Unity MVP Project Setup Complete!"
echo ""
echo "Project Location: $PROJECT_DIR"
echo ""
echo "Next Steps:"
echo "1. Open Unity Hub"
echo "2. Add existing project: $PROJECT_DIR"
echo "3. Open project in Unity 2022.3 LTS"
echo "4. Follow UNITY_SETUP_GUIDE.md for detailed setup"
echo ""
echo "To run the MVP:"
echo "cd $PROJECT_DIR && ./run_mvp.sh"
echo ""