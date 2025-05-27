# Digital Human Unity MVP - Production Ready

A production-grade Unity implementation of the AIQToolkit Digital Human system with full WebSocket/REST integration, Ready Player Me avatars, and real-time lip sync.

## 🚀 Features

- **Production-Grade Architecture**
  - Robust error handling and recovery
  - Comprehensive logging and monitoring
  - Automated deployment scripts
  - Health check monitoring
  - Performance metrics collection

- **Core Functionality**
  - Real-time WebSocket communication
  - Ready Player Me avatar integration with caching
  - uLipSync for realistic mouth movements
  - Professional chat UI with animations
  - Offline mode support
  - Multi-environment configuration

- **DevOps Ready**
  - Docker containerization
  - Kubernetes deployment manifests
  - CI/CD pipeline support
  - Prometheus metrics integration
  - Grafana dashboards
  - ELK stack for logging

## 📋 Prerequisites

- Unity 2022.3 LTS or newer
- Docker and Docker Compose
- Node.js 16+ (for backend)
- Python 3.8+ (for monitoring)
- Ready Player Me account (for avatar customization)

## 🛠️ Quick Start

1. **Clone the repository**
   ```bash
   cd /Users/apple/projects/AIQToolkit/src/aiq/digital_human/unity_mvp
   ```

2. **Run one-click deployment**
   ```bash
   ./Deployment/deploy_production_mvp.sh
   ```

3. **Access the application**
   - Web Interface: http://localhost:8080
   - Metrics Dashboard: http://localhost:3000
   - Health Check: http://localhost:8080/health

## 📁 Project Structure

```
unity_mvp/
├── Scripts/                    # Production Unity C# scripts
│   ├── ProductionMVP.cs       # Main controller
│   ├── ProductionBackendConnector.cs
│   ├── ProductionAvatarManager.cs
│   ├── ProductionLipSync.cs
│   ├── ProductionChatUI.cs
│   ├── ProductionConfig.cs
│   ├── ProductionMetrics.cs
│   └── ProductionLogger.cs
├── Deployment/                # Deployment configuration
│   ├── deploy_production_mvp.sh
│   ├── docker-compose.mvp.yml
│   ├── kubernetes_mvp.yaml
│   ├── Dockerfile
│   └── nginx/
├── Monitoring/               # Monitoring scripts
│   ├── health_check.py
│   └── prometheus.yml
├── Prefabs/                 # Unity prefabs
│   └── MVPPrefab.prefab
├── Config/                  # Configuration files
│   ├── scene_setup.json
│   ├── build_settings.json
│   └── production_config.json
└── README.md
```

## 🎮 Unity Setup

1. **Import the package**
   - Open Unity Hub
   - Create new project with Universal Render Pipeline
   - Import the unity_mvp folder

2. **Configure the scene**
   - Open scene_setup.json for reference
   - Drag MVPPrefab into the scene
   - Configure backend URLs in inspector

3. **Build for WebGL**
   ```bash
   Unity -batchmode -quit -projectPath . -buildTarget WebGL -executeMethod BuildScript.BuildProduction
   ```

## 🔧 Configuration

### Environment Variables
```bash
# Backend connection
BACKEND_URL=ws://localhost:8081/ws
API_URL=http://localhost:8081/api

# Avatar settings
AVATAR_URL=https://api.readyplayer.me/v1/avatars/default.glb

# Monitoring
METRICS_ENDPOINT=http://localhost:9090/metrics
LOG_LEVEL=info

# Authentication (optional)
AUTH_TOKEN=your-auth-token
API_KEY=your-api-key
```

### Configuration Files

1. **production_config.json**
   ```json
   {
     "backend": {
       "webSocketUrl": "ws://localhost:8080/ws",
       "restApiUrl": "http://localhost:8080/api"
     },
     "avatar": {
       "quality": "High",
       "enableCaching": true
     },
     "metrics": {
       "enabled": true,
       "reportingInterval": 60
     }
   }
   ```

