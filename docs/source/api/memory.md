# Memory API Reference

## Overview

The Memory module provides persistent storage and retrieval of conversation history, context, and knowledge for AIQToolkit agents. It supports multiple backends and offers both short-term and long-term memory capabilities.

## Core Interfaces

### MemoryInterface

Base interface for all memory implementations.

```python
from aiq.memory import MemoryInterface
from typing import List, Dict, Any, Optional

class MemoryInterface:
    async def store(
        self,
        key: str,
        value: Any,
        metadata: Optional[Dict[str, Any]] = None
    ) -> None:
        """Store item in memory
        
        Args:
            key: Unique identifier
            value: Value to store
            metadata: Optional metadata
        """
        
    async def retrieve(
        self,
        key: str
    ) -> Optional[Any]:
        """Retrieve item from memory
        
        Args:
            key: Item identifier
            
        Returns:
            Stored value or None
        """
        
    async def search(
        self,
        query: str,
        top_k: int = 5,
        filters: Optional[Dict[str, Any]] = None
    ) -> List[MemoryItem]:
        """Search memory by query
        
        Args:
            query: Search query
            top_k: Number of results
            filters: Optional filters
            
        Returns:
            List of matching items
        """
        
    async def delete(
        self,
        key: str
    ) -> bool:
        """Delete item from memory
        
        Args:
            key: Item to delete
            
        Returns:
            True if deleted
        """
```

## Memory Implementations

### ConversationMemory

Manages conversation history and context.

```python
from aiq.memory import ConversationMemory

class ConversationMemory(MemoryInterface):
    def __init__(
        self,
        max_history: int = 100,
        summarization_threshold: int = 50
    ):
        """Initialize conversation memory
        
        Args:
            max_history: Maximum messages to store
            summarization_threshold: When to summarize
        """
```

#### Methods

```python
async def add_message(
    self,
    role: str,
    content: str,
    metadata: Optional[Dict[str, Any]] = None
) -> None:
    """Add message to conversation history"""

async def get_history(
    self,
    limit: Optional[int] = None
) -> List[Message]:
    """Get conversation history"""

async def summarize(self) -> str:
    """Generate conversation summary"""

async def clear_history(self) -> None:
    """Clear conversation history"""
```

#### Example

```python
from aiq.memory import ConversationMemory

# Initialize memory
memory = ConversationMemory(max_history=50)

# Add messages
await memory.add_message("user", "What is AI?")
await memory.add_message("assistant", "AI is artificial intelligence...")

# Get history
history = await memory.get_history(limit=10)

# Get summary
summary = await memory.summarize()
```

### VectorMemory

Vector-based semantic memory using embeddings.

```python
from aiq.memory import VectorMemory

class VectorMemory(MemoryInterface):
    def __init__(
        self,
        embedding_model: str = "text-embedding-ada-002",
        index_type: str = "cosine",
        dimension: int = 768
    ):
        """Initialize vector memory
        
        Args:
            embedding_model: Model for embeddings
            index_type: Vector index type
            dimension: Embedding dimension
        """
```

#### Methods

```python
async def store_document(
    self,
    document: str,
    metadata: Optional[Dict[str, Any]] = None
) -> str:
    """Store document with embeddings"""

async def similarity_search(
    self,
    query: str,
    top_k: int = 5,
    threshold: float = 0.7
) -> List[MemoryItem]:
    """Search by semantic similarity"""

async def update_embeddings(
    self,
    key: str,
    new_content: str
) -> None:
    """Update document embeddings"""
```

#### Example

```python
from aiq.memory import VectorMemory

# Initialize vector memory
memory = VectorMemory(
    embedding_model="text-embedding-ada-002",
    dimension=1536
)

# Store documents
doc_id = await memory.store_document(
    "AIQ Toolkit is a powerful framework for building AI agents.",
    metadata={"category": "documentation"}
)

# Search by similarity
results = await memory.similarity_search(
    "What is AIQ Toolkit?",
    top_k=3
)

for result in results:
    print(f"Score: {result.score}, Content: {result.content}")
```

