# Neural Network Orchestration Guide

## Overview

This guide explains how to **dynamically add, remove, and orchestrate reasoning processes** within AIQToolkit's neural network architecture. The system supports runtime modification of the reasoning pipeline, allowing you to adapt the neural network topology based on performance, requirements, and context.

## Neural Orchestration Architecture

```
┌─────────────────────────────────────────────────────────────────────────────────────┐
│                        Neural Reasoning Orchestration                              │
├─────────────────────────────────────────────────────────────────────────────────────┤
│                                                                                     │
│  ┌─────────────────────────────────────────────────────────────────────────────────┐ │
│  │                          Neural Router & Dispatcher                            │ │
│  │                                                                                 │ │
│  │  ┌───────────────┐    ┌───────────────┐    ┌───────────────┐                 │ │
│  │  │ Query         │    │ Route         │    │ Load          │                 │ │
│  │  │ Analysis      │───▶│ Selection     │───▶│ Balancing     │                 │ │
│  │  │               │    │               │    │               │                 │ │
│  │  │ • Complexity  │    │ • Best System │    │ • Resource    │                 │ │
│  │  │ • Domain      │    │ • Fallbacks   │    │ • Priority    │                 │ │
│  │  │ • Context     │    │ • Parallel    │    │ • Queue Mgmt  │                 │ │
│  │  └───────────────┘    └───────────────┘    └───────────────┘                 │ │
│  └─────────────────────────────────────────────────────────────────────────────────┘ │
│                                                                                     │
│  ┌─────────────────────────────────────────────────────────────────────────────────┐ │
│  │                         Dynamic Reasoning Network                               │ │
│  │                                                                                 │ │
│  │  ┌─────────────┐ ┌─────────────┐ ┌─────────────┐ ┌─────────────┐             │ │
│  │  │ Reasoning   │ │ Reasoning   │ │ Reasoning   │ │ Reasoning   │             │ │
│  │  │ Node 1      │ │ Node 2      │ │ Node 3      │ │ Node N      │             │ │
│  │  │             │ │             │ │             │ │             │             │ │
│  │  │ [ReAct]     │ │ [MCTS]      │ │ [DSPy]      │ │ [Neural-    │             │ │
│  │  │ Status:●    │ │ Status:●    │ │ Status:○    │ │  Symbolic]  │             │ │
│  │  │ Load: 45%   │ │ Load: 78%   │ │ Load: 12%   │ │ Status:●    │             │ │
│  │  │ Queue: 3    │ │ Queue: 8    │ │ Queue: 1    │ │ Load: 23%   │             │ │
│  │  └─────────────┘ └─────────────┘ └─────────────┘ └─────────────┘             │ │
│  │         │               │               │               │                     │ │
│  │         └───────────────┼───────────────┼───────────────┘                     │ │
│  │                         │               │                                     │ │
│  │  ┌─────────────────────────────────────────────────────────────────────────── │ │
│  │  │                     Result Fusion & Synthesis                             │ │
│  │  │                                                                            │ │
│  │  │  • Weighted combination    • Confidence scoring    • Output validation    │ │
│  │  │  • Consensus mechanisms    • Error detection       • Quality assurance   │ │
│  │  └──────────────────────────────────────────────────────────────────────────── │ │
│  └─────────────────────────────────────────────────────────────────────────────────┘ │
│                                                                                     │
│  ┌─────────────────────────────────────────────────────────────────────────────────┐ │
│  │                        Neural Network Management                               │ │
│  │                                                                                 │ │
│  │  • Dynamic node addition/removal     • Real-time performance monitoring       │ │
│  │  • Automatic scaling & load balancing • Health checks & failure recovery      │ │
│  │  • Configuration hot-swapping        • Memory & GPU resource management       │ │
│  └─────────────────────────────────────────────────────────────────────────────────┘ │
└─────────────────────────────────────────────────────────────────────────────────────┘
```

## 1. Neural Network Node Management

### Reasoning Node Architecture

