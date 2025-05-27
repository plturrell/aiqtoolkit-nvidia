# Builder API Reference

## Overview

The Builder module provides the core infrastructure for constructing AIQToolkit workflows, managing components, and orchestrating execution. It includes builders for workflows, functions, LLMs, and other components.

## WorkflowBuilder

The `WorkflowBuilder` class is responsible for constructing and managing workflows from configuration files.

```python
from aiq.builder import WorkflowBuilder

class WorkflowBuilder:
    def __init__(self, config: Union[str, Path, Dict[str, Any]]):
        """Initialize workflow builder with configuration
        
        Args:
            config: Path to config file or config dictionary
        """
```

### Methods

#### build()

```python
def build(self) -> Workflow:
    """Build workflow from configuration
    
    Returns:
        Workflow: Constructed workflow instance
        
    Raises:
        ValueError: Invalid configuration
        RuntimeError: Build process failed
    """
```

#### validate()

```python
def validate(self) -> bool:
    """Validate workflow configuration
    
    Returns:
        bool: True if valid, False otherwise
    """
```

### Example

```python
from aiq.builder import WorkflowBuilder

# Build from config file
builder = WorkflowBuilder("config/workflow.yaml")
workflow = builder.build()

# Build from dictionary
config = {
    "name": "example_workflow",
    "version": "1.0.0",
    "components": {
        "llm": {"type": "openai", "model": "gpt-4"},
        "agent": {"type": "react", "max_steps": 5}
    }
}
builder = WorkflowBuilder(config)
workflow = builder.build()
```

## FunctionBuilder

The `FunctionBuilder` class constructs function components for workflows.

```python
from aiq.builder import FunctionBuilder

class FunctionBuilder:
    def __init__(self):
        """Initialize function builder"""
```

### Methods

#### build_function()

```python
def build_function(
    self,
    function_ref: Union[str, Dict[str, Any]],
    context: BuildContext
) -> Function:
    """Build function from reference
    
    Args:
        function_ref: Function reference or configuration
        context: Build context with dependencies
        
    Returns:
        Function: Built function instance
    """
```

#### register_function()

```python
def register_function(
    self,
    name: str,
    function: Callable,
    metadata: Optional[Dict[str, Any]] = None
) -> None:
    """Register a function for use in workflows
    
    Args:
        name: Function identifier
        function: Callable to register
        metadata: Optional function metadata
    """
```

### Example

```python
from aiq.builder import FunctionBuilder

builder = FunctionBuilder()

# Register custom function
def my_function(text: str) -> str:
    return text.upper()

builder.register_function(
    name="uppercase",
    function=my_function,
    metadata={"description": "Convert text to uppercase"}
)

# Build function from reference
function = builder.build_function(
    function_ref="uppercase",
    context=build_context
)
```

## LLMBuilder

The `LLMBuilder` class constructs LLM components.

```python
from aiq.builder import LLMBuilder

class LLMBuilder:
    def __init__(self):
        """Initialize LLM builder"""
```

### Methods

#### build_llm()

```python
def build_llm(
    self,
    llm_config: Dict[str, Any],
    context: BuildContext
) -> LLM:
    """Build LLM from configuration
    
    Args:
        llm_config: LLM configuration
        context: Build context
        
    Returns:
        LLM: Built LLM instance
    """
```

### Example

```python
from aiq.builder import LLMBuilder

builder = LLMBuilder()

# Build OpenAI LLM
llm = builder.build_llm(
    llm_config={
        "type": "openai",
        "model": "gpt-4",
        "temperature": 0.7,
        "max_tokens": 1000
    },
    context=build_context
)

# Build NIM LLM
nim_llm = builder.build_llm(
    llm_config={
        "type": "nim",
        "model": "llama3:70b",
        "endpoint": "http://localhost:8000"
    },
    context=build_context
)
```

## Component Management

### ComponentRef

Reference to a component in the workflow.

