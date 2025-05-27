# Reasoning Systems Configuration Guide

## Overview

This guide explains how to configure AIQToolkit reasoning systems with **shared parameters**, **individual customizations**, and **dynamic orchestration**. The configuration system supports both static setup and runtime modification of reasoning components.

## Configuration Architecture

```
┌─────────────────────────────────────────────────────────────────────────────────────┐
│                        Configuration Hierarchy                                     │
├─────────────────────────────────────────────────────────────────────────────────────┤
│                                                                                     │
│  ┌─────────────────────────────────────────────────────────────────────────────────┐ │
│  │                          Global Shared Config                                  │ │
│  │                                                                                 │ │
│  │  • Base LLM settings        • GPU/CUDA config       • Security settings       │ │
│  │  • Logging preferences      • Performance limits    • API rate limits        │ │
│  │  • Default timeouts         • Memory allocation     • Error handling          │ │
│  └─────────────────────────────────────────────────────────────────────────────────┘ │
│                                                                                     │
│  ┌─────────────────────────────────────────────────────────────────────────────────┐ │
│  │                       System-Specific Configs                                  │ │
│  │                                                                                 │ │
│  │  ┌─────────────┐ ┌─────────────┐ ┌─────────────┐ ┌─────────────┐             │ │
│  │  │ ReAct       │ │ MCTS        │ │ DSPy        │ │ Neural-Sym  │             │ │
│  │  │ Config      │ │ Config      │ │ Config      │ │ Config      │             │ │
│  │  │             │ │             │ │             │ │             │ │             │ │
│  │  │ • Max iter  │ │ • Sim count │ │ • Optimize  │ │ • KG endpoint│             │ │
│  │  │ • Tools     │ │ • GPU batch │ │ • Metrics   │ │ • Embed dim │             │ │
│  │  │ • Prompts   │ │ • Risk model│ │ • Examples  │ │ • Reasoning │             │ │
│  │  └─────────────┘ └─────────────┘ └─────────────┘ └─────────────┘             │ │
│  └─────────────────────────────────────────────────────────────────────────────────┘ │
│                                                                                     │
│  ┌─────────────────────────────────────────────────────────────────────────────────┐ │
│  │                        Runtime Orchestration                                   │ │
│  │                                                                                 │ │
│  │  • Dynamic system addition/removal    • Parameter hot-swapping                │ │
│  │  • Load balancing configurations      • A/B testing setups                    │ │
│  │  • Fallback chain management          • Performance monitoring                │ │
│  └─────────────────────────────────────────────────────────────────────────────────┘ │
└─────────────────────────────────────────────────────────────────────────────────────┘
```

## 1. Shared Configuration Parameters

### Global Base Configuration

```python
from typing import Dict, List, Any, Optional
from dataclasses import dataclass, field
from enum import Enum

@dataclass
class SharedReasoningConfig:
    """Shared configuration across all reasoning systems"""
    
    # Base LLM Configuration
    base_llm_provider: str = "openai"
    base_llm_model: str = "gpt-4"
    base_llm_temperature: float = 0.7
    base_llm_max_tokens: int = 2000
    base_llm_timeout: float = 30.0
    
    # GPU/CUDA Configuration
    enable_gpu: bool = True
    gpu_device: str = "cuda:0"
    gpu_memory_fraction: float = 0.8
    mixed_precision: bool = True
    
    # Performance Configuration
    max_concurrent_requests: int = 10
    request_timeout: float = 60.0
    retry_attempts: int = 3
    cache_enabled: bool = True
    cache_ttl_seconds: int = 3600
    
    # Logging and Monitoring
    log_level: str = "INFO"
    enable_detailed_logging: bool = False
    enable_performance_metrics: bool = True
    metrics_export_interval: int = 60
    
    # Security Configuration
    enable_input_validation: bool = True
    enable_output_sanitization: bool = True
    max_input_length: int = 100000
    sensitive_data_patterns: List[str] = field(default_factory=list)
    
    # Error Handling
    enable_graceful_degradation: bool = True
    fallback_to_simple_reasoning: bool = True
    error_escalation_threshold: int = 5

@dataclass 
class SystemSpecificConfig:
    """Base class for system-specific configurations"""
    
    # Override shared parameters
    llm_provider: Optional[str] = None
    llm_model: Optional[str] = None
    llm_temperature: Optional[float] = None
    
    # System enablement
    enabled: bool = True
    priority: int = 1  # Higher priority systems are preferred
    
    # Resource allocation
    cpu_cores: Optional[int] = None
    memory_limit_gb: Optional[float] = None
    gpu_allocation: Optional[float] = None  # Fraction of GPU to use
    
    # Performance tuning
    batch_size: Optional[int] = None
    max_parallel_executions: Optional[int] = None
    
    def merge_with_shared(self, shared_config: SharedReasoningConfig) -> Dict[str, Any]:
        """Merge system-specific config with shared config"""
        
        merged = {}
        
        # Use system-specific values if provided, otherwise use shared
        merged["llm_provider"] = self.llm_provider or shared_config.base_llm_provider
        merged["llm_model"] = self.llm_model or shared_config.base_llm_model
        merged["llm_temperature"] = self.llm_temperature or shared_config.base_llm_temperature
        merged["gpu_device"] = shared_config.gpu_device
        merged["enable_gpu"] = shared_config.enable_gpu
        merged["log_level"] = shared_config.log_level
        
        # Add system-specific parameters
        merged["enabled"] = self.enabled
        merged["priority"] = self.priority
        merged["batch_size"] = self.batch_size
        merged["max_parallel"] = self.max_parallel_executions
        
        return merged
```

