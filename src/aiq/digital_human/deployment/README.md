# Digital Human Production Deployment

## Quick Start

To deploy the digital human system:

```bash
cd /Users/apple/projects/AIQToolkit/src/aiq/digital_human/deployment
./deploy_with_venv.sh
```

## Services

The system runs the following services:

1. **Main UI** (Port 8080)
   - Web interface with NVIDIA branding
   - Chat interface
   - Avatar display
   - Financial analysis controls

2. **Orchestrator** (Port 8081)
   - Manages service coordination
   - Health monitoring

3. **Avatar Renderer** (Port 8082)  
   - NVIDIA ACE integration
   - Avatar streaming

4. **Financial Analyzer** (Port 8083)
   - Monte Carlo Tree Search (MCTS)
   - Portfolio optimization

## Configuration

NVIDIA API Key: `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`

## Access Points

- Main UI: http://localhost:8080
- Health Check: http://localhost:8080/health
- Service Status: Check the service status in the UI footer

## Management

### Stop Services
```bash
./stop_all_venv.sh
```

### Check Logs
```bash
cd /Users/apple/projects/AIQToolkit/src/aiq/digital_human/logs
tail -f *.log
```

### Monitor Services
```bash
ps aux | grep -E "(ui_server|orchestrator|avatar|financial)" | grep -v grep
```

## Features

- ✅ Photorealistic digital human with NVIDIA ACE
- ✅ Real-time chat interface
- ✅ Financial analysis with MCTS
- ✅ Market insights
- ✅ Portfolio optimization
- ✅ Service health monitoring
- ✅ CORS-enabled APIs

## Troubleshooting

If services fail to start:

1. Check if ports are available:
   ```bash
   lsof -i :8080-8083
   ```

2. Clean logs and restart:
   ```bash
   ./clean_logs.sh
   ./deploy_with_venv.sh
   ```

3. Check virtual environment:
   ```bash
   which python3
   pip list
   ```

## Blueprint Integration

The system is configured to integrate with the NVIDIA Blueprint at `/projects/digital-human` when available.