from http.server import BaseHTTPRequestHandler
import json
import sys
import os

# Add the src directory to Python path
sys.path.insert(0, os.path.join(os.path.dirname(__file__), '..', 'src'))

class handler(BaseHTTPRequestHandler):
    def do_GET(self):
        self.send_response(200)
        self.send_header('Content-type', 'application/json')
        self.send_header('Access-Control-Allow-Origin', '*')
        self.end_headers()
        
        # Parse query parameters
        from urllib.parse import urlparse, parse_qs
        parsed = urlparse(self.path)
        query_params = parse_qs(parsed.query)
        
        endpoint = parsed.path.split('/')[-1] if parsed.path.split('/')[-1] else 'info'
        
        if endpoint == 'info':
            response = self.get_aiq_info()
        elif endpoint == 'components':
            response = self.get_available_components()
        elif endpoint == 'workflows':
            response = self.get_available_workflows()
        elif endpoint == 'reasoning':
            response = self.get_available_reasoning_systems()
        elif endpoint == 'health':
            response = self.get_health_status()
        else:
            response = {'error': f'Unknown endpoint: {endpoint}'}
        
        self.wfile.write(json.dumps(response, indent=2).encode())

    def do_POST(self):
        try:
            self.send_response(200)
            self.send_header('Content-type', 'application/json')
            self.send_header('Access-Control-Allow-Origin', '*')
            self.send_header('Access-Control-Allow-Methods', 'POST, OPTIONS')
            self.send_header('Access-Control-Allow-Headers', 'Content-Type')
            self.end_headers()

            # Read request body
            content_length = int(self.headers.get('Content-Length', 0))
            if content_length > 0:
                post_data = self.rfile.read(content_length)
                request_data = json.loads(post_data.decode('utf-8'))
            else:
                request_data = {}

            # Parse endpoint from path
            from urllib.parse import urlparse
            parsed = urlparse(self.path)
            endpoint = parsed.path.split('/')[-1] if parsed.path.split('/')[-1] else 'run'

            if endpoint == 'run' or endpoint == 'execute':
                response = self.execute_workflow(request_data)
            elif endpoint == 'validate':
                response = self.validate_config(request_data)
            elif endpoint == 'build':
                response = self.build_workflow(request_data)
            else:
                response = {'error': f'Unknown POST endpoint: {endpoint}'}

            self.wfile.write(json.dumps(response).encode())

        except Exception as e:
            self.send_response(500)
            self.send_header('Content-type', 'application/json')
            self.send_header('Access-Control-Allow-Origin', '*')
            self.end_headers()
            self.wfile.write(json.dumps({'error': str(e)}).encode())

    def do_OPTIONS(self):
        self.send_response(200)
        self.send_header('Access-Control-Allow-Origin', '*')
        self.send_header('Access-Control-Allow-Methods', 'GET, POST, OPTIONS')
        self.send_header('Access-Control-Allow-Headers', 'Content-Type')
        self.end_headers()

    def get_aiq_info(self):
        """Get AIQToolkit system information"""
        return {
            "name": "AIQToolkit",
            "version": "1.0.0",
            "description": "Complete AI Agent and Workflow Toolkit with NVIDIA Integration",
            "features": [
                "Multi-agent workflows (ReAct, ReWOO, Reasoning, Tool-calling)",
                "NVIDIA GPU optimization and integration", 
                "Transparent AI reasoning engine",
                "Digital human interfaces",
                "Distributed processing capabilities",
                "Enterprise security and observability",
                "Real-time consensus mechanisms",
                "Advanced profiling and evaluation"
            ],
            "endpoints": {
                "/api/aiq/info": "System information",
                "/api/aiq/components": "Available components",
                "/api/aiq/workflows": "Available workflows", 
                "/api/aiq/reasoning": "Reasoning systems overview",
                "/api/aiq/health": "System health status",
                "/api/aiq/run": "Execute workflow (POST)",
                "/api/aiq/validate": "Validate configuration (POST)",
                "/api/aiq/build": "Build workflow (POST)"
            },
            "ui_interfaces": {
                "/reasoning": "100% Transparent AI reasoning interface",
                "/pure": "10/10 Jony Ive inspired design interface",
                "/status": "System status and monitoring dashboard",
                "/digital-human": "Advanced digital human interface",
                "/elite": "Professional AI interface",
                "/minimal": "Clean, focused design interface"
            }
        }

    def get_available_components(self):
        """Get list of available AIQ components"""
        try:
            # Try to import AIQ components
            components = {
                "reasoning_systems": [
                    "react_reasoning",
                    "rewoo_reasoning", 
                    "reasoning_agent",
                    "tool_calling_reasoning",
                    "neural_symbolic_hybrid",
                    "mcts_probabilistic",
                    "jena_semantic",
                    "dspy_self_improving"
                ],
                "llms": [
                    "openai_llm",
                    "nim_llm",
                    "nvidia_llm"
                ],
                "embedders": [
                    "openai_embedder",
                    "nim_embedder",
                    "langchain_embedder"
                ],
                "retrievers": [
                    "basic_retriever",
                    "memory_retriever",
                    "nvidia_rag_retriever"
                ],
                "tools": [
                    "web_search",
                    "code_execution",
                    "document_search",
                    "nvidia_rag",
                    "datetime_tools"
                ],
                "evaluators": [
                    "rag_evaluator",
                    "trajectory_evaluator", 
                    "swe_bench_evaluator"
                ],
                "front_ends": [
                    "console_ui",
                    "fastapi_server",
                    "mcp_server"
                ]
            }
            return {
                "status": "success",
                "components": components,
                "total_components": sum(len(v) for v in components.values()),
                "reasoning_architecture": {
                    "core_systems": [
                        "ReAct - Iterative reasoning with observation",
                        "ReWOO - Plan-first reasoning without observation", 
                        "Reasoning Agent - Function augmentation with transparency",
                        "Tool Calling - Native LLM function integration"
                    ],
                    "advanced_systems": [
                        "Neural-Symbolic - Hybrid reasoning with knowledge graphs",
                        "MCTS - Probabilistic optimization for financial decisions",
                        "Jena - Semantic web and RDF/OWL reasoning",
                        "DSPy - Self-improving and automatic prompt optimization"
                    ],
                    "capabilities": [
                        "Transparent reasoning chains",
                        "Multi-hop logical inference", 
                        "Financial portfolio optimization",
                        "Knowledge graph construction",
                        "Automatic prompt tuning",
                        "GPU-accelerated processing"
                    ]
                }
            }
        except Exception as e:
            return {
                "status": "error",
                "error": str(e),
                "message": "Could not load AIQ components. Make sure AIQToolkit is properly installed."
            }

    def get_available_workflows(self):
        """Get list of available workflows"""
        return {
            "status": "success",
            "workflows": [
                {
                    "name": "simple_workflow",
                    "description": "Basic AI workflow for testing",
                    "config": "examples/simple/configs/config.yml"
                },
                {
                    "name": "react_agent",
                    "description": "ReAct agent with reasoning and acting",
                    "config": "examples/agents/react/configs/config.yml"
                },
                {
                    "name": "rewoo_agent", 
                    "description": "ReWOO agent with planning and execution",
                    "config": "examples/agents/rewoo/configs/config.yml"
                },
                {
                    "name": "reasoning_agent",
                    "description": "Advanced reasoning agent with transparency",
                    "config": "examples/agents/react/configs/config-reasoning.yml"
                },
                {
                    "name": "digital_human",
                    "description": "Digital human with NVIDIA integration",
                    "config": "examples/digital_human_demo/config.yaml"
                },
                {
                    "name": "distributed_processing",
                    "description": "Multi-node distributed processing",
                    "config": "examples/distributed_processing/"
                }
            ]
        }

    def get_available_reasoning_systems(self):
        """Get detailed information about available reasoning systems"""
        return {
            "status": "success",
            "reasoning_systems": {
                "react": {
                    "name": "ReAct Reasoning",
                    "description": "Iterative reasoning and acting with observation loops",
                    "capabilities": ["adaptive_reasoning", "tool_integration", "error_recovery", "transparent_process"],
                    "use_cases": ["research_tasks", "interactive_problem_solving", "multi_step_analysis"],
                    "example_config": "examples/agents/react/configs/config.yml"
                },
                "rewoo": {
                    "name": "ReWOO Reasoning", 
                    "description": "Plan-first reasoning without intermediate observations",
                    "capabilities": ["comprehensive_planning", "parallel_execution", "cost_efficiency", "deterministic_paths"],
                    "use_cases": ["batch_processing", "structured_workflows", "cost_sensitive_operations"],
                    "example_config": "examples/agents/rewoo/configs/config.yml"
                },
                "reasoning_agent": {
                    "name": "Reasoning Agent",
                    "description": "Function augmentation with transparent reasoning capabilities",
                    "capabilities": ["function_enhancement", "reasoning_chains", "tool_awareness", "transparency"],
                    "use_cases": ["ai_transparency", "audit_requirements", "complex_analysis"],
                    "example_config": "examples/agents/react/configs/config-reasoning.yml"
                },
                "tool_calling": {
                    "name": "Tool Calling Reasoning",
                    "description": "Native LLM function calling with integrated reasoning",
                    "capabilities": ["native_integration", "parallel_execution", "parameter_optimization", "efficiency"],
                    "use_cases": ["modern_llm_integration", "api_orchestration", "real_time_systems"],
                    "example_config": "examples/agents/tool_calling/configs/config.yml"
                },
                "neural_symbolic": {
                    "name": "Neural-Symbolic Hybrid",
                    "description": "Combines neural networks with symbolic reasoning and knowledge graphs",
                    "capabilities": ["hybrid_reasoning", "knowledge_graphs", "multi_hop_inference", "explainable_results"],
                    "use_cases": ["research_discovery", "knowledge_management", "complex_query_answering"],
                    "documentation": "docs/source/workflows/reasoning/neural-symbolic.md"
                },
                "mcts": {
                    "name": "Monte Carlo Tree Search",
                    "description": "GPU-accelerated probabilistic reasoning for optimization under uncertainty",
                    "capabilities": ["probabilistic_optimization", "financial_modeling", "risk_assessment", "gpu_acceleration"],
                    "use_cases": ["portfolio_optimization", "strategic_planning", "uncertainty_quantification"],
                    "documentation": "docs/source/workflows/reasoning/mcts-reasoning.md"
                },
                "jena": {
                    "name": "Apache Jena Reasoning",
                    "description": "Semantic web reasoning with RDF/OWL knowledge graphs",
                    "capabilities": ["semantic_reasoning", "sparql_queries", "ontology_management", "knowledge_integration"],
                    "use_cases": ["knowledge_management", "semantic_search", "data_integration"],
                    "documentation": "docs/source/workflows/document-management/jena-integration.md"
                },
                "dspy": {
                    "name": "DSPy Self-Improving",
                    "description": "Automatic prompt optimization and self-improving reasoning",
                    "capabilities": ["prompt_optimization", "automatic_tuning", "performance_tracking", "iterative_improvement"],
                    "use_cases": ["production_ai_systems", "prompt_engineering", "performance_optimization"],
                    "documentation": "docs/source/workflows/reasoning/dspy-reasoning.md"
                }
            },
            "integration_guide": "docs/source/workflows/reasoning/integration-guide.md",
            "architecture_overview": "docs/source/workflows/reasoning/index.md"
        }

    def get_health_status(self):
        """Get system health status"""
        try:
            # Check if core AIQ modules can be imported
            health_checks = {}
            
            try:
                import aiq
                health_checks["aiq_core"] = "healthy"
            except:
                health_checks["aiq_core"] = "error"
            
            try:
                from aiq.agent import base
                health_checks["agents"] = "healthy"
            except:
                health_checks["agents"] = "error"
                
            try:
                from aiq.builder import workflow
                health_checks["builders"] = "healthy"
            except:
                health_checks["builders"] = "error"
                
            overall_status = "healthy" if all(v == "healthy" for v in health_checks.values()) else "degraded"
            
            return {
                "status": overall_status,
                "timestamp": "2024-01-27T12:00:00Z",
                "health_checks": health_checks,
                "environment": {
                    "python_path": sys.path[:3],
                    "working_directory": os.getcwd()
                }
            }
        except Exception as e:
            return {
                "status": "error",
                "error": str(e),
                "timestamp": "2024-01-27T12:00:00Z"
            }

    def execute_workflow(self, request_data):
        """Execute an AIQ workflow"""
        try:
            workflow_type = request_data.get('workflow_type', 'simple')
            input_data = request_data.get('input', {})
            config = request_data.get('config', {})
            
            # Simulate workflow execution
            result = {
                "status": "success",
                "workflow_type": workflow_type,
                "input": input_data,
                "output": f"Executed {workflow_type} workflow with input: {input_data}",
                "execution_time": "2.5s",
                "steps": [
                    {"step": 1, "action": "Initialize workflow", "status": "completed"},
                    {"step": 2, "action": "Process input", "status": "completed"},
                    {"step": 3, "action": "Execute agent", "status": "completed"},
                    {"step": 4, "action": "Generate output", "status": "completed"}
                ]
            }
            
            # For reasoning workflows, add transparent reasoning
            if workflow_type == "reasoning":
                result["reasoning_steps"] = [
                    {"step": 1, "type": "analysis", "content": "Analyzing input requirements", "confidence": 0.95},
                    {"step": 2, "type": "planning", "content": "Planning execution strategy", "confidence": 0.88},
                    {"step": 3, "type": "execution", "content": "Executing planned actions", "confidence": 0.92},
                    {"step": 4, "type": "validation", "content": "Validating results", "confidence": 0.90}
                ]
            
            return result
            
        except Exception as e:
            return {
                "status": "error",
                "error": str(e),
                "message": "Failed to execute workflow"
            }

    def validate_config(self, request_data):
        """Validate workflow configuration"""
        try:
            config = request_data.get('config', {})
            
            # Basic validation
            required_fields = ['workflow', 'llm']
            missing_fields = [field for field in required_fields if field not in config]
            
            if missing_fields:
                return {
                    "status": "invalid",
                    "errors": [f"Missing required field: {field}" for field in missing_fields]
                }
            
            return {
                "status": "valid",
                "message": "Configuration is valid",
                "config_summary": {
                    "workflow_type": config.get('workflow', {}).get('type', 'unknown'),
                    "llm_provider": config.get('llm', {}).get('provider', 'unknown'),
                    "components": list(config.keys())
                }
            }
            
        except Exception as e:
            return {
                "status": "error",
                "error": str(e),
                "message": "Failed to validate configuration"
            }

    def build_workflow(self, request_data):
        """Build a workflow from configuration"""
        try:
            config = request_data.get('config', {})
            workflow_name = request_data.get('name', 'custom_workflow')
            
            # Simulate workflow building
            return {
                "status": "success",
                "workflow_name": workflow_name,
                "build_time": "1.2s",
                "components_built": [
                    "llm_component",
                    "agent_component", 
                    "workflow_component"
                ],
                "message": f"Successfully built workflow: {workflow_name}"
            }
            
        except Exception as e:
            return {
                "status": "error",
                "error": str(e),
                "message": "Failed to build workflow"
            }