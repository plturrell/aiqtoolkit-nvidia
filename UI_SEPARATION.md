# AIQToolkit UI Separation

## Directory Structure

```
AIQToolkit/
├── standard-ui/          # Clean standard UI without digital human
│   ├── backend/         # FastAPI server
│   ├── frontend/        # Next.js React app
│   └── start.sh         # Startup script
│
├── web-ui/              # NVIDIA integration UI
│   ├── index.html       # Simple HTML interface
│   ├── api/             # Vercel serverless functions
│   └── vercel.json      # Vercel configuration
│
├── src/aiq/digital_human/  # Digital Human components
│   ├── ui/              # Digital Human specific UI
│   ├── deployment/      # Deployment scripts
│   └── config/          # Configuration files
│
└── external/aiqtoolkit-opensource-ui/  # External UI (needs cleanup)
```

## Standard UI Features

The `standard-ui/` directory contains:

1. **Backend API** (`backend/server.py`):
   - Chat completions endpoint
   - Model management
   - Workflow execution
   - WebSocket support
   - No digital human dependencies

2. **Frontend** (`frontend/`):
   - Clean React/Next.js app
   - Model selection
   - Chat interface
   - Workflow management
   - No digital human components

3. **Startup**:
   ```bash
   cd standard-ui
   ./start.sh
   ```

## NVIDIA Integration UI

The `web-ui/` directory contains:
- HTML interface for report generation
- Integration with NVIDIA Brev instance
- Deployable to Vercel

## Digital Human Components

Kept separate in `src/aiq/digital_human/`:
- Specialized UI for digital avatars
- NVIDIA Audio2Face integration
- Consensus system
- Production deployment scripts

## Benefits of Separation

1. **Modularity**: Each UI serves a specific purpose
2. **Clean Dependencies**: No unwanted cross-dependencies
3. **Easier Deployment**: Deploy only what you need
4. **Better Maintenance**: Focused codebases
5. **Flexibility**: Mix and match components as needed

## Usage

### Standard UI Only
```bash
cd standard-ui
./start.sh
# Access at http://localhost:3000
```

### NVIDIA Report Generator
```bash
cd web-ui
python -m http.server 3000
# Access at http://localhost:3000
```

### Digital Human
```bash
cd src/aiq/digital_human/ui
./scripts/start_elite.sh
# Access at configured port
```