```python
from typing import Dict, List, Any, Optional, Callable, Union
from dataclasses import dataclass, field
from enum import Enum
import asyncio
import time
import logging
from abc import ABC, abstractmethod

class NodeStatus(Enum):
    """Status of a reasoning node"""
    INITIALIZING = "initializing"
    ACTIVE = "active"
    BUSY = "busy"
    OVERLOADED = "overloaded"
    MAINTENANCE = "maintenance"
    ERROR = "error"
    SHUTDOWN = "shutdown"

@dataclass
class ReasoningNodeMetrics:
    """Performance metrics for a reasoning node"""
    
    # Request metrics
    total_requests: int = 0
    successful_requests: int = 0
    failed_requests: int = 0
    current_queue_size: int = 0
    
    # Performance metrics
    average_response_time: float = 0.0
    min_response_time: float = float('inf')
    max_response_time: float = 0.0
    
    # Resource metrics
    cpu_usage_percent: float = 0.0
    memory_usage_mb: float = 0.0
    gpu_memory_usage_mb: float = 0.0
    
    # Quality metrics
    average_confidence_score: float = 0.0
    error_rate: float = 0.0
    
    # Temporal metrics
    last_request_time: Optional[float] = None
    node_uptime_seconds: float = 0.0
    
    def calculate_success_rate(self) -> float:
        """Calculate success rate percentage"""
        if self.total_requests == 0:
            return 0.0
        return (self.successful_requests / self.total_requests) * 100
    
    def calculate_load_factor(self) -> float:
        """Calculate current load factor (0.0 to 1.0+)"""
        # Base load on queue size and response time
        queue_factor = min(self.current_queue_size / 10.0, 1.0)  # Normalize to max 10 queue
        time_factor = min(self.average_response_time / 30.0, 1.0)  # Normalize to 30s max
        
        return (queue_factor + time_factor) / 2

class ReasoningNode(ABC):
    """Abstract base class for reasoning nodes in the neural network"""
    
    def __init__(
        self,
        node_id: str,
        node_type: str,
        config: Dict[str, Any],
        max_concurrent_requests: int = 5
    ):
        self.node_id = node_id
        self.node_type = node_type
        self.config = config
        self.max_concurrent_requests = max_concurrent_requests
        
        # Node state
        self.status = NodeStatus.INITIALIZING
        self.metrics = ReasoningNodeMetrics()
        self.request_queue: asyncio.Queue = asyncio.Queue()
        self.active_requests: Dict[str, asyncio.Task] = {}
        
        # Timing
        self.created_at = time.time()
        self.last_health_check = time.time()
        
        # Callbacks
        self.status_change_callbacks: List[Callable] = []
        self.metrics_update_callbacks: List[Callable] = []
        
        # Logging
        self.logger = logging.getLogger(f"ReasoningNode.{node_id}")
    
    async def initialize(self) -> bool:
        """Initialize the reasoning node"""
        try:
            self.status = NodeStatus.INITIALIZING
            success = await self._initialize_reasoning_system()
            
            if success:
                self.status = NodeStatus.ACTIVE
                self.logger.info(f"Node {self.node_id} initialized successfully")
                await self._notify_status_change()
                return True
            else:
                self.status = NodeStatus.ERROR
                self.logger.error(f"Node {self.node_id} initialization failed")
                return False
                
        except Exception as e:
            self.status = NodeStatus.ERROR
            self.logger.error(f"Node {self.node_id} initialization error: {e}")
            return False
    
    @abstractmethod
    async def _initialize_reasoning_system(self) -> bool:
        """Initialize the specific reasoning system"""
        pass
    
    async def process_request(
        self,
        request_id: str,
        query: str,
        context: Dict[str, Any] = None
    ) -> Dict[str, Any]:
        """Process a reasoning request"""
        
        start_time = time.time()
        self.metrics.total_requests += 1
        self.metrics.last_request_time = start_time
        
        try:
            # Check if node can accept request
            if not self._can_accept_request():
                raise Exception(f"Node {self.node_id} overloaded")
            
            # Update status to busy if at capacity
            if len(self.active_requests) >= self.max_concurrent_requests - 1:
                self.status = NodeStatus.BUSY
            
            # Process the request
            result = await self._process_reasoning_request(request_id, query, context)
            
            # Update metrics
            response_time = time.time() - start_time
            self._update_response_time_metrics(response_time)
            self.metrics.successful_requests += 1
            
            # Update confidence metric
            if 'confidence' in result:
                confidence = result['confidence']
                self._update_confidence_metrics(confidence)
            
            # Update status back to active if not overloaded
            if len(self.active_requests) < self.max_concurrent_requests:
                self.status = NodeStatus.ACTIVE
            
            await self._notify_metrics_update()
            return result
            
        except Exception as e:
            self.metrics.failed_requests += 1
            self.logger.error(f"Request {request_id} failed on node {self.node_id}: {e}")
            
            return {
                "error": str(e),
                "node_id": self.node_id,
                "request_id": request_id,
                "failed": True
            }
        
        finally:
            # Clean up request tracking
            if request_id in self.active_requests:
                del self.active_requests[request_id]
    
    @abstractmethod
    async def _process_reasoning_request(
        self,
        request_id: str,
        query: str,
        context: Dict[str, Any] = None
    ) -> Dict[str, Any]:
        """Process the actual reasoning request"""
        pass
    
    def _can_accept_request(self) -> bool:
        """Check if node can accept new requests"""
        return (
            self.status in [NodeStatus.ACTIVE, NodeStatus.BUSY] and
            len(self.active_requests) < self.max_concurrent_requests and
            self.request_queue.qsize() < self.max_concurrent_requests * 2
        )
    
    def _update_response_time_metrics(self, response_time: float):
        """Update response time metrics"""
        
        # Update min/max
        self.metrics.min_response_time = min(self.metrics.min_response_time, response_time)
        self.metrics.max_response_time = max(self.metrics.max_response_time, response_time)
        
        # Update average (exponential moving average)
        alpha = 0.1  # Smoothing factor
        if self.metrics.average_response_time == 0:
            self.metrics.average_response_time = response_time
        else:
            self.metrics.average_response_time = (
                alpha * response_time + 
                (1 - alpha) * self.metrics.average_response_time
            )
    
    def _update_confidence_metrics(self, confidence: float):
        """Update confidence score metrics"""
        
        alpha = 0.1  # Smoothing factor
        if self.metrics.average_confidence_score == 0:
            self.metrics.average_confidence_score = confidence
        else:
            self.metrics.average_confidence_score = (
                alpha * confidence + 
                (1 - alpha) * self.metrics.average_confidence_score
            )
    
    async def health_check(self) -> Dict[str, Any]:
        """Perform health check on the node"""
        
        current_time = time.time()
        self.metrics.node_uptime_seconds = current_time - self.created_at
        
        # Calculate error rate
        if self.metrics.total_requests > 0:
            self.metrics.error_rate = (
                self.metrics.failed_requests / self.metrics.total_requests
            ) * 100
        
        # Update queue size
        self.metrics.current_queue_size = self.request_queue.qsize()
        
        # Check if node is healthy
        is_healthy = (
            self.status != NodeStatus.ERROR and
            self.metrics.error_rate < 50.0 and  # Less than 50% error rate
            self.metrics.current_queue_size < self.max_concurrent_requests * 3
        )
        
        health_status = {
            "node_id": self.node_id,
            "node_type": self.node_type,
            "status": self.status.value,
            "is_healthy": is_healthy,
            "metrics": self.metrics.__dict__,
            "load_factor": self.metrics.calculate_load_factor(),
            "success_rate": self.metrics.calculate_success_rate(),
            "last_health_check": current_time
        }
        
        self.last_health_check = current_time
        return health_status
    
    async def shutdown(self):
        """Gracefully shutdown the node"""
        
        self.status = NodeStatus.SHUTDOWN
        
        # Wait for active requests to complete (with timeout)
        if self.active_requests:
            await asyncio.wait_for(
                asyncio.gather(*self.active_requests.values(), return_exceptions=True),
                timeout=30.0
            )
        
        # Perform cleanup
        await self._cleanup_reasoning_system()
        
        self.logger.info(f"Node {self.node_id} shutdown complete")
    
    async def _cleanup_reasoning_system(self):
        """Cleanup reasoning system resources"""
        pass
    
    async def _notify_status_change(self):
        """Notify registered callbacks of status changes"""
        for callback in self.status_change_callbacks:
            try:
                await callback(self.node_id, self.status)
            except Exception as e:
                self.logger.error(f"Status change callback error: {e}")
    
    async def _notify_metrics_update(self):
        """Notify registered callbacks of metrics updates"""
        for callback in self.metrics_update_callbacks:
            try:
                await callback(self.node_id, self.metrics)
            except Exception as e:
                self.logger.error(f"Metrics update callback error: {e}")
```

