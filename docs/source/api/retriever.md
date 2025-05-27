# Retriever API Reference

## Overview

The Retriever module provides intelligent document retrieval capabilities for AIQToolkit. It supports multiple backends including vector databases, traditional search engines, and neural-symbolic approaches.

## Core Interface

### RetrieverInterface

Base interface for all retriever implementations.

```python
from aiq.retriever import RetrieverInterface
from typing import List, Dict, Any, Optional

class RetrieverInterface:
    async def retrieve(
        self,
        query: str,
        top_k: int = 5,
        filters: Optional[Dict[str, Any]] = None
    ) -> List[Document]:
        """Retrieve documents matching query
        
        Args:
            query: Search query
            top_k: Number of results to return
            filters: Optional metadata filters
            
        Returns:
            List of relevant documents
        """
        
    async def index(
        self,
        documents: List[Document],
        batch_size: int = 100
    ) -> IndexResult:
        """Index documents for retrieval
        
        Args:
            documents: Documents to index
            batch_size: Batch size for indexing
            
        Returns:
            Indexing result with statistics
        """
```

## Retriever Implementations

### MilvusRetriever

Vector database retriever using Milvus.

```python
from aiq.retriever.milvus import MilvusRetriever

class MilvusRetriever(RetrieverInterface):
    def __init__(
        self,
        collection_name: str,
        embedding_model: str = "text-embedding-ada-002",
        host: str = "localhost",
        port: int = 19530,
        metric_type: str = "IP",
        index_type: str = "IVF_FLAT",
        nprobe: int = 16
    ):
        """Initialize Milvus retriever
        
        Args:
            collection_name: Milvus collection name
            embedding_model: Model for embeddings
            host: Milvus host
            port: Milvus port
            metric_type: Distance metric (IP, L2)
            index_type: Index type
            nprobe: Number of clusters to search
        """
```

#### Methods

```python
async def create_collection(
    self,
    dimension: int,
    schema: Optional[CollectionSchema] = None
) -> None:
    """Create Milvus collection"""

async def add_documents(
    self,
    documents: List[Document],
    metadata: Optional[List[Dict[str, Any]]] = None
) -> List[str]:
    """Add documents to collection"""

async def search(
    self,
    query_vector: List[float],
    top_k: int = 5,
    expr: Optional[str] = None
) -> List[SearchResult]:
    """Vector similarity search"""

async def hybrid_search(
    self,
    query: str,
    top_k: int = 5,
    alpha: float = 0.5
) -> List[Document]:
    """Hybrid text + vector search"""
```

#### Example

```python
from aiq.retriever.milvus import MilvusRetriever

# Initialize retriever
retriever = MilvusRetriever(
    collection_name="knowledge_base",
    embedding_model="text-embedding-ada-002",
    metric_type="IP"
)

# Create collection
await retriever.create_collection(dimension=1536)

# Index documents
documents = [
    Document(content="AI is transforming industries", metadata={"source": "article1"}),
    Document(content="Machine learning enables prediction", metadata={"source": "article2"})
]
await retriever.index(documents)

# Search
results = await retriever.retrieve(
    query="How is AI changing business?",
    top_k=5,
    filters={"source": "article1"}
)
```

### NEMORetriever

NVIDIA NEMO-based retriever with advanced NLP.

```python
from aiq.retriever.nemo_retriever import NEMORetriever

class NEMORetriever(RetrieverInterface):
    def __init__(
        self,
        model_name: str = "nemo_retriever_base",
        device: str = "cuda",
        max_length: int = 512,
        use_cache: bool = True
    ):
        """Initialize NEMO retriever
        
        Args:
            model_name: NEMO model name
            device: Computing device
            max_length: Maximum sequence length
            use_cache: Enable result caching
        """
```

#### Advanced Features

```python
async def retrieve_with_reranking(
    self,
    query: str,
    top_k: int = 5,
    rerank_top_k: int = 20
) -> List[Document]:
    """Retrieve with neural reranking"""

async def multi_query_retrieve(
    self,
    queries: List[str],
    top_k: int = 5,
    aggregate: str = "max"
) -> List[Document]:
    """Multi-query fusion retrieval"""

async def contextual_retrieve(
    self,
    query: str,
    context: List[str],
    top_k: int = 5
) -> List[Document]:
    """Context-aware retrieval"""
```

### NeuralSymbolicRetriever

Combines neural and symbolic approaches.

