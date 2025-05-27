#!/usr/bin/env python3
"""
Start AIQToolkit with console frontend
"""

import os
import sys
from pathlib import Path

# Add the source directory to the Python path
project_root = Path(__file__).parent.parent
sys.path.insert(0, str(project_root / "src"))

# Set up basic configuration
os.environ["AIQ_CONFIG_FILE"] = str(project_root / "config" / "examples" / "simple_fastapi_config.yml")

# Import and run AIQToolkit
try:
    from aiq.cli.main import run_cli
    
    # Run with console frontend
    sys.argv = ["aiq", "start", "console", "--config_file", os.environ["AIQ_CONFIG_FILE"]]
    run_cli()
except ImportError as e:
    print(f"Error importing AIQToolkit: {e}")
    print("Make sure AIQToolkit is properly installed")
except Exception as e:
    print(f"Error starting AIQToolkit: {e}")