### System-Specific Configurations

```python
@dataclass
class ReActConfig(SystemSpecificConfig):
    """ReAct reasoning system configuration"""
    
    # ReAct-specific parameters
    max_iterations: int = 10
    enable_retry_on_error: bool = True
    max_retries: int = 2
    detailed_logs: bool = False
    
    # Tool configuration
    available_tools: List[str] = field(default_factory=list)
    tool_timeout: float = 10.0
    enable_tool_validation: bool = True
    
    # Prompt configuration
    react_prompt_template: Optional[str] = None
    observation_prompt_template: Optional[str] = None
    
    # Performance tuning
    stop_on_final_answer: bool = True
    enable_intermediate_results: bool = True

@dataclass
class MCTSConfig(SystemSpecificConfig):
    """MCTS reasoning system configuration"""
    
    # MCTS Algorithm Parameters
    simulation_count: int = 10000
    exploration_constant: float = 1.414  # UCT exploration parameter
    max_depth: int = 50
    
    # GPU Configuration (inherits from shared but can override)
    gpu_batch_size: int = 512
    enable_gpu_acceleration: bool = True
    cuda_streams: int = 4
    
    # Financial Modeling Parameters
    enable_risk_assessment: bool = True
    risk_models: List[str] = field(default_factory=lambda: ["var", "cvar", "sharpe"])
    confidence_levels: List[float] = field(default_factory=lambda: [0.95, 0.99])
    
    # Optimization Parameters
    enable_adaptive_exploration: bool = True
    performance_tracking_window: int = 1000
    optimization_frequency: int = 5000

@dataclass
class DSPyConfig(SystemSpecificConfig):
    """DSPy self-improving system configuration"""
    
    # Optimization Parameters
    enable_optimization: bool = True
    optimization_metric: str = "accuracy"
    max_bootstrapped_demos: int = 8
    max_labeled_demos: int = 16
    
    # Learning Parameters
    optimization_frequency: int = 100  # Optimize every N examples
    min_examples_for_optimization: int = 50
    performance_improvement_threshold: float = 0.05  # 5% improvement required
    
    # Prompt Management
    enable_prompt_versioning: bool = True
    max_prompt_versions: int = 10
    prompt_rollback_enabled: bool = True
    
    # Training Data Management
    collect_user_feedback: bool = True
    feedback_buffer_size: int = 1000
    auto_labeling_confidence_threshold: float = 0.9

@dataclass
class NeuralSymbolicConfig(SystemSpecificConfig):
    """Neural-Symbolic hybrid system configuration"""
    
    # Knowledge Graph Configuration
    knowledge_graph_endpoint: str = "http://localhost:3030/sparql"
    enable_graph_reasoning: bool = True
    max_reasoning_hops: int = 3
    
    # Neural Component Configuration
    embedding_model: str = "sentence-transformers/all-mpnet-base-v2"
    embedding_dimension: int = 768
    similarity_threshold: float = 0.7
    
    # Hybrid Fusion Parameters
    neural_weight: float = 0.6
    symbolic_weight: float = 0.4
    fusion_strategy: str = "weighted_combination"  # "reranking", "cascading"
    
    # Performance Configuration
    vector_index_type: str = "hnsw"  # "flat", "ivf", "lsh"
    enable_caching: bool = True
    cache_embeddings: bool = True

@dataclass
class JenaConfig(SystemSpecificConfig):
    """Apache Jena reasoning system configuration"""
    
    # Jena Server Configuration
    sparql_endpoint: str = "http://localhost:3030/dataset/sparql"
    update_endpoint: str = "http://localhost:3030/dataset/update"
    graph_store_endpoint: str = "http://localhost:3030/dataset/data"
    
    # Authentication
    username: Optional[str] = None
    password: Optional[str] = None
    
    # Reasoning Configuration
    enable_owl_reasoning: bool = True
    reasoning_profile: str = "OWL_MEM_RDFS_INF"  # Jena reasoning profile
    enable_sparql_optimization: bool = True
    
    # Query Configuration
    default_query_timeout: int = 30
    max_query_results: int = 1000
    enable_query_caching: bool = True
```

