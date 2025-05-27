# Unity Scene Setup Guide

This guide provides step-by-step instructions for setting up the Unity scene for the Digital Human MVP.

## Scene Hierarchy Structure

```
Digital Human MVP Scene
├── Managers
│   ├── MVP Manager
│   ├── Audio Manager
│   └── Event System
├── Avatar
│   ├── Avatar Container
│   └── Lighting
│       ├── Directional Light
│       └── Reflection Probe
├── Cameras
│   └── Main Camera
└── UI
    └── Canvas
        ├── Chat Panel
        │   ├── Header
        │   ├── Scroll View
        │   │   └── Content
        │   ├── Input Panel
        │   │   ├── Input Field
        │   │   └── Send Button
        │   └── Typing Indicator
        ├── Status Panel
        │   └── Status Text
        └── Loading Panel
            └── Loading Text
```

## Step-by-Step Setup

### 1. Create Base GameObjects

1. **Create Manager Objects**:
   ```
   GameObject > Create Empty > "Managers"
   GameObject > Create Empty > "MVP Manager" (child of Managers)
   GameObject > Create Empty > "Audio Manager" (child of Managers)
   ```

2. **Create Avatar Container**:
   ```
   GameObject > Create Empty > "Avatar"
   GameObject > Create Empty > "Avatar Container" (child of Avatar)
   Position: (0, 0, 0)
   Rotation: (0, 0, 0)
   Scale: (1, 1, 1)
   ```

3. **Setup Lighting**:
   ```
   GameObject > Light > Directional Light
   Position: (0, 3, 0)
   Rotation: (45, -30, 0)
   Intensity: 1.2
   Color: Warm white (#FFF4E5)
   ```

4. **Configure Camera**:
   ```
   Main Camera
   Position: (0, 1.6, 2.5)
   Rotation: (0, 0, 0)
   Field of View: 40
   Background: Solid Color (#2C3E50)
   ```

### 2. Create UI Structure

1. **Create Canvas**:
   ```
   GameObject > UI > Canvas
   Canvas Scaler:
   - UI Scale Mode: Scale With Screen Size
   - Reference Resolution: 1920x1080
   - Screen Match Mode: 0.5
   ```

2. **Create Chat Panel**:
   ```
   Right-click Canvas > UI > Panel > "Chat Panel"
   Rect Transform:
   - Anchor: Bottom Right
   - Position: (-200, 200)
   - Size: (400, 600)
   - Anchor Preset: Bottom Right
   ```

3. **Create Scroll View**:
   ```
   Right-click Chat Panel > UI > Scroll View
   Rect Transform:
   - Anchor: Stretch
   - Left: 10, Right: 10
   - Top: 50, Bottom: 100
   ```

4. **Create Input Panel**:
   ```
   Right-click Chat Panel > UI > Panel > "Input Panel"
   Rect Transform:
   - Anchor: Bottom Stretch
   - Height: 80
   - Bottom: 10
   ```

5. **Create Status Panel**:
   ```
   Right-click Canvas > UI > Panel > "Status Panel"
   Rect Transform:
   - Anchor: Top Center
   - Position: (0, -50)
   - Size: (400, 60)
   ```

### 3. Add Components

1. **MVP Manager**:
   ```csharp
   Add Component > MVPManager
   
   Configure in Inspector:
   - Avatar Controller: Avatar Container
   - Lip Sync Controller: Audio Manager
   - Chat UI Controller: Chat Panel
   - Backend Connector: MVP Manager (self)
   - Start Chat Button: (assign button)
   - Loading Panel: (assign panel)
   - Status Text: (assign text)
   ```

2. **Avatar Container**:
   ```csharp
   Add Component > AvatarController
   
   Configure in Inspector:
   - Default Avatar URL: (Ready Player Me URL)
   - Avatar Container: (self reference)
   - Avatar Scale: 1
   - Animator Controller: (create and assign)
   ```

3. **Audio Manager**:
   ```csharp
   Add Component > AudioSource
   Add Component > LipSyncController
   
   Configure in Inspector:
   - Audio Source: (self AudioSource)
   - Sensitivity: 1
   - Smooth Time: 0.1
   ```

4. **Chat Panel**:
   ```csharp
   Add Component > ChatUIController
   
   Configure in Inspector:
   - Chat Scroll View: (assign)
   - Message Container: Content
   - Input Field: (assign)
   - Send Button: (assign)
   - Message Prefabs: (create and assign)
   ```

5. **Backend Connector** (on MVP Manager):
   ```csharp
   Add Component > BackendConnector
   
   Configure in Inspector:
   - Websocket URL: ws://localhost:8080/ws
   - Reconnect Delay: 5
   - Max Reconnect Attempts: 3
   - Heartbeat Interval: 30
   ```