### Specific Reasoning Node Implementations

```python
class ReActReasoningNode(ReasoningNode):
    """ReAct reasoning node implementation"""
    
    async def _initialize_reasoning_system(self) -> bool:
        """Initialize ReAct reasoning system"""
        try:
            from aiq.agent.react_agent import build_react_agent
            from aiq.builder.builder import Builder
            
            builder = Builder()
            self.react_agent = await build_react_agent(self.config, builder)
            return True
            
        except Exception as e:
            self.logger.error(f"ReAct initialization failed: {e}")
            return False
    
    async def _process_reasoning_request(
        self,
        request_id: str,
        query: str,
        context: Dict[str, Any] = None
    ) -> Dict[str, Any]:
        """Process ReAct reasoning request"""
        
        result = await self.react_agent.run(query, context=context or {})
        
        return {
            "answer": result.get("answer", str(result)),
            "reasoning_chain": result.get("reasoning_chain", []),
            "confidence": result.get("confidence", 0.8),
            "node_id": self.node_id,
            "node_type": self.node_type,
            "request_id": request_id
        }

class MCTSReasoningNode(ReasoningNode):
    """MCTS reasoning node implementation"""
    
    async def _initialize_reasoning_system(self) -> bool:
        """Initialize MCTS reasoning system"""
        try:
            from aiq.digital_human.financial import MCTSFinancialAnalyzer
            
            self.mcts_analyzer = MCTSFinancialAnalyzer(
                device=self.config.get("gpu_device", "cuda"),
                simulation_count=self.config.get("simulation_count", 10000),
                exploration_constant=self.config.get("exploration_constant", 1.414),
                enable_gpu_acceleration=self.config.get("enable_gpu_acceleration", True)
            )
            return True
            
        except Exception as e:
            self.logger.error(f"MCTS initialization failed: {e}")
            return False
    
    async def _process_reasoning_request(
        self,
        request_id: str,
        query: str,
        context: Dict[str, Any] = None
    ) -> Dict[str, Any]:
        """Process MCTS reasoning request"""
        
        # Extract portfolio information from context
        portfolio = context.get("portfolio", {})
        risk_constraints = context.get("risk_constraints", {})
        market_data = context.get("market_data", {})
        
        if not portfolio:
            return {
                "error": "MCTS requires portfolio data",
                "node_id": self.node_id,
                "request_id": request_id
            }
        
        # Run MCTS optimization
        result = await self.mcts_analyzer.optimize_portfolio(
            current_portfolio=portfolio,
            risk_constraints=risk_constraints,
            market_data=market_data
        )
        
        return {
            "answer": f"Optimized portfolio allocation: {result.optimal_allocation}",
            "optimization_details": {
                "expected_return": result.expected_return,
                "risk_score": result.risk_score,
                "sharpe_ratio": result.sharpe_ratio
            },
            "confidence": result.confidence_score,
            "reasoning_chain": result.reasoning_path,
            "node_id": self.node_id,
            "node_type": self.node_type,
            "request_id": request_id
        }

class DSPyReasoningNode(ReasoningNode):
    """DSPy reasoning node implementation"""
    
    async def _initialize_reasoning_system(self) -> bool:
        """Initialize DSPy reasoning system"""
        try:
            from aiq.digital_human.knowledge import DSPyFinancialProcessor
            
            self.dspy_processor = DSPyFinancialProcessor(
                llm_model=self.config.get("llm_model", "gpt-4"),
                optimization_metric=self.config.get("optimization_metric", "accuracy"),
                max_bootstrapped_demos=self.config.get("max_bootstrapped_demos", 8)
            )
            return True
            
        except Exception as e:
            self.logger.error(f"DSPy initialization failed: {e}")
            return False
    
    async def _process_reasoning_request(
        self,
        request_id: str,
        query: str,
        context: Dict[str, Any] = None
    ) -> Dict[str, Any]:
        """Process DSPy reasoning request"""
        
        financial_data = context.get("financial_data", {})
        user_context = context.get("user_context", {})
        
        result = await self.dspy_processor.generate_analysis(
            financial_data=financial_data,
            user_context=user_context,
            query=query
        )
        
        return {
            "answer": result["analysis"],
            "recommendations": result["recommendations"],
            "risk_assessment": result["risk_assessment"],
            "confidence": result["confidence_score"],
            "supporting_evidence": result["supporting_evidence"],
            "node_id": self.node_id,
            "node_type": self.node_type,
            "request_id": request_id
        }
```