```python
from aiq.data_models import ComponentRef

class ComponentRef:
    name: str
    version: Optional[str] = None
    source: Optional[str] = None
    
    def resolve(self, context: BuildContext) -> Component:
        """Resolve reference to actual component"""
```

### BuildContext

Context for building components with dependency management.

```python
from aiq.builder import BuildContext

class BuildContext:
    def __init__(self):
        """Initialize build context"""
        
    def add_component(self, name: str, component: Component) -> None:
        """Add component to context"""
        
    def get_component(self, name: str) -> Component:
        """Get component from context"""
        
    def resolve_dependencies(self) -> None:
        """Resolve all component dependencies"""
```

## Distributed Workflow Builder

Builder for distributed workflows across multiple nodes.

```python
from aiq.builder import DistributedWorkflowBuilder

class DistributedWorkflowBuilder(WorkflowBuilder):
    def __init__(self, config: Dict[str, Any], nodes: List[str]):
        """Initialize distributed workflow builder
        
        Args:
            config: Workflow configuration
            nodes: List of node addresses
        """
```

### Methods

#### distribute_tasks()

```python
def distribute_tasks(self, tasks: List[Task]) -> Dict[str, List[Task]]:
    """Distribute tasks across nodes
    
    Args:
        tasks: Tasks to distribute
        
    Returns:
        Dict mapping node to assigned tasks
    """
```

#### build_distributed()

```python
def build_distributed(self) -> DistributedWorkflow:
    """Build distributed workflow
    
    Returns:
        DistributedWorkflow: Distributed workflow instance
    """
```

### Example

```python
from aiq.builder import DistributedWorkflowBuilder

# Create distributed workflow
builder = DistributedWorkflowBuilder(
    config={
        "name": "distributed_research",
        "tasks": [
            {"type": "research", "topic": "AI"},
            {"type": "research", "topic": "ML"},
            {"type": "analysis", "data": "results"}
        ]
    },
    nodes=["node1.cluster:5000", "node2.cluster:5000"]
)

workflow = builder.build_distributed()

# Execute across cluster
results = await workflow.execute()
```

## Validation

### ConfigValidator

Validates workflow configurations.

```python
from aiq.builder import ConfigValidator

class ConfigValidator:
    def __init__(self, schema: Optional[Dict[str, Any]] = None):
        """Initialize validator with optional schema"""
        
    def validate(self, config: Dict[str, Any]) -> ValidationResult:
        """Validate configuration
        
        Returns:
            ValidationResult: Result with errors if any
        """
```

### Example

```python
from aiq.builder import ConfigValidator

validator = ConfigValidator()

result = validator.validate(config)
if not result.is_valid:
    for error in result.errors:
        print(f"Error: {error}")
```

## Error Handling

### BuildError

Exception raised during build process.

```python
from aiq.builder import BuildError

class BuildError(Exception):
    def __init__(self, message: str, component: Optional[str] = None):
        """Initialize build error
        
        Args:
            message: Error message
            component: Component that failed to build
        """
```

### Example

```python
try:
    workflow = builder.build()
except BuildError as e:
    print(f"Build failed: {e}")
    if e.component:
        print(f"Failed component: {e.component}")
```

## Best Practices

1. **Configuration Validation**
   ```python
   # Always validate before building
   if builder.validate():
       workflow = builder.build()
   else:
       print("Invalid configuration")
   ```

2. **Error Handling**
   ```python
   # Handle build errors gracefully
   try:
       workflow = builder.build()
   except BuildError as e:
       logger.error(f"Build failed: {e}")
       # Fallback or retry logic
   ```

3. **Component Registration**
   ```python
   # Register all custom components before building
   function_builder.register_function("custom", my_function)
   workflow = workflow_builder.build()
   ```

4. **Resource Management**
   ```python
   # Clean up resources after building
   try:
       workflow = builder.build()
   finally:
       builder.cleanup()
   ```

## See Also

- [Workflow Configuration](../workflows/workflow-configuration.md)
- [Component Reference](component-ref.md)
- [Function Builder Guide](../extend/functions.md)