## 2. Configuration Management System

```python
class ReasoningConfigManager:
    """Manages configuration for all reasoning systems"""
    
    def __init__(self, config_path: Optional[str] = None):
        self.shared_config = SharedReasoningConfig()
        self.system_configs: Dict[str, SystemSpecificConfig] = {}
        self.runtime_overrides: Dict[str, Dict[str, Any]] = {}
        
        if config_path:
            self.load_from_file(config_path)
    
    def set_shared_config(self, **kwargs) -> None:
        """Update shared configuration parameters"""
        for key, value in kwargs.items():
            if hasattr(self.shared_config, key):
                setattr(self.shared_config, key, value)
    
    def add_system_config(
        self, 
        system_name: str, 
        config: SystemSpecificConfig
    ) -> None:
        """Add or update system-specific configuration"""
        self.system_configs[system_name] = config
    
    def get_merged_config(self, system_name: str) -> Dict[str, Any]:
        """Get merged configuration for a specific system"""
        
        if system_name not in self.system_configs:
            raise ValueError(f"No configuration found for system: {system_name}")
        
        system_config = self.system_configs[system_name]
        merged = system_config.merge_with_shared(self.shared_config)
        
        # Apply runtime overrides
        if system_name in self.runtime_overrides:
            merged.update(self.runtime_overrides[system_name])
        
        return merged
    
    def set_runtime_override(
        self, 
        system_name: str, 
        parameter: str, 
        value: Any
    ) -> None:
        """Set runtime parameter override for a system"""
        
        if system_name not in self.runtime_overrides:
            self.runtime_overrides[system_name] = {}
        
        self.runtime_overrides[system_name][parameter] = value
    
    def remove_runtime_override(
        self, 
        system_name: str, 
        parameter: str = None
    ) -> None:
        """Remove runtime parameter overrides"""
        
        if system_name in self.runtime_overrides:
            if parameter:
                self.runtime_overrides[system_name].pop(parameter, None)
            else:
                del self.runtime_overrides[system_name]
    
    def export_config(self) -> Dict[str, Any]:
        """Export complete configuration"""
        
        return {
            "shared_config": self.shared_config.__dict__,
            "system_configs": {
                name: config.__dict__ 
                for name, config in self.system_configs.items()
            },
            "runtime_overrides": self.runtime_overrides
        }
    
    def load_from_file(self, config_path: str) -> None:
        """Load configuration from YAML file"""
        
        import yaml
        
        with open(config_path, 'r') as f:
            config_data = yaml.safe_load(f)
        
        # Load shared configuration
        if "shared" in config_data:
            shared_data = config_data["shared"]
            for key, value in shared_data.items():
                if hasattr(self.shared_config, key):
                    setattr(self.shared_config, key, value)
        
        # Load system configurations
        if "systems" in config_data:
            system_data = config_data["systems"]
            
            for system_name, system_config in system_data.items():
                config_class = self._get_config_class(system_name)
                if config_class:
                    config_instance = config_class(**system_config)
                    self.system_configs[system_name] = config_instance
    
    def _get_config_class(self, system_name: str) -> Optional[type]:
        """Get configuration class for system name"""
        
        config_classes = {
            "react": ReActConfig,
            "rewoo": ReWOOConfig,
            "mcts": MCTSConfig,
            "dspy": DSPyConfig,
            "neural_symbolic": NeuralSymbolicConfig,
            "jena": JenaConfig
        }
        
        return config_classes.get(system_name)

# Example usage
config_manager = ReasoningConfigManager()

# Set shared configuration
config_manager.set_shared_config(
    base_llm_model="gpt-4",
    enable_gpu=True,
    log_level="DEBUG"
)

# Add system-specific configurations
config_manager.add_system_config(
    "mcts",
    MCTSConfig(
        simulation_count=50000,
        gpu_batch_size=1024,
        enable_gpu_acceleration=True
    )
)

config_manager.add_system_config(
    "dspy",
    DSPyConfig(
        enable_optimization=True,
        optimization_frequency=200,
        max_bootstrapped_demos=16
    )
)

# Get merged configuration for MCTS
mcts_config = config_manager.get_merged_config("mcts")
print(f"MCTS LLM Model: {mcts_config['llm_model']}")  # Inherits from shared
print(f"MCTS Simulations: {mcts_config['simulation_count']}")  # System-specific
```