## 🚢 Deployment

### Docker Deployment
```bash
# Build and run with Docker Compose
docker-compose -f Deployment/docker-compose.mvp.yml up -d

# Check logs
docker-compose -f Deployment/docker-compose.mvp.yml logs -f

# Scale services
docker-compose -f Deployment/docker-compose.mvp.yml up -d --scale digital-human-frontend=3
```

### Kubernetes Deployment
```bash
# Deploy to Kubernetes
kubectl apply -f Deployment/kubernetes_mvp.yaml

# Check status
kubectl get pods -n digital-human-mvp

# View logs
kubectl logs -n digital-human-mvp -l app=digital-human-frontend
```

### Production Deployment
```bash
# Deploy to production
./Deployment/deploy_production_mvp.sh --env production

# Deploy to staging
./Deployment/deploy_production_mvp.sh --env staging
```

## 📊 Monitoring

### Health Checks
```bash
# Run health check
python3 Monitoring/health_check.py --backend-url http://localhost:8081

# Continuous monitoring
python3 Monitoring/health_check.py --interval 30
```

### Metrics
- Prometheus metrics: http://localhost:9090
- Grafana dashboards: http://localhost:3000
- Custom metrics endpoint: http://localhost:8000/metrics

### Available Metrics
- Frame rate and performance
- WebSocket connection status
- Message throughput
- Avatar loading times
- Lip sync accuracy
- Error rates
- User interaction metrics

## 🧪 Testing

```bash
# Run unit tests
Unity -batchmode -quit -runTests -testPlatform EditMode

# Run integration tests
cd Tests
python3 run_integration_tests.py

# Run performance tests
python3 run_performance_tests.py
```

## 🔍 Troubleshooting

### Common Issues

1. **WebSocket Connection Failed**
   ```bash
   # Check backend status
   curl http://localhost:8081/health
   
   # Check WebSocket endpoint
   wscat -c ws://localhost:8081/ws
   ```

2. **Avatar Loading Issues**
   - Check Ready Player Me API status
   - Verify avatar URL is accessible
   - Clear avatar cache in settings

3. **Performance Issues**
   - Check frame rate in metrics
   - Adjust quality settings
   - Enable dynamic quality mode

### Debug Mode
```csharp
// Enable debug logging
ProductionLogger.Instance.minLogLevel = LogLevel.Debug;

// Enable performance profiler
ProductionMetrics.Instance.enableProfiling = true;
```

## 📚 Architecture

### Component Hierarchy
```
ProductionMVP (Main Controller)
├── ProductionBackendConnector (WebSocket/REST)
├── ProductionAvatarManager (Ready Player Me)
├── ProductionLipSync (uLipSync)
├── ProductionChatUI (UI Management)
├── ProductionConfig (Settings)
├── ProductionMetrics (Performance)
└── ProductionLogger (Logging)
```

### Data Flow
1. User sends message via UI
2. BackendConnector sends to server
3. Server processes and returns response
4. Avatar displays emotion/animation
5. LipSync processes audio
6. Metrics track performance

## 🔒 Security

- HTTPS/WSS support in production
- Authentication token management
- CORS configuration
- Rate limiting
- Input sanitization
- Secure WebSocket implementation

## 🤝 Contributing

1. Fork the repository
2. Create feature branch
3. Make changes with tests
4. Submit pull request

## 📄 License

Copyright (c) 2024 AIQToolkit. All rights reserved.

## 🆘 Support

- Documentation: [docs.aiqtoolkit.com](https://docs.aiqtoolkit.com)
- Issues: [GitHub Issues](https://github.com/aiqtoolkit/issues)
- Discord: [Join our community](https://discord.gg/aiqtoolkit)

## 🚀 Roadmap

- [ ] VR/AR support
- [ ] Multi-language support
- [ ] Advanced emotion detection
- [ ] Voice cloning integration
- [ ] Gesture recognition
- [ ] Real-time translation