## 2. Neural Network Orchestrator

```python
class NeuralReasoningOrchestrator:
    """Orchestrates dynamic reasoning network with automatic scaling"""
    
    def __init__(self, max_nodes_per_type: int = 5):
        self.nodes: Dict[str, ReasoningNode] = {}
        self.node_types: Dict[str, type] = {}
        self.max_nodes_per_type = max_nodes_per_type
        
        # Load balancing
        self.request_router = RequestRouter()
        self.load_balancer = LoadBalancer()
        
        # Monitoring
        self.health_monitor = HealthMonitor()
        self.performance_tracker = PerformanceTracker()
        
        # Auto-scaling
        self.auto_scaler = AutoScaler(self)
        
        # Logging
        self.logger = logging.getLogger("NeuralReasoningOrchestrator")
        
        # Register node types
        self._register_node_types()
    
    def _register_node_types(self):
        """Register available reasoning node types"""
        
        self.node_types = {
            "react": ReActReasoningNode,
            "rewoo": ReWOOReasoningNode,
            "mcts": MCTSReasoningNode,
            "dspy": DSPyReasoningNode,
            "neural_symbolic": NeuralSymbolicReasoningNode,
            "jena": JenaReasoningNode
        }
    
    async def add_reasoning_node(
        self,
        node_type: str,
        config: Dict[str, Any],
        node_id: Optional[str] = None
    ) -> str:
        """Add a new reasoning node to the network"""
        
        if node_type not in self.node_types:
            raise ValueError(f"Unknown node type: {node_type}")
        
        # Generate node ID if not provided
        if node_id is None:
            existing_count = len([n for n in self.nodes.keys() if n.startswith(node_type)])
            node_id = f"{node_type}_{existing_count + 1}"
        
        # Check if we can add more nodes of this type
        type_count = len([n for n in self.nodes.values() if n.node_type == node_type])
        if type_count >= self.max_nodes_per_type:
            raise Exception(f"Maximum nodes of type {node_type} reached")
        
        # Create and initialize node
        node_class = self.node_types[node_type]
        node = node_class(
            node_id=node_id,
            node_type=node_type,
            config=config,
            max_concurrent_requests=config.get("max_concurrent_requests", 5)
        )
        
        # Register callbacks
        node.status_change_callbacks.append(self._handle_node_status_change)
        node.metrics_update_callbacks.append(self._handle_node_metrics_update)
        
        # Initialize the node
        success = await node.initialize()
        if not success:
            raise Exception(f"Failed to initialize node {node_id}")
        
        # Add to network
        self.nodes[node_id] = node
        
        # Register with router and load balancer
        await self.request_router.register_node(node)
        await self.load_balancer.register_node(node)
        
        self.logger.info(f"Added reasoning node: {node_id} ({node_type})")
        return node_id
    
    async def remove_reasoning_node(self, node_id: str) -> bool:
        """Remove a reasoning node from the network"""
        
        if node_id not in self.nodes:
            self.logger.warning(f"Node {node_id} not found")
            return False
        
        node = self.nodes[node_id]
        
        try:
            # Unregister from router and load balancer
            await self.request_router.unregister_node(node)
            await self.load_balancer.unregister_node(node)
            
            # Shutdown the node
            await node.shutdown()
            
            # Remove from network
            del self.nodes[node_id]
            
            self.logger.info(f"Removed reasoning node: {node_id}")
            return True
            
        except Exception as e:
            self.logger.error(f"Failed to remove node {node_id}: {e}")
            return False
    
    async def process_reasoning_request(
        self,
        query: str,
        context: Dict[str, Any] = None,
        preferred_nodes: List[str] = None,
        fallback_enabled: bool = True
    ) -> Dict[str, Any]:
        """Process a reasoning request through the network"""
        
        request_id = f"req_{int(time.time() * 1000)}"
        start_time = time.time()
        
        try:
            # Route request to appropriate node(s)
            selected_nodes = await self.request_router.route_request(
                query=query,
                context=context,
                preferred_nodes=preferred_nodes,
                available_nodes=list(self.nodes.keys())
            )
            
            if not selected_nodes:
                return {
                    "error": "No available nodes for request",
                    "request_id": request_id
                }
            
            # Process request with load balancing
            result = await self.load_balancer.process_request(
                request_id=request_id,
                query=query,
                context=context,
                selected_nodes=selected_nodes,
                fallback_enabled=fallback_enabled
            )
            
            # Track performance
            execution_time = time.time() - start_time
            await self.performance_tracker.track_request(
                request_id=request_id,
                query=query,
                result=result,
                execution_time=execution_time,
                nodes_used=selected_nodes
            )
            
            return result
            
        except Exception as e:
            self.logger.error(f"Request {request_id} failed: {e}")
            return {
                "error": str(e),
                "request_id": request_id,
                "failed": True
            }
    
    async def scale_network(
        self,
        target_capacity: Dict[str, int],
        scale_up_strategy: str = "gradual"
    ) -> Dict[str, Any]:
        """Scale the reasoning network to target capacity"""
        
        scaling_result = {
            "added_nodes": [],
            "removed_nodes": [],
            "errors": []
        }
        
        for node_type, target_count in target_capacity.items():
            current_count = len([
                n for n in self.nodes.values() 
                if n.node_type == node_type
            ])
            
            if target_count > current_count:
                # Scale up
                nodes_to_add = target_count - current_count
                
                for i in range(nodes_to_add):
                    try:
                        # Use default config for new nodes
                        default_config = self._get_default_config(node_type)
                        node_id = await self.add_reasoning_node(node_type, default_config)
                        scaling_result["added_nodes"].append(node_id)
                        
                        # Gradual scaling - add delay between nodes
                        if scale_up_strategy == "gradual":
                            await asyncio.sleep(2.0)
                            
                    except Exception as e:
                        scaling_result["errors"].append(f"Failed to add {node_type}: {e}")
            
            elif target_count < current_count:
                # Scale down
                nodes_to_remove = current_count - target_count
                type_nodes = [
                    (node_id, node) for node_id, node in self.nodes.items()
                    if node.node_type == node_type
                ]
                
                # Remove nodes with lowest utilization first
                type_nodes.sort(key=lambda x: x[1].metrics.calculate_load_factor())
                
                for node_id, node in type_nodes[:nodes_to_remove]:
                    try:
                        success = await self.remove_reasoning_node(node_id)
                        if success:
                            scaling_result["removed_nodes"].append(node_id)
                    except Exception as e:
                        scaling_result["errors"].append(f"Failed to remove {node_id}: {e}")
        
        return scaling_result
    
    async def get_network_status(self) -> Dict[str, Any]:
        """Get comprehensive network status"""
        
        node_statuses = {}
        type_summary = {}
        
        for node_id, node in self.nodes.items():
            health_status = await node.health_check()
            node_statuses[node_id] = health_status
            
            # Aggregate by type
            node_type = node.node_type
            if node_type not in type_summary:
                type_summary[node_type] = {
                    "total_nodes": 0,
                    "active_nodes": 0,
                    "total_requests": 0,
                    "average_load_factor": 0.0,
                    "average_success_rate": 0.0
                }
            
            type_summary[node_type]["total_nodes"] += 1
            if health_status["is_healthy"]:
                type_summary[node_type]["active_nodes"] += 1
            
            type_summary[node_type]["total_requests"] += health_status["metrics"]["total_requests"]
            type_summary[node_type]["average_load_factor"] += health_status["load_factor"]
            type_summary[node_type]["average_success_rate"] += health_status["success_rate"]
        
        # Calculate averages
        for node_type, summary in type_summary.items():
            if summary["total_nodes"] > 0:
                summary["average_load_factor"] /= summary["total_nodes"]
                summary["average_success_rate"] /= summary["total_nodes"]
        
        return {
            "network_summary": {
                "total_nodes": len(self.nodes),
                "active_nodes": len([n for n in node_statuses.values() if n["is_healthy"]]),
                "node_types": list(type_summary.keys())
            },
            "node_statuses": node_statuses,
            "type_summary": type_summary,
            "auto_scaler_status": await self.auto_scaler.get_status()
        }
    
    async def _handle_node_status_change(self, node_id: str, status: NodeStatus):
        """Handle node status changes"""
        
        self.logger.info(f"Node {node_id} status changed to {status.value}")
        
        # Trigger auto-scaling if needed
        if status == NodeStatus.ERROR:
            await self.auto_scaler.handle_node_failure(node_id)
        elif status == NodeStatus.OVERLOADED:
            await self.auto_scaler.handle_node_overload(node_id)
    
    async def _handle_node_metrics_update(self, node_id: str, metrics: ReasoningNodeMetrics):
        """Handle node metrics updates"""
        
        # Update performance tracking
        await self.performance_tracker.update_node_metrics(node_id, metrics)
        
        # Check if scaling is needed
        await self.auto_scaler.evaluate_scaling_needs()
    
    def _get_default_config(self, node_type: str) -> Dict[str, Any]:
        """Get default configuration for node type"""
        
        default_configs = {
            "react": {
                "max_iterations": 10,
                "enable_retry_on_error": True,
                "available_tools": ["web_search", "calculator"]
            },
            "mcts": {
                "simulation_count": 10000,
                "gpu_device": "cuda:0",
                "enable_gpu_acceleration": True
            },
            "dspy": {
                "enable_optimization": True,
                "optimization_frequency": 100
            }
        }
        
        return default_configs.get(node_type, {})
```