## 3. Dynamic System Orchestration

### Adding/Removing Reasoning Systems at Runtime

```python
class DynamicReasoningOrchestrator:
    """Dynamic orchestration of reasoning systems"""
    
    def __init__(self, config_manager: ReasoningConfigManager):
        self.config_manager = config_manager
        self.active_systems: Dict[str, Any] = {}
        self.system_registry = {}
        self.performance_monitor = {}
        
        # Register available system types
        self._register_system_types()
    
    def _register_system_types(self):
        """Register available reasoning system types"""
        
        from aiq.agent.react_agent import build_react_agent
        from aiq.agent.rewoo_agent import build_rewoo_agent
        from aiq.digital_human.financial import MCTSFinancialAnalyzer
        from aiq.digital_human.knowledge import DSPyFinancialProcessor
        from aiq.retriever.neural_symbolic import NeuralSymbolicRetriever
        from aiq.document_management.jena_integration import JenaIntegration
        
        self.system_registry = {
            "react": {
                "builder": build_react_agent,
                "config_class": ReActConfig,
                "requires_builder": True
            },
            "rewoo": {
                "builder": build_rewoo_agent,
                "config_class": ReWOOConfig,
                "requires_builder": True
            },
            "mcts": {
                "builder": MCTSFinancialAnalyzer,
                "config_class": MCTSConfig,
                "requires_builder": False
            },
            "dspy": {
                "builder": DSPyFinancialProcessor,
                "config_class": DSPyConfig,
                "requires_builder": False
            },
            "neural_symbolic": {
                "builder": NeuralSymbolicRetriever,
                "config_class": NeuralSymbolicConfig,
                "requires_builder": False
            },
            "jena": {
                "builder": JenaIntegration,
                "config_class": JenaConfig,
                "requires_builder": False
            }
        }
    
    async def add_reasoning_system(
        self, 
        system_name: str, 
        system_type: str,
        config_overrides: Dict[str, Any] = None
    ) -> bool:
        """Dynamically add a reasoning system"""
        
        try:
            if system_type not in self.system_registry:
                raise ValueError(f"Unknown system type: {system_type}")
            
            # Get system information
            system_info = self.system_registry[system_type]
            
            # Create configuration
            config_class = system_info["config_class"]
            base_config = config_class()
            
            # Apply overrides
            if config_overrides:
                for key, value in config_overrides.items():
                    if hasattr(base_config, key):
                        setattr(base_config, key, value)
            
            # Add to config manager
            self.config_manager.add_system_config(system_name, base_config)
            
            # Get merged configuration
            merged_config = self.config_manager.get_merged_config(system_name)
            
            # Build/initialize the system
            builder_func = system_info["builder"]
            
            if system_info["requires_builder"]:
                # Systems that need a builder (like ReAct, ReWOO)
                from aiq.builder.builder import Builder
                builder = Builder()  # Initialize builder
                system_instance = await builder_func(base_config, builder)
            else:
                # Direct instantiation systems (like MCTS, DSPy)
                system_instance = builder_func(**merged_config)
            
            # Add to active systems
            self.active_systems[system_name] = {
                "instance": system_instance,
                "type": system_type,
                "config": base_config,
                "status": "active",
                "added_at": datetime.now()
            }
            
            # Initialize performance monitoring
            self.performance_monitor[system_name] = {
                "total_requests": 0,
                "successful_requests": 0,
                "average_response_time": 0.0,
                "last_used": None
            }
            
            logger.info(f"Successfully added reasoning system: {system_name} ({system_type})")
            return True
            
        except Exception as e:
            logger.error(f"Failed to add reasoning system {system_name}: {e}")
            return False
    
    async def remove_reasoning_system(self, system_name: str) -> bool:
        """Dynamically remove a reasoning system"""
        
        try:
            if system_name not in self.active_systems:
                logger.warning(f"System {system_name} not found in active systems")
                return False
            
            # Get system instance
            system_info = self.active_systems[system_name]
            system_instance = system_info["instance"]
            
            # Graceful shutdown if supported
            if hasattr(system_instance, 'shutdown'):
                await system_instance.shutdown()
            
            # Remove from active systems
            del self.active_systems[system_name]
            
            # Remove from performance monitoring
            if system_name in self.performance_monitor:
                del self.performance_monitor[system_name]
            
            # Remove from config manager
            if system_name in self.config_manager.system_configs:
                del self.config_manager.system_configs[system_name]
            
            logger.info(f"Successfully removed reasoning system: {system_name}")
            return True
            
        except Exception as e:
            logger.error(f"Failed to remove reasoning system {system_name}: {e}")
            return False
    
    async def hot_swap_system_config(
        self, 
        system_name: str, 
        new_config: Dict[str, Any]
    ) -> bool:
        """Hot-swap configuration for an active system"""
        
        try:
            if system_name not in self.active_systems:
                logger.error(f"System {system_name} not active, cannot hot-swap")
                return False
            
            system_info = self.active_systems[system_name]
            system_type = system_info["type"]
            
            # Apply runtime overrides
            for key, value in new_config.items():
                self.config_manager.set_runtime_override(system_name, key, value)
            
            # Check if the system supports hot configuration updates
            system_instance = system_info["instance"]
            
            if hasattr(system_instance, 'update_config'):
                # System supports hot updates
                merged_config = self.config_manager.get_merged_config(system_name)
                await system_instance.update_config(merged_config)
                logger.info(f"Hot-swapped config for {system_name}")
                return True
            else:
                # Need to recreate the system
                logger.info(f"Recreating {system_name} with new configuration")
                
                # Remove old system
                await self.remove_reasoning_system(system_name)
                
                # Add new system with updated config
                return await self.add_reasoning_system(
                    system_name, 
                    system_type, 
                    new_config
                )
                
        except Exception as e:
            logger.error(f"Failed to hot-swap config for {system_name}: {e}")
            return False
    
    def get_active_systems(self) -> Dict[str, Any]:
        """Get information about active reasoning systems"""
        
        active_info = {}
        
        for name, system_info in self.active_systems.items():
            performance = self.performance_monitor.get(name, {})
            
            active_info[name] = {
                "type": system_info["type"],
                "status": system_info["status"],
                "added_at": system_info["added_at"].isoformat(),
                "performance": performance,
                "config_summary": {
                    key: getattr(system_info["config"], key)
                    for key in ["enabled", "priority"]
                    if hasattr(system_info["config"], key)
                }
            }
        
        return active_info
    
    async def rebalance_system_priorities(self, new_priorities: Dict[str, int]) -> None:
        """Rebalance system priorities for routing"""
        
        for system_name, priority in new_priorities.items():
            if system_name in self.active_systems:
                self.config_manager.set_runtime_override(
                    system_name, 
                    "priority", 
                    priority
                )
                
                # Update system config
                system_config = self.active_systems[system_name]["config"]
                if hasattr(system_config, "priority"):
                    system_config.priority = priority
        
        logger.info(f"Rebalanced system priorities: {new_priorities}")
```

