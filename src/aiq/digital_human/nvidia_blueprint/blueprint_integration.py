"""
NVIDIA Blueprint Integration for Digital Human
Connects the downloaded NVIDIA Blueprint with AIQToolkit
"""

import os
import sys
import json
import logging
from typing import Dict, Any, Optional
from pathlib import Path

# Add the NVIDIA Blueprint path to Python path
BLUEPRINT_PATH = "/projects/digital-human"
if os.path.exists(BLUEPRINT_PATH):
    sys.path.insert(0, BLUEPRINT_PATH)

# NVIDIA API Key from environment or parameter
NVIDIA_API_KEY = "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"

logger = logging.getLogger(__name__)


class NVIDIABlueprintIntegration:
    """
    Integrates the downloaded NVIDIA Blueprint with AIQToolkit's digital human
    """
    
    def __init__(self, api_key: Optional[str] = None):
        self.api_key = api_key or NVIDIA_API_KEY or os.getenv("NVIDIA_API_KEY")
        self.blueprint_path = Path(BLUEPRINT_PATH)
        
        # Initialize NVIDIA services
        self._init_nvidia_services()
        
        # Load blueprint configuration
        self._load_blueprint_config()
        
    def _init_nvidia_services(self):
        """Initialize NVIDIA services with API key"""
        os.environ["NVIDIA_API_KEY"] = self.api_key
        
        # Import NVIDIA modules from blueprint
        try:
            from nvidia_ace import ACEClient
            from nvidia_nim import NIMClient
            from nvidia_riva import RivaClient
            
            self.ace_client = ACEClient(api_key=self.api_key)
            self.nim_client = NIMClient(api_key=self.api_key)
            self.riva_client = RivaClient(api_key=self.api_key)
            
            logger.info("NVIDIA services initialized successfully")
            
        except ImportError as e:
            logger.warning(f"Could not import NVIDIA modules: {e}")
            # Fallback to AIQToolkit implementations
            from aiq.digital_human.nvidia_integration.ace_platform import NVIDIAACEPlatform
            self.ace_client = NVIDIAACEPlatform(api_key=self.api_key)
            
    def _load_blueprint_config(self):
        """Load configuration from NVIDIA Blueprint"""
        config_path = self.blueprint_path / "config" / "blueprint_config.json"
        
        if config_path.exists():
            with open(config_path, 'r') as f:
                self.blueprint_config = json.load(f)
                logger.info("Blueprint configuration loaded")
        else:
            # Default configuration
            self.blueprint_config = {
                "avatar": {
                    "model": "digital-human-2d",
                    "fps": 60,
                    "resolution": [1920, 1080]
                },
                "speech": {
                    "asr_model": "conformer-ctc",
                    "tts_model": "fastpitch-hifigan"
                },
                "language": {
                    "model": "nemotron-4-340b-instruct",
                    "temperature": 0.7
                },
                "rag": {
                    "embedding_model": "nv-embed-v1",
                    "retriever": "nemo-retriever"
                }
            }
            
    def create_digital_human(self, config: Optional[Dict] = None) -> 'DigitalHuman':
        """
        Create a digital human instance using the blueprint
        """
        from digital_human import DigitalHuman  # From blueprint
        
        # Merge configurations
        final_config = {**self.blueprint_config, **(config or {})}
        
        # Initialize digital human
        digital_human = DigitalHuman(
            api_key=self.api_key,
            config=final_config
        )
        
        # Connect AIQToolkit components
        self._connect_aiqtoolkit_components(digital_human)
        
        return digital_human
        
    def _connect_aiqtoolkit_components(self, digital_human):
        """Connect AIQToolkit's neural supercomputer and financial engine"""
        # Import AIQToolkit components
        from aiq.digital_human.orchestrator.digital_human_orchestrator import DigitalHumanOrchestrator
        from aiq.digital_human.financial_engine.mcts_analyzer import FinancialMCTSAnalyzer
        from aiq.digital_human.conversation.sglang_engine import SgLangConversationEngine
        
        # Connect neural reasoning
        orchestrator = DigitalHumanOrchestrator()
        digital_human.set_reasoning_engine(orchestrator.neural_computer)
        
        # Connect financial analysis
        financial_analyzer = FinancialMCTSAnalyzer()
        digital_human.set_financial_engine(financial_analyzer)
        
        # Connect conversation engine
        conversation_engine = SgLangConversationEngine()
        digital_human.set_conversation_engine(conversation_engine)
        
    def run_demo(self):
        """Run a demo of the integrated digital human"""
        logger.info("Starting NVIDIA Blueprint Digital Human Demo")
        
        # Create digital human
        digital_human = self.create_digital_human()
        
        # Run demo interaction
        response = digital_human.interact(
            text="Hello, I need help with my investment portfolio",
            emotion="curious"
        )
        
        return response


# Utility function to setup the integration
def setup_nvidia_blueprint():
    """Setup NVIDIA Blueprint integration with AIQToolkit"""
    integration = NVIDIABlueprintIntegration()
    return integration


if __name__ == "__main__":
    # Test the integration
    integration = setup_nvidia_blueprint()
    response = integration.run_demo()
    print(f"Demo response: {response}")