### PersistentMemory

File-based persistent memory storage.

```python
from aiq.memory import PersistentMemory

class PersistentMemory(MemoryInterface):
    def __init__(
        self,
        storage_path: str,
        format: str = "json",
        compression: bool = False
    ):
        """Initialize persistent memory
        
        Args:
            storage_path: Path to storage directory
            format: Storage format (json, pickle, parquet)
            compression: Enable compression
        """
```

#### Example

```python
from aiq.memory import PersistentMemory

# Initialize persistent memory
memory = PersistentMemory(
    storage_path="/data/memory",
    format="parquet",
    compression=True
)

# Store data
await memory.store(
    "session_123",
    {"user": "john", "context": "research"},
    metadata={"timestamp": time.time()}
)

# Retrieve data
session = await memory.retrieve("session_123")
```

## Research Memory

Advanced memory system for research tasks.

```python
from aiq.memory.research import ResearchContextMemory

class ResearchContextMemory:
    def __init__(
        self,
        vector_store: Optional[VectorMemory] = None,
        graph_store: Optional[GraphMemory] = None
    ):
        """Initialize research memory
        
        Args:
            vector_store: Vector memory backend
            graph_store: Graph memory backend
        """
```

### Methods

```python
async def store_research_context(
    self,
    context: ResearchContext
) -> str:
    """Store research context with relationships"""

async def retrieve_related_contexts(
    self,
    query: str,
    max_depth: int = 2
) -> List[ResearchContext]:
    """Retrieve related research contexts"""

async def build_knowledge_graph(
    self,
    contexts: List[ResearchContext]
) -> KnowledgeGraph:
    """Build knowledge graph from contexts"""
```

### Example

```python
from aiq.memory.research import ResearchContextMemory, ResearchContext

# Initialize research memory
memory = ResearchContextMemory()

# Create research context
context = ResearchContext(
    topic="Machine Learning",
    findings=["Neural networks can learn patterns", "Deep learning requires data"],
    sources=["paper1.pdf", "website.com"],
    relationships=["AI", "Data Science"]
)

# Store context
context_id = await memory.store_research_context(context)

# Retrieve related contexts
related = await memory.retrieve_related_contexts(
    "deep learning applications",
    max_depth=2
)
```

## Memory Models

### MemoryItem

Base model for memory items.

```python
from aiq.memory.models import MemoryItem
from datetime import datetime

class MemoryItem:
    key: str
    content: Any
    metadata: Dict[str, Any]
    timestamp: datetime
    score: Optional[float] = None
    
    def to_dict(self) -> Dict[str, Any]:
        """Convert to dictionary"""
        
    @classmethod
    def from_dict(cls, data: Dict[str, Any]) -> 'MemoryItem':
        """Create from dictionary"""
```

### Message

Model for conversation messages.

```python
from aiq.memory.models import Message

class Message:
    role: str  # "user", "assistant", "system"
    content: str
    timestamp: datetime
    metadata: Optional[Dict[str, Any]] = None
    
    def to_prompt_format(self) -> str:
        """Convert to prompt format"""
```

## Memory Configuration

### YAML Configuration

```yaml
memory:
  type: hybrid
  
  conversation:
    max_history: 100
    summarization_threshold: 50
    
  vector:
    backend: milvus
    collection: aiq_memory
    embedding_model: text-embedding-ada-002
    dimension: 1536
    
  persistent:
    path: /data/aiq/memory
    format: parquet
    compression: gzip
    
  cache:
    enabled: true
    ttl: 3600
    max_size: 1000
```

### Python Configuration

