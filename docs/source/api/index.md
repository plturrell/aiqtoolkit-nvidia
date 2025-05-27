# API Reference

Welcome to the AIQToolkit API Reference. This section provides detailed documentation for all public APIs, interfaces, and components.

## Core APIs

### [Builder API](builder.md)
- `WorkflowBuilder` - Build workflows from configuration
- `FunctionBuilder` - Construct function components
- `LLMBuilder` - Build LLM providers
- `DistributedWorkflowBuilder` - Distributed workflow construction

### [Agent API](agent.md)
- `BaseAgent` - Base agent interface
- `ReActAgent` - ReAct implementation
- `ReasoningAgent` - Advanced reasoning agent
- `ReWOOAgent` - ReWOO implementation
- `ToolCallingAgent` - Tool-calling agent

### [Memory API](memory.md)
- `MemoryInterface` - Base memory interface
- `ConversationMemory` - Conversation history management
- `VectorMemory` - Vector-based semantic memory
- `PersistentMemory` - File-based persistence
- `ResearchContextMemory` - Research-specific memory

### [Retriever API](retriever.md)
- `RetrieverInterface` - Base retriever interface
- `MilvusRetriever` - Milvus vector database
- `NEMORetriever` - NVIDIA NEMO retriever
- `NeuralSymbolicRetriever` - Hybrid retrieval
- `DSPyRetriever` - DSPy-based retriever

## Component APIs

### [LLM Providers](llm.md)
- `OpenAILLM` - OpenAI integration
- `NIMLLM` - NVIDIA NIM integration
- `LLMInterface` - Base LLM interface
- Configuration and usage

### [Embedder API](embedder.md)
- `EmbedderInterface` - Base embedder interface
- `OpenAIEmbedder` - OpenAI embeddings
- `NIMEmbedder` - NIM embeddings
- `LangChainEmbedder` - LangChain integration

### [Evaluator API](evaluator.md)
- `EvaluatorInterface` - Base evaluator interface
- `RAGEvaluator` - RAG evaluation
- `TrajectoryEvaluator` - Agent trajectory evaluation
- `SWEBenchEvaluator` - SWE-bench evaluation

## Advanced APIs

### [Verification System](verification.md)
- `VerificationSystem` - Real-time verification
- Confidence scoring methods
- W3C PROV compliance
- Source attribution

### [Nash-Ethereum Consensus](consensus.md)
- `NashEthereumConsensus` - Consensus mechanism
- Game theory integration
- Smart contract interface
- Multi-agent coordination

### [Digital Human](digital-human.md)
- `DigitalHumanOrchestrator` - Main orchestrator
- `AvatarController` - Avatar management
- `ConversationEngine` - Conversation processing
- `EmotionMapper` - Emotion processing

### [Research Framework](research.md)
- `ResearchTaskExecutor` - Task execution
- `NeuralSymbolicRetriever` - Advanced retrieval
- `SelfCorrectingSystem` - Self-correction
- GPU optimization

## Infrastructure APIs

### [GPU Acceleration](gpu.md)
- `GPUManager` - GPU resource management
- `TensorCoreOptimizer` - Tensor core optimization
- CUDA kernel integration
- Memory management

### [Distributed Computing](distributed.md)
- `NodeManager` - Node management
- `TaskScheduler` - Task distribution
- `WorkerNode` - Worker implementation
- gRPC communication

### [Profiler API](profiler.md)
- `ProfileRunner` - Profiling execution
- `CallbackHandler` - Event callbacks
- Performance metrics
- Optimization suggestions

## Tool APIs

### [Function Tools](tools.md)
- `RetrieverTool` - Document retrieval
- `CodeExecutionTool` - Code sandbox
- `MemoryTool` - Memory operations
- `MCPTool` - MCP integration

### [GitHub Tools](github-tools.md)
- `CreateGitHubCommit` - Git operations
- `CreateGitHubPR` - Pull requests
- `GitHubIssueTool` - Issue management
- Authentication and configuration

## Data Models

### [Core Models](data-models.md)
- `Component` - Base component model
- `ComponentRef` - Component references
- `Function` - Function model
- `Workflow` - Workflow model

### [Configuration Models](config-models.md)
- `WorkflowConfig` - Workflow configuration
- `LLMConfig` - LLM configuration
- `RetrieverConfig` - Retriever configuration
- `AgentConfig` - Agent configuration

## Utilities

### [Type System](type-utils.md)
- Type converters
- Type validators
- Custom type definitions
- Type annotations

### [Error Handling](errors.md)
- Exception hierarchy
- Error codes
- Error handlers
- Recovery strategies

### [Observability](observability.md)
- OpenTelemetry integration
- Metrics collection
- Distributed tracing
- Logging configuration

## Best Practices

### [Development Guidelines](guidelines.md)
- Coding standards
- API design principles
- Testing requirements
- Documentation standards

### [Performance Tips](performance.md)
- Optimization techniques
- Resource management
- Caching strategies
- Profiling methods

### [Security](security.md)
- Authentication methods
- Authorization patterns
- Data encryption
- Secret management

## Examples

Each API section includes practical examples demonstrating common use cases. For more comprehensive examples, see the [Examples](../examples) directory.

## Version Compatibility

This documentation covers AIQToolkit version 0.2.0. For version-specific information:

- [v0.2.0](https://docs.aiq.nvidia.com/v0.2.0) - Current version
- [v0.1.0](https://docs.aiq.nvidia.com/v0.1.0) - Previous version
- [Migration Guide](migration.md) - Version migration

## Getting Help

- [Troubleshooting Guide](../troubleshooting.md)
- [Support Channels](../support.md)
- [GitHub Issues](https://github.com/NVIDIA/AIQToolkit/issues)
- [Community Forum](https://forums.nvidia.com/aiq)

## Contributing

To contribute to the API documentation:

1. Read the [Contributing Guide](../resources/contributing.md)
2. Follow the [Documentation Standards](guidelines.md#documentation)
3. Submit a pull request

## License

AIQToolkit is licensed under the Apache License 2.0. See [LICENSE](../../../LICENSE.md) for details.