## 4. Configuration Examples

### Complete YAML Configuration

```yaml
# reasoning_systems_config.yaml

shared:
  # Base LLM Configuration
  base_llm_provider: "openai"
  base_llm_model: "gpt-4"
  base_llm_temperature: 0.7
  base_llm_max_tokens: 2000
  
  # GPU Configuration
  enable_gpu: true
  gpu_device: "cuda:0"
  gpu_memory_fraction: 0.8
  mixed_precision: true
  
  # Performance
  max_concurrent_requests: 20
  request_timeout: 60.0
  cache_enabled: true
  cache_ttl_seconds: 3600
  
  # Logging
  log_level: "INFO"
  enable_detailed_logging: true
  enable_performance_metrics: true

systems:
  react:
    enabled: true
    priority: 3
    max_iterations: 15
    enable_retry_on_error: true
    available_tools: ["web_search", "calculator", "code_execution"]
    llm_temperature: 0.5  # Override shared temperature
    
  rewoo:
    enabled: true
    priority: 2
    max_planning_iterations: 5
    enable_parallel_execution: true
    
  mcts:
    enabled: true
    priority: 5  # High priority for financial tasks
    simulation_count: 50000
    exploration_constant: 1.414
    gpu_batch_size: 1024
    enable_risk_assessment: true
    risk_models: ["var", "cvar", "sharpe", "sortino"]
    
  dspy:
    enabled: true
    priority: 4
    enable_optimization: true
    optimization_frequency: 150
    max_bootstrapped_demos: 12
    llm_model: "gpt-4-turbo"  # Override shared model
    
  neural_symbolic:
    enabled: false  # Disabled by default
    priority: 3
    knowledge_graph_endpoint: "http://localhost:3030/sparql"
    embedding_model: "sentence-transformers/all-mpnet-base-v2"
    max_reasoning_hops: 4
    neural_weight: 0.7
    symbolic_weight: 0.3
    
  jena:
    enabled: false  # Disabled by default
    priority: 2
    sparql_endpoint: "http://localhost:3030/dataset/sparql"
    enable_owl_reasoning: true
    reasoning_profile: "OWL_MEM_RDFS_INF"
```

