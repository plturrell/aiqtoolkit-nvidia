"""
Integration module for NVIDIA LangChain Report Generator
Connects AIQToolkit to the NVIDIA Brev environment
"""

import requests
import json
from typing import Dict, Any, Optional
from aiq.builder.function_base import AIQFunctionBase

class NVIDIAReportGenerator(AIQFunctionBase):
    """
    Connects to NVIDIA LangChain report generator running on Brev
    """
    
    def __init__(self, endpoint: str, description: str = "NVIDIA Report Generator"):
        self.endpoint = endpoint
        self.description = description
        self.api_key = None  # Add if needed
        
    async def run(self, prompt: str, **kwargs) -> Dict[str, Any]:
        """
        Generate a structured report using NVIDIA LangChain
        """
        try:
            headers = {
                "Content-Type": "application/json",
            }
            
            # Add auth if needed
            if self.api_key:
                headers["Authorization"] = f"Bearer {self.api_key}"
            
            payload = {
                "prompt": prompt,
                "options": kwargs
            }
            
            # Call the NVIDIA endpoint
            response = requests.post(
                f"{self.endpoint}/generate",
                headers=headers,
                json=payload
            )
            
            if response.status_code == 200:
                return response.json()
            else:
                return {
                    "error": f"API Error: {response.status_code}",
                    "message": response.text
                }
                
        except Exception as e:
            return {
                "error": str(e),
                "message": "Failed to connect to NVIDIA report generator"
            }
    
    def get_description(self) -> str:
        return self.description
        
    def get_schema(self) -> Dict[str, Any]:
        return {
            "type": "function",
            "function": {
                "name": "nvidia_report_generator",
                "description": self.description,
                "parameters": {
                    "type": "object",
                    "properties": {
                        "prompt": {
                            "type": "string",
                            "description": "The prompt to generate a report from"
                        },
                        "format": {
                            "type": "string",
                            "enum": ["markdown", "pdf", "html"],
                            "description": "Output format for the report"
                        },
                        "include_sources": {
                            "type": "boolean",
                            "description": "Include source citations in the report"
                        }
                    },
                    "required": ["prompt"]
                }
            }
        }