```python
from aiq.memory import MemoryConfig

config = MemoryConfig(
    type="hybrid",
    conversation=ConversationConfig(
        max_history=100,
        summarization_threshold=50
    ),
    vector=VectorConfig(
        backend="milvus",
        collection="aiq_memory",
        embedding_model="text-embedding-ada-002"
    ),
    persistent=PersistentConfig(
        path="/data/aiq/memory",
        format="parquet"
    )
)

memory = build_memory(config)
```

## Memory Tools

### Memory Management Tools

```python
from aiq.tool.memory_tools import (
    AddMemoryTool,
    GetMemoryTool,
    DeleteMemoryTool,
    SearchMemoryTool
)

# Add memory tool
add_tool = AddMemoryTool()
await add_tool.execute(
    key="fact_123",
    content="Paris is the capital of France",
    metadata={"category": "geography"}
)

# Search memory tool
search_tool = SearchMemoryTool()
results = await search_tool.execute(
    query="capital cities",
    top_k=5
)
```

## Advanced Features

### Memory Compression

```python
from aiq.memory import CompressedMemory

class CompressedMemory(MemoryInterface):
    def __init__(
        self,
        backend: MemoryInterface,
        compression_ratio: float = 0.5
    ):
        """Memory with automatic compression"""
        
    async def compress_old_memories(
        self,
        age_threshold_days: int = 30
    ) -> int:
        """Compress memories older than threshold"""
```

### Memory Hierarchies

```python
from aiq.memory import HierarchicalMemory

memory = HierarchicalMemory(
    levels=[
        {"name": "cache", "ttl": 300, "max_size": 100},
        {"name": "hot", "ttl": 3600, "max_size": 1000},
        {"name": "cold", "ttl": None, "max_size": None}
    ]
)

# Items automatically move between levels
await memory.store("key", "value")
value = await memory.retrieve("key")  # Promotes to cache
```

### Memory Synchronization

```python
from aiq.memory import SynchronizedMemory

# Synchronize across multiple backends
memory = SynchronizedMemory(
    backends=[
        LocalMemory(),
        RedisMemory(),
        S3Memory()
    ],
    sync_strategy="eventual"
)
```

## Performance Optimization

### Batch Operations

```python
# Batch store
await memory.batch_store([
    {"key": "key1", "value": "value1"},
    {"key": "key2", "value": "value2"},
    {"key": "key3", "value": "value3"}
])

# Batch retrieve
values = await memory.batch_retrieve(["key1", "key2", "key3"])
```

### Caching Strategies

```python
from aiq.memory import CachedMemory

memory = CachedMemory(
    backend=VectorMemory(),
    cache_size=1000,
    ttl=3600,
    eviction_policy="lru"
)
```

## Error Handling

```python
from aiq.memory.exceptions import (
    MemoryError,
    MemoryNotFoundError,
    MemoryQuotaExceededError
)

try:
    value = await memory.retrieve("key")
except MemoryNotFoundError:
    print("Key not found")
except MemoryQuotaExceededError:
    print("Memory quota exceeded")
except MemoryError as e:
    print(f"Memory error: {e}")
```

## Best Practices

1. **Choose appropriate backend**
   ```python
   # For conversation history
   memory = ConversationMemory()
   
   # for semantic search
   memory = VectorMemory()
   
   # For persistence
   memory = PersistentMemory()
   ```

2. **Set retention policies**
   ```python
   memory = ConversationMemory(
       max_history=100,
       retention_days=30
   )
   ```

3. **Use appropriate indexes**
   ```python
   vector_memory = VectorMemory(
       index_type="hnsw",  # For large-scale similarity search
       dimension=1536
   )
   ```

4. **Monitor memory usage**
   ```python
   stats = await memory.get_stats()
   print(f"Items: {stats['count']}")
   print(f"Size: {stats['size_mb']}MB")
   ```

## See Also

- [Memory Configuration](../store-and-retrieve/memory.md)
- [Retriever API](retriever.md)
- [Research Framework](../workflows/research/index.md)