## 3. Auto-Scaling and Load Management

```python
class AutoScaler:
    """Automatic scaling of reasoning network based on demand"""
    
    def __init__(self, orchestrator: NeuralReasoningOrchestrator):
        self.orchestrator = orchestrator
        self.scaling_policies = {}
        self.scaling_history = []
        self.enabled = True
        
        # Scaling parameters
        self.scale_up_threshold = 0.8  # Scale up when load > 80%
        self.scale_down_threshold = 0.3  # Scale down when load < 30%
        self.min_nodes_per_type = 1
        self.max_nodes_per_type = 10
        
        # Timing
        self.last_scaling_action = 0
        self.scaling_cooldown = 300  # 5 minutes between scaling actions
    
    async def evaluate_scaling_needs(self):
        """Evaluate if network needs scaling"""
        
        if not self.enabled:
            return
        
        current_time = time.time()
        if current_time - self.last_scaling_action < self.scaling_cooldown:
            return  # Still in cooldown period
        
        network_status = await self.orchestrator.get_network_status()
        type_summary = network_status["type_summary"]
        
        scaling_actions = {}
        
        for node_type, summary in type_summary.items():
            current_nodes = summary["total_nodes"]
            active_nodes = summary["active_nodes"]
            avg_load = summary["average_load_factor"]
            
            # Determine scaling action
            if avg_load > self.scale_up_threshold and current_nodes < self.max_nodes_per_type:
                # Scale up
                target_nodes = min(current_nodes + 1, self.max_nodes_per_type)
                scaling_actions[node_type] = target_nodes
                
            elif avg_load < self.scale_down_threshold and current_nodes > self.min_nodes_per_type:
                # Scale down
                target_nodes = max(current_nodes - 1, self.min_nodes_per_type)
                scaling_actions[node_type] = target_nodes
        
        # Execute scaling if needed
        if scaling_actions:
            await self.orchestrator.scale_network(scaling_actions)
            self.last_scaling_action = current_time
            
            # Record scaling action
            self.scaling_history.append({
                "timestamp": current_time,
                "actions": scaling_actions,
                "reason": "auto_scaling_evaluation"
            })

class LoadBalancer:
    """Load balancer for reasoning network"""
    
    def __init__(self):
        self.nodes: Dict[str, ReasoningNode] = {}
        self.routing_strategy = "least_loaded"  # "round_robin", "weighted", "least_loaded"
        self.round_robin_index = 0
    
    async def register_node(self, node: ReasoningNode):
        """Register a node with the load balancer"""
        self.nodes[node.node_id] = node
    
    async def unregister_node(self, node: ReasoningNode):
        """Unregister a node from the load balancer"""
        if node.node_id in self.nodes:
            del self.nodes[node.node_id]
    
    async def select_node(self, available_node_ids: List[str]) -> Optional[str]:
        """Select the best node for a request"""
        
        available_nodes = {
            node_id: node for node_id, node in self.nodes.items()
            if node_id in available_node_ids and node._can_accept_request()
        }
        
        if not available_nodes:
            return None
        
        if self.routing_strategy == "least_loaded":
            # Select node with lowest load factor
            best_node_id = min(
                available_nodes.keys(),
                key=lambda nid: available_nodes[nid].metrics.calculate_load_factor()
            )
            return best_node_id
            
        elif self.routing_strategy == "round_robin":
            # Simple round-robin selection
            available_ids = list(available_nodes.keys())
            selected_id = available_ids[self.round_robin_index % len(available_ids)]
            self.round_robin_index += 1
            return selected_id
            
        else:
            # Default to first available
            return list(available_nodes.keys())[0]
    
    async def process_request(
        self,
        request_id: str,
        query: str,
        context: Dict[str, Any],
        selected_nodes: List[str],
        fallback_enabled: bool = True
    ) -> Dict[str, Any]:
        """Process request with load balancing and fallback"""
        
        for node_id in selected_nodes:
            try:
                if node_id in self.nodes:
                    node = self.nodes[node_id]
                    
                    if node._can_accept_request():
                        result = await node.process_request(request_id, query, context)
                        
                        if "error" not in result:
                            return result
                        elif not fallback_enabled:
                            return result
                
            except Exception as e:
                if not fallback_enabled:
                    return {"error": str(e), "node_id": node_id, "request_id": request_id}
                continue
        
        # All nodes failed
        return {
            "error": "All selected nodes failed to process request",
            "request_id": request_id,
            "attempted_nodes": selected_nodes
        }
```

This neural orchestration system provides complete control over dynamically adding, removing, and managing reasoning processes within your neural network architecture, with automatic scaling, load balancing, and performance monitoring.