```python
from aiq.retriever.neural_symbolic import NeuralSymbolicRetriever

class NeuralSymbolicRetriever(RetrieverInterface):
    def __init__(
        self,
        neural_model: str = "sentence-transformers/all-mpnet-base-v2",
        symbolic_engine: str = "elasticsearch",
        knowledge_graph: Optional[KnowledgeGraph] = None,
        fusion_method: str = "weighted"
    ):
        """Initialize neural-symbolic retriever
        
        Args:
            neural_model: Neural embedding model
            symbolic_engine: Symbolic search engine
            knowledge_graph: Optional KG integration
            fusion_method: Result fusion method
        """
```

#### Methods

```python
async def symbolic_search(
    self,
    query: str,
    filters: Dict[str, Any]
) -> List[Document]:
    """Pure symbolic search"""

async def neural_search(
    self,
    query: str,
    top_k: int
) -> List[Document]:
    """Pure neural search"""

async def hybrid_search(
    self,
    query: str,
    top_k: int = 5,
    neural_weight: float = 0.6
) -> List[Document]:
    """Hybrid neural-symbolic search"""

async def graph_enhanced_search(
    self,
    query: str,
    hop_distance: int = 2
) -> List[Document]:
    """Knowledge graph enhanced search"""
```

#### Example

```python
from aiq.retriever.neural_symbolic import NeuralSymbolicRetriever

# Initialize with knowledge graph
retriever = NeuralSymbolicRetriever(
    neural_model="all-mpnet-base-v2",
    symbolic_engine="elasticsearch",
    knowledge_graph=kg,
    fusion_method="reciprocal_rank"
)

# Hybrid search
results = await retriever.hybrid_search(
    query="quantum computing applications",
    neural_weight=0.7
)

# Graph-enhanced search
graph_results = await retriever.graph_enhanced_search(
    query="quantum computing",
    hop_distance=2
)
```

### DSPyRetriever

Retriever using DSPy framework for optimization.

```python
from aiq.retriever.neural_symbolic import DSPyRetriever
import dspy

class DSPyRetriever(RetrieverInterface):
    def __init__(
        self,
        lm: dspy.LM,
        rm: dspy.RM,
        optimize: bool = True
    ):
        """Initialize DSPy retriever
        
        Args:
            lm: Language model
            rm: Retrieval model
            optimize: Enable optimization
        """
```

#### Advanced Methods

```python
async def optimized_retrieve(
    self,
    query: str,
    examples: List[Example],
    metric: Callable
) -> List[Document]:
    """Retrieve with DSPy optimization"""

async def chain_of_thought_retrieve(
    self,
    query: str,
    reasoning_steps: int = 3
) -> List[Document]:
    """Retrieve with reasoning"""

async def multi_hop_retrieve(
    self,
    query: str,
    max_hops: int = 3
) -> List[Document]:
    """Multi-hop reasoning retrieval"""
```

## Models

### Document

Base document model.

```python
from aiq.retriever.models import Document

class Document:
    id: str
    content: str
    metadata: Dict[str, Any]
    embedding: Optional[List[float]] = None
    score: Optional[float] = None
    
    def to_dict(self) -> Dict[str, Any]:
        """Convert to dictionary"""
    
    def chunk(self, chunk_size: int = 512) -> List['Document']:
        """Split into chunks"""
```

### SearchResult

Search result model.

```python
from aiq.retriever.models import SearchResult

class SearchResult:
    document: Document
    score: float
    rank: int
    explanation: Optional[str] = None
    
    def format(self) -> str:
        """Format for display"""
```

## Configuration

### YAML Configuration

```yaml
retriever:
  type: milvus
  
  milvus:
    collection: knowledge_base
    host: localhost
    port: 19530
    embedding_model: text-embedding-ada-002
    metric_type: IP
    index_type: IVF_FLAT
    nprobe: 16
    
  preprocessing:
    chunk_size: 512
    chunk_overlap: 50
    
  reranking:
    enabled: true
    model: cross-encoder/ms-marco-MiniLM-L-12-v2
    top_k: 20
```

### Python Configuration

```python
from aiq.retriever import RetrieverConfig

config = RetrieverConfig(
    type="milvus",
    milvus=MilvusConfig(
        collection="knowledge_base",
        embedding_model="text-embedding-ada-002",
        metric_type="IP"
    ),
    preprocessing=PreprocessingConfig(
        chunk_size=512,
        chunk_overlap=50
    ),
    reranking=RerankingConfig(
        enabled=True,
        model="cross-encoder/ms-marco-MiniLM-L-12-v2"
    )
)

retriever = build_retriever(config)
```

## Advanced Features

### Document Processing