### Runtime Configuration Management

```python
# Example: Dynamic system management
async def main():
    # Initialize configuration manager
    config_manager = ReasoningConfigManager("reasoning_systems_config.yaml")
    
    # Initialize orchestrator
    orchestrator = DynamicReasoningOrchestrator(config_manager)
    
    # Add systems dynamically
    await orchestrator.add_reasoning_system(
        system_name="financial_mcts",
        system_type="mcts",
        config_overrides={
            "simulation_count": 100000,
            "enable_gpu_acceleration": True,
            "gpu_batch_size": 2048
        }
    )
    
    await orchestrator.add_reasoning_system(
        system_name="research_react", 
        system_type="react",
        config_overrides={
            "max_iterations": 20,
            "available_tools": ["web_search", "arxiv_search", "wikipedia"]
        }
    )
    
    # Hot-swap configuration
    await orchestrator.hot_swap_system_config(
        "financial_mcts",
        {
            "simulation_count": 150000,
            "exploration_constant": 1.8
        }
    )
    
    # Get active systems status
    active_systems = orchestrator.get_active_systems()
    print("Active systems:", active_systems)
    
    # Rebalance priorities based on performance
    await orchestrator.rebalance_system_priorities({
        "financial_mcts": 10,  # Highest priority
        "research_react": 5
    })
    
    # Remove system when no longer needed
    await orchestrator.remove_reasoning_system("research_react")

if __name__ == "__main__":
    asyncio.run(main())
```

## 5. Advanced Configuration Patterns

### A/B Testing Configuration

```python
class ABTestingConfigManager:
    """Manage A/B testing of reasoning system configurations"""
    
    def __init__(self):
        self.test_configs = {}
        self.traffic_split = {}
        self.performance_metrics = {}
    
    def setup_ab_test(
        self,
        system_name: str,
        config_a: Dict[str, Any],
        config_b: Dict[str, Any],
        traffic_split: float = 0.5
    ):
        """Setup A/B test for system configuration"""
        
        self.test_configs[system_name] = {
            "config_a": config_a,
            "config_b": config_b
        }
        self.traffic_split[system_name] = traffic_split
        self.performance_metrics[system_name] = {
            "a": {"requests": 0, "total_time": 0, "success_rate": 0},
            "b": {"requests": 0, "total_time": 0, "success_rate": 0}
        }
    
    def get_config_for_request(self, system_name: str, request_id: str) -> Dict[str, Any]:
        """Get configuration variant for a specific request"""
        
        import hashlib
        
        # Use request ID to deterministically assign variant
        hash_value = int(hashlib.md5(request_id.encode()).hexdigest(), 16)
        split_point = self.traffic_split.get(system_name, 0.5)
        
        if (hash_value % 100) / 100 < split_point:
            return self.test_configs[system_name]["config_a"]
        else:
            return self.test_configs[system_name]["config_b"]
```

This configuration guide provides comprehensive control over your reasoning systems, enabling both shared parameter management and dynamic orchestration of individual systems within the neural network architecture.