### 4. Create Prefabs

1. **User Message Prefab**:
   ```
   GameObject
   ├── Background (Image)
   │   Color: Blue (#3498db)
   │   Border Radius: 10px
   └── Message Text (TextMeshPro)
       Font Size: 14
       Color: White
       Alignment: Right
   ```

2. **AI Message Prefab**:
   ```
   GameObject
   ├── Background (Image)
   │   Color: Light Gray (#ecf0f1)
   │   Border Radius: 10px
   └── Message Text (TextMeshPro)
       Font Size: 14
       Color: Dark Gray (#2c3e50)
       Alignment: Left
   ```

3. **Typing Indicator**:
   ```
   GameObject
   └── Dots Container (HorizontalLayoutGroup)
       ├── Dot 1 (Text) "•"
       ├── Dot 2 (Text) "•"
       └── Dot 3 (Text) "•"
   ```

### 5. Animation Setup

1. **Create Animator Controller**:
   ```
   Assets > Create > Animator Controller > "AvatarAnimator"
   ```

2. **Add Animation States**:
   - Idle
   - Speaking
   - Thinking
   - Listening
   - Blinking (as sub-state)

3. **Create Transitions**:
   ```
   Idle → Speaking (Trigger: "Speaking")
   Speaking → Idle (Exit Time: 1.0)
   Idle → Thinking (Trigger: "Thinking")
   Thinking → Idle (Trigger: "Idle")
   ```

### 6. Material and Shader Setup

1. **Create Avatar Material**:
   ```
   Assets > Create > Material > "AvatarMaterial"
   Shader: Universal Render Pipeline/Lit
   Base Map: (Avatar texture)
   Metallic: 0
   Smoothness: 0.5
   ```

2. **Create UI Materials**:
   ```
   Assets > Create > Material > "UIBlur"
   Shader: UI/Default
   ```

### 7. Project Settings

1. **Player Settings**:
   ```
   Edit > Project Settings > Player
   - Company Name: AIQToolkit
   - Product Name: Digital Human MVP
   - Default Icon: (assign icon)
   ```

2. **Quality Settings**:
   ```
   Edit > Project Settings > Quality
   - Anti Aliasing: 4x Multi Sampling
   - Shadows: Soft Shadows
   - Texture Quality: Full Res
   ```

3. **Audio Settings**:
   ```
   Edit > Project Settings > Audio
   - Default Speaker Mode: Stereo
   - Sample Rate: 44100 Hz
   ```

### 8. Testing Setup

1. **Test Avatar Loading**:
   - Enter Play Mode
   - Click "Start Chat" button
   - Verify avatar loads

2. **Test Backend Connection**:
   - Ensure backend is running
   - Check console for connection logs
   - Send test message

3. **Test Lip Sync**:
   - Play audio clip
   - Verify mouth movements
   - Adjust sensitivity if needed

### 9. Optimization

1. **Performance Settings**:
   ```
   - Target Frame Rate: 60
   - V-Sync: Every V Blank
   - Rendering Path: Forward
   ```

2. **LOD Settings**:
   ```
   - Avatar LOD: 3 levels
   - UI Canvas: Screen Space - Overlay
   - Culling Mask: Optimize layers
   ```

### 10. Build Configuration

1. **Standalone Build**:
   ```
   File > Build Settings
   - Platform: PC, Mac & Linux Standalone
   - Target Platform: Windows/Mac/Linux
   - Architecture: x86_64
   ```

2. **WebGL Build** (optional):
   ```
   File > Build Settings
   - Platform: WebGL
   - Compression Format: Gzip
   - WebAssembly Streaming: Enabled
   ```

## Troubleshooting

### Common Issues

1. **Avatar Not Loading**:
   - Check internet connection
   - Verify Ready Player Me URL
   - Check CORS settings

2. **No Audio**:
   - Verify AudioSource component
   - Check audio mixer settings
   - Test with local audio file

3. **UI Not Responding**:
   - Check EventSystem exists
   - Verify Canvas sorting order
   - Test input module settings

4. **Performance Issues**:
   - Reduce avatar polycount
   - Optimize UI draw calls
   - Use object pooling for messages

## Next Steps

1. Add custom animations
2. Implement gesture system
3. Add voice input support
4. Create settings panel
5. Add multiplayer support

## Resources

- [Unity Documentation](https://docs.unity3d.com/)
- [Ready Player Me Unity SDK](https://docs.readyplayer.me/ready-player-me/integration-guides/unity)
- [uLipSync Documentation](https://github.com/hecomi/uLipSync/wiki)
- [TextMeshPro Documentation](https://docs.unity3d.com/Packages/com.unity.textmeshpro@3.0/manual/index.html)