```python
from aiq.retriever.processing import DocumentProcessor

processor = DocumentProcessor(
    chunk_size=512,
    chunk_overlap=50,
    tokenizer="bert-base-uncased"
)

# Process documents
chunks = processor.process_documents(documents)

# Extract metadata
metadata = processor.extract_metadata(documents)

# Clean text
cleaned = processor.clean_text(documents)
```

### Query Expansion

```python
from aiq.retriever.query import QueryExpander

expander = QueryExpander(
    method="synonym",
    model="word2vec"
)

# Expand query
expanded_queries = expander.expand(
    query="machine learning algorithms",
    num_expansions=5
)

# Multi-query retrieval
results = await retriever.multi_query_retrieve(
    queries=expanded_queries,
    aggregate="reciprocal_rank"
)
```

### Reranking

```python
from aiq.retriever.reranking import Reranker

reranker = Reranker(
    model="cross-encoder/ms-marco-MiniLM-L-12-v2",
    device="cuda"
)

# Initial retrieval
candidates = await retriever.retrieve(query, top_k=50)

# Rerank results
reranked = reranker.rerank(
    query=query,
    documents=candidates,
    top_k=10
)
```

### Indexing Pipeline

```python
from aiq.retriever.pipeline import IndexingPipeline

pipeline = IndexingPipeline(
    stages=[
        DocumentLoader(),
        TextCleaner(),
        ChunkSplitter(chunk_size=512),
        EmbeddingGenerator(model="text-embedding-ada-002"),
        MetadataExtractor()
    ]
)

# Process and index documents
indexed_docs = await pipeline.process(
    documents=raw_documents,
    retriever=retriever
)
```

## Performance Optimization

### Batch Processing

```python
# Batch indexing
await retriever.batch_index(
    documents=documents,
    batch_size=1000,
    num_workers=4
)

# Batch retrieval
queries = ["query1", "query2", "query3"]
results = await retriever.batch_retrieve(
    queries=queries,
    top_k=5
)
```

### Caching

```python
from aiq.retriever.cache import CachedRetriever

cached_retriever = CachedRetriever(
    retriever=milvus_retriever,
    cache_size=10000,
    ttl=3600
)

# Cached retrieval
results = await cached_retriever.retrieve(query)
```

### GPU Acceleration

```python
from aiq.retriever.gpu import GPUAcceleratedRetriever

gpu_retriever = GPUAcceleratedRetriever(
    base_retriever=retriever,
    device="cuda:0",
    batch_size=256
)

# GPU-accelerated search
results = await gpu_retriever.retrieve(query)
```

## Error Handling

```python
from aiq.retriever.exceptions import (
    RetrieverError,
    IndexingError,
    SearchError,
    ConnectionError
)

try:
    results = await retriever.retrieve(query)
except ConnectionError:
    print("Failed to connect to retriever backend")
except SearchError as e:
    print(f"Search failed: {e}")
except RetrieverError as e:
    print(f"Retriever error: {e}")
```

## Tools Integration

```python
from aiq.tool import RetrieverTool

# Create retriever tool
tool = RetrieverTool(
    retriever=milvus_retriever,
    name="knowledge_search",
    description="Search knowledge base"
)

# Use in agent
agent = ReActAgent(
    tools=[tool],
    llm=llm
)

response = await agent.run("Find information about quantum computing")
```

## Monitoring

```python
from aiq.retriever.monitoring import RetrieverMonitor

monitor = RetrieverMonitor(retriever)

# Get metrics
metrics = monitor.get_metrics()
print(f"Query latency: {metrics['avg_latency_ms']}ms")
print(f"Success rate: {metrics['success_rate']}%")

# Monitor performance
monitor.start_monitoring()
```

## Best Practices

1. **Choose appropriate backend**
   ```python
   # For semantic search
   retriever = MilvusRetriever()
   
   # For hybrid search
   retriever = NeuralSymbolicRetriever()
   
   # For optimization
   retriever = DSPyRetriever()
   ```

2. **Optimize indexing**
   ```python
   # Use appropriate chunk sizes
   processor = DocumentProcessor(
       chunk_size=512,  # Optimal for most models
       chunk_overlap=50
   )
   ```

3. **Enable caching**
   ```python
   retriever = CachedRetriever(
       base_retriever,
       cache_size=10000
   )
   ```

4. **Monitor performance**
   ```python
   monitor = RetrieverMonitor(retriever)
   monitor.alert_on_high_latency(threshold_ms=100)
   ```

## See Also

- [Retriever Configuration](../store-and-retrieve/retrievers.md)
- [Memory API](memory.md)
- [Vector Databases](../tutorials/vector-databases.md)