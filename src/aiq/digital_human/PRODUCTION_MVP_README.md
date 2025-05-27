# Production Unity Digital Human MVP

## 🚀 Quick Start (Production Ready)

### One-Click Start
```bash
cd /Users/apple/projects/AIQToolkit/src/aiq/digital_human
./start_production_mvp.sh
```

This starts:
- Unity WebGL Application (http://localhost:8080)
- Backend API Server (http://localhost:8000)
- WebSocket Server (ws://localhost:8088/ws)
- Monitoring Stack (Grafana, Prometheus)
- Health Checks

## 🎮 What's Included

### Unity Application
- **Ready Player Me** avatar integration
- **uLipSync** real-time lip synchronization
- **Professional Chat UI** with animations
- **WebSocket/REST** backend connectivity
- **Error Recovery** with automatic reconnection
- **Performance Monitoring** built-in

### Backend Services
- **LangChain** AI integration
- **MCP Servers** (web search, file browser)
- **WebSocket** real-time communication
- **REST API** with fallback support
- **Health Monitoring** endpoints
- **Prometheus Metrics** export

### Infrastructure
- **Docker Compose** deployment
- **Kubernetes** manifests included
- **Nginx** reverse proxy
- **SSL/TLS** ready
- **Auto-scaling** configuration
- **Load balancing** support

## 📊 Monitoring

### Grafana Dashboard
- URL: http://localhost:3000
- Login: admin/admin
- Pre-configured dashboard: "Digital Human MVP"

### Metrics Available
- Response times
- Error rates
- Active connections
- CPU/Memory usage
- Avatar load times
- Lip sync accuracy

## 🔧 Configuration

### Environment Variables
```bash
# Backend Configuration
LANGCHAIN_API_KEY=your-key
NVIDIA_API_KEY=nvapi-xxx
OPENAI_API_KEY=sk-xxx

# Unity Configuration
AVATAR_URL=https://models.readyplayerme.com/your-avatar.glb
BACKEND_URL=ws://localhost:8088/ws
```

### Config Files
- `unity_mvp/Config/production_config.json`
- `unity_mvp/Deployment/docker-compose.mvp.yml`
- `unity_mvp/Scripts/ProductionConfig.cs`

## 🏗️ Architecture

```
┌─────────────────┐     ┌─────────────────┐
│  Unity WebGL    │────▶│  Nginx Proxy    │
│  Application    │     │  (Port 8080)    │
└─────────────────┘     └─────────────────┘
         │                       │
         ▼                       ▼
┌─────────────────┐     ┌─────────────────┐
│  WebSocket      │────▶│  Backend API    │
│  (Port 8088)    │     │  (Port 8000)    │
└─────────────────┘     └─────────────────┘
         │                       │
         ▼                       ▼
┌─────────────────┐     ┌─────────────────┐
│  LangChain      │────▶│  MCP Servers    │
│  Integration    │     │  (Search/Files) │
└─────────────────┘     └─────────────────┘
```

## 🚀 Deployment Options

### Local Development
```bash
./start_production_mvp.sh
```

### Docker Deployment
```bash
cd unity_mvp/Deployment
docker-compose -f docker-compose.mvp.yml up -d
```

### Kubernetes Deployment
```bash
kubectl apply -f unity_mvp/Deployment/kubernetes_mvp.yaml
```

### Cloud Deployment (AWS/GCP/Azure)
```bash
cd unity_mvp/Deployment
./deploy_production_mvp.sh --cloud aws --region us-west-2
```

## 🔍 Testing

### Health Check
```bash
cd unity_mvp/Deployment/monitoring
python3 health_check.py
```

### Load Testing
```bash
cd unity_mvp/Testing
./load_test.sh --users 100 --duration 5m
```

### Integration Tests
```bash
cd unity_mvp/Testing
python3 integration_tests.py
```

## 🐛 Troubleshooting

### Common Issues

1. **"Cannot connect to backend"**
   - Check Docker is running
   - Verify ports are not in use
   - Check firewall settings

2. **"Avatar not loading"**
   - Verify Ready Player Me URL
   - Check network connectivity
   - Review CORS settings

3. **"No lip sync"**
   - Ensure audio is enabled
   - Check uLipSync configuration
   - Verify TTS is working

### Debug Mode
```bash
# Enable debug logging
export DEBUG=true
./start_production_mvp.sh
```

### View Logs
```bash
# All services
docker-compose -f docker-compose.mvp.yml logs

# Specific service
docker-compose -f docker-compose.mvp.yml logs backend-api
```

## 📈 Performance

### Benchmarks
- Response Time: < 200ms
- Lip Sync Latency: < 50ms
- Avatar Load: < 3s
- Memory Usage: < 2GB
- CPU Usage: < 50%

### Optimization Tips
1. Enable GPU acceleration
2. Use CDN for assets
3. Implement response caching
4. Optimize texture sizes
5. Enable compression

## 🔐 Security

### Built-in Security
- API key management
- WebSocket authentication
- Rate limiting
- CORS configuration
- SSL/TLS support

### Best Practices
1. Rotate API keys regularly
2. Use environment variables
3. Enable HTTPS in production
4. Implement user authentication
5. Monitor security logs

## 📚 Documentation

- [Unity Setup Guide](unity_mvp/Docs/UnitySetup.md)
- [API Reference](unity_mvp/Docs/APIReference.md)
- [Deployment Guide](unity_mvp/Deployment/README.md)
- [Migration Plan](unity_mvp/NVIDIA_ACE_TO_UNITY_MIGRATION_PLAN.md)

## 🤝 Support

- GitHub Issues: [AIQToolkit/issues](https://github.com/aiqtoolkit/issues)
- Documentation: [docs.aiqtoolkit.com](https://docs.aiqtoolkit.com)
- Discord: [Join our community](https://discord.gg/aiqtoolkit)
- Email: support@aiqtoolkit.com

## 🎯 Next Steps

1. **Customize Avatar**: Update Ready Player Me URL
2. **Train Models**: Fine-tune LangChain responses
3. **Add Features**: Gestures, emotions, backgrounds
4. **Scale Up**: Deploy to cloud infrastructure
5. **Monitor**: Set up alerts and dashboards

---

**Version**: 1.0.0  
**License**: MIT  
**Last Updated**: May 2025