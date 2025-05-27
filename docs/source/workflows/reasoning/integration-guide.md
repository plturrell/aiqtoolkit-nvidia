# Reasoning Systems Integration Guide

## Overview

This guide demonstrates how to combine AIQToolkit's multiple reasoning systems to create powerful, hybrid intelligent applications. By integrating core reasoning (ReAct, ReWOO, Reasoning Agent, Tool Calling) with advanced systems (Neural-Symbolic, MCTS, Jena, DSPy), you can build sophisticated AI solutions that leverage the strengths of each approach.

## Integration Architecture

```
┌─────────────────────────────────────────────────────────────────────────────────────┐
│                        Unified Reasoning Architecture                               │
├─────────────────────────────────────────────────────────────────────────────────────┤
│                                                                                     │
│  ┌─────────────────────────────────────────────────────────────────────────────────┐ │
│  │                          Application Layer                                      │ │
│  │                                                                                 │ │
│  │  ┌─────────────────┐  ┌─────────────────┐  ┌─────────────────┐               │ │
│  │  │   Financial     │  │    Research     │  │   Knowledge     │               │ │
│  │  │   Analysis      │  │   Assistant     │  │   Management    │               │ │
│  │  └─────────────────┘  └─────────────────┘  └─────────────────┘               │ │
│  └─────────────────────────────────────────────────────────────────────────────────┘ │
│                                                                                     │
│  ┌─────────────────────────────────────────────────────────────────────────────────┐ │
│  │                      Reasoning Orchestrator                                    │ │
│  │                                                                                 │ │
│  │  • Route queries to appropriate reasoning systems                              │ │
│  │  • Combine outputs from multiple systems                                       │ │
│  │  • Manage reasoning chain transparency                                          │ │
│  │  • Optimize performance and cost                                               │ │
│  └─────────────────────────────────────────────────────────────────────────────────┘ │
│                                                                                     │
│  ┌─────────────────────────────────────────────────────────────────────────────────┐ │
│  │                        Reasoning Systems Layer                                 │ │
│  │                                                                                 │ │
│  │  ┌─────────────┐ ┌─────────────┐ ┌─────────────┐ ┌─────────────┐             │ │
│  │  │ Core        │ │ Neural-     │ │ MCTS        │ │ DSPy        │             │ │
│  │  │ Reasoning   │ │ Symbolic    │ │ Probabilistic│ │ Self-       │             │ │
│  │  │             │ │ Hybrid      │ │ Optimization │ │ Improving   │             │ │
│  │  │ • ReAct     │ │ • Knowledge │ │ • Financial │ │ • Prompt    │             │ │
│  │  │ • ReWOO     │ │   Graphs    │ │   Models    │ │   Optimization│             │ │
│  │  │ • Reasoning │ │ • Jena RDF  │ │ • Risk      │ │ • Auto-tuning │             │ │
│  │  │ • Tool Call │ │ • SPARQL    │ │   Assessment│ │ • Learning   │             │ │
│  │  └─────────────┘ └─────────────┘ └─────────────┘ └─────────────┘             │ │
│  └─────────────────────────────────────────────────────────────────────────────────┘ │
│                                                                                     │
│  ┌─────────────────────────────────────────────────────────────────────────────────┐ │
│  │                         Foundation Layer                                       │ │
│  │                                                                                 │ │
│  │  ┌─────────────┐ ┌─────────────┐ ┌─────────────┐ ┌─────────────┐             │ │
│  │  │ LLM/Models  │ │ Vector      │ │ Knowledge   │ │ GPU/CUDA    │             │ │
│  │  │             │ │ Stores      │ │ Graphs      │ │ Acceleration │             │ │
│  │  │ • OpenAI    │ │ • Embeddings │ │ • RDF/OWL   │ │ • Parallel  │             │ │
│  │  │ • NVIDIA    │ │ • Similarity │ │ • SPARQL    │ │   Processing │             │ │
│  │  │ • Local     │ │ • Search    │ │ • Ontology  │ │ • Optimization│             │ │
│  │  └─────────────┘ └─────────────┘ └─────────────┘ └─────────────┘             │ │
│  └─────────────────────────────────────────────────────────────────────────────────┘ │
└─────────────────────────────────────────────────────────────────────────────────────┘
```

## Core Integration Patterns

### 1. Reasoning Orchestrator

The Reasoning Orchestrator manages routing, combination, and optimization across all reasoning systems:

```python
from typing import Dict, List, Any, Optional, Union
from enum import Enum
import asyncio
import logging

from aiq.agent.react_agent import build_react_agent
from aiq.agent.rewoo_agent import build_rewoo_agent
from aiq.agent.reasoning_agent import build_reasoning_function
from aiq.agent.tool_calling_agent import build_tool_calling_agent
from aiq.retriever.neural_symbolic import NeuralSymbolicRetriever
from aiq.digital_human.financial import MCTSFinancialAnalyzer
from aiq.document_management.jena_integration import JenaIntegration
from aiq.digital_human.knowledge import DSPyFinancialProcessor

class ReasoningStrategy(Enum):
    """Available reasoning strategies"""
    INTERACTIVE = "interactive"  # ReAct for exploratory tasks
    PLANNED = "planned"          # ReWOO for structured workflows
    TRANSPARENT = "transparent"  # Reasoning Agent for explainability
    NATIVE = "native"           # Tool Calling for modern LLMs
    HYBRID = "hybrid"           # Neural-Symbolic for knowledge tasks
    PROBABILISTIC = "probabilistic"  # MCTS for optimization
    SEMANTIC = "semantic"       # Jena for knowledge graphs
    ADAPTIVE = "adaptive"       # DSPy for self-improvement

class ReasoningOrchestrator:
    """Central orchestrator for all reasoning systems"""
    
    def __init__(self, config: Dict[str, Any]):
        self.config = config
        self.reasoning_systems = {}
        self.performance_tracker = {}
        self.fallback_chain = []
        
        # Initialize logging
        self.logger = logging.getLogger(__name__)
        
        # Initialize systems based on configuration
        asyncio.run(self._initialize_systems())
    
    async def _initialize_systems(self):
        """Initialize all reasoning systems"""
        
        builder = self._get_builder()
        
        # Initialize core reasoning systems
        if self.config.get("enable_react", True):
            self.reasoning_systems["react"] = await build_react_agent(
                self.config["react_config"], builder
            )
        
        if self.config.get("enable_rewoo", True):
            self.reasoning_systems["rewoo"] = await build_rewoo_agent(
                self.config["rewoo_config"], builder
            )
        
        if self.config.get("enable_reasoning_agent", True):
            self.reasoning_systems["reasoning"] = await build_reasoning_function(
                self.config["reasoning_config"], builder
            )
        
        if self.config.get("enable_tool_calling", True):
            self.reasoning_systems["tool_calling"] = await build_tool_calling_agent(
                self.config["tool_calling_config"], builder
            )
        
        # Initialize advanced systems
        if self.config.get("enable_neural_symbolic", False):
            self.reasoning_systems["neural_symbolic"] = NeuralSymbolicRetriever(
                **self.config["neural_symbolic_config"]
            )
        
        if self.config.get("enable_mcts", False):
            self.reasoning_systems["mcts"] = MCTSFinancialAnalyzer(
                **self.config["mcts_config"]
            )
        
        if self.config.get("enable_jena", False):
            self.reasoning_systems["jena"] = JenaIntegration(
                **self.config["jena_config"]
            )
        
        if self.config.get("enable_dspy", False):
            self.reasoning_systems["dspy"] = DSPyFinancialProcessor(
                **self.config["dspy_config"]
            )
        
        # Set up fallback chain
        self.fallback_chain = self.config.get("fallback_chain", [
            "tool_calling", "react", "rewoo", "reasoning"
        ])
        
        self.logger.info(f"Initialized {len(self.reasoning_systems)} reasoning systems")
    
    async def reason(
        self,
        query: str,
        context: Dict[str, Any] = None,
        strategy: Union[ReasoningStrategy, str] = "auto",
        combine_systems: List[str] = None,
        **kwargs
    ) -> Dict[str, Any]:
        """Main reasoning interface"""
        
        if context is None:
            context = {}
        
        # Auto-select strategy if not specified
        if strategy == "auto":
            strategy = await self._select_optimal_strategy(query, context)
        elif isinstance(strategy, str):
            strategy = ReasoningStrategy(strategy)
        
        # Single system reasoning
        if combine_systems is None:
            return await self._single_system_reasoning(
                query, context, strategy, **kwargs
            )
        
        # Multi-system reasoning
        return await self._multi_system_reasoning(
            query, context, combine_systems, **kwargs
        )
    
    async def _select_optimal_strategy(
        self,
        query: str,
        context: Dict[str, Any]
    ) -> ReasoningStrategy:
        """Automatically select the best reasoning strategy"""
        
        # Analyze query characteristics
        query_analysis = await self._analyze_query(query, context)
        
        # Decision rules based on query characteristics
        if query_analysis["is_financial_optimization"]:
            return ReasoningStrategy.PROBABILISTIC  # Use MCTS
        
        elif query_analysis["requires_knowledge_graph"]:
            return ReasoningStrategy.SEMANTIC  # Use Jena
        
        elif query_analysis["is_research_intensive"]:
            return ReasoningStrategy.HYBRID  # Use Neural-Symbolic
        
        elif query_analysis["requires_transparency"]:
            return ReasoningStrategy.TRANSPARENT  # Use Reasoning Agent
        
        elif query_analysis["is_structured_workflow"]:
            return ReasoningStrategy.PLANNED  # Use ReWOO
        
        elif query_analysis["is_exploratory"]:
            return ReasoningStrategy.INTERACTIVE  # Use ReAct
        
        else:
            return ReasoningStrategy.NATIVE  # Use Tool Calling
    
    async def _single_system_reasoning(
        self,
        query: str,
        context: Dict[str, Any],
        strategy: ReasoningStrategy,
        **kwargs
    ) -> Dict[str, Any]:
        """Execute reasoning using a single system"""
        
        system_name = strategy.value
        
        if system_name not in self.reasoning_systems:
            # Fallback to available system
            for fallback in self.fallback_chain:
                if fallback in self.reasoning_systems:
                    system_name = fallback
                    break
            else:
                raise ValueError("No reasoning systems available")
        
        system = self.reasoning_systems[system_name]
        
        try:
            # Execute reasoning with performance tracking
            start_time = time.time()
            
            result = await self._execute_reasoning(
                system, system_name, query, context, **kwargs
            )
            
            execution_time = time.time() - start_time
            
            # Track performance
            self._track_performance(system_name, execution_time, result)
            
            # Add metadata
            result["reasoning_metadata"] = {
                "strategy": strategy.value,
                "system": system_name,
                "execution_time": execution_time,
                "query_analysis": await self._analyze_query(query, context)
            }
            
            return result
            
        except Exception as e:
            self.logger.error(f"Reasoning failed with {system_name}: {e}")
            
            # Try fallback systems
            for fallback in self.fallback_chain:
                if fallback != system_name and fallback in self.reasoning_systems:
                    try:
                        self.logger.info(f"Attempting fallback to {fallback}")
                        
                        fallback_system = self.reasoning_systems[fallback]
                        result = await self._execute_reasoning(
                            fallback_system, fallback, query, context, **kwargs
                        )
                        
                        result["reasoning_metadata"] = {
                            "strategy": "fallback",
                            "system": fallback,
                            "original_system": system_name,
                            "fallback_reason": str(e)
                        }
                        
                        return result
                        
                    except Exception as fallback_error:
                        self.logger.error(f"Fallback {fallback} also failed: {fallback_error}")
                        continue
            
            # All systems failed
            raise Exception(f"All reasoning systems failed. Original error: {e}")
    
    async def _multi_system_reasoning(
        self,
        query: str,
        context: Dict[str, Any],
        systems: List[str],
        combination_strategy: str = "weighted_ensemble",
        **kwargs
    ) -> Dict[str, Any]:
        """Execute reasoning using multiple systems and combine results"""
        
        # Execute reasoning with each system
        results = {}
        
        for system_name in systems:
            if system_name in self.reasoning_systems:
                try:
                    system = self.reasoning_systems[system_name]
                    result = await self._execute_reasoning(
                        system, system_name, query, context, **kwargs
                    )
                    results[system_name] = result
                    
                except Exception as e:
                    self.logger.warning(f"System {system_name} failed: {e}")
                    results[system_name] = {"error": str(e)}
        
        # Combine results based on strategy
        if combination_strategy == "weighted_ensemble":
            combined_result = await self._weighted_ensemble_combination(results)
        elif combination_strategy == "majority_vote":
            combined_result = await self._majority_vote_combination(results)
        elif combination_strategy == "confidence_ranking":
            combined_result = await self._confidence_ranking_combination(results)
        else:
            combined_result = await self._simple_combination(results)
        
        # Add combination metadata
        combined_result["reasoning_metadata"] = {
            "strategy": "multi_system",
            "systems": list(results.keys()),
            "combination_strategy": combination_strategy,
            "individual_results": {
                system: result.get("reasoning_metadata", {})
                for system, result in results.items()
                if "error" not in result
            }
        }
        
        return combined_result
```

### 2. System-Specific Execution

```python
    async def _execute_reasoning(
        self,
        system: Any,
        system_name: str,
        query: str,
        context: Dict[str, Any],
        **kwargs
    ) -> Dict[str, Any]:
        """Execute reasoning for specific system type"""
        
        if system_name in ["react", "rewoo", "reasoning", "tool_calling"]:
            # Core reasoning systems
            return await self._execute_core_reasoning(system, query, context, **kwargs)
        
        elif system_name == "neural_symbolic":
            # Neural-symbolic retrieval
            return await self._execute_neural_symbolic(system, query, context, **kwargs)
        
        elif system_name == "mcts":
            # Monte Carlo Tree Search
            return await self._execute_mcts(system, query, context, **kwargs)
        
        elif system_name == "jena":
            # Jena semantic reasoning
            return await self._execute_jena(system, query, context, **kwargs)
        
        elif system_name == "dspy":
            # DSPy self-improving
            return await self._execute_dspy(system, query, context, **kwargs)
        
        else:
            raise ValueError(f"Unknown system type: {system_name}")
    
    async def _execute_core_reasoning(
        self,
        system: Any,
        query: str,
        context: Dict[str, Any],
        **kwargs
    ) -> Dict[str, Any]:
        """Execute core reasoning systems (ReAct, ReWOO, etc.)"""
        
        # Convert query and context to appropriate format
        if hasattr(system, 'run'):
            result = await system.run(query, context=context, **kwargs)
        elif hasattr(system, 'ainvoke'):
            result = await system.ainvoke({"query": query, "context": context}, **kwargs)
        else:
            result = await system(query, context, **kwargs)
        
        # Standardize output format
        return {
            "answer": result.get("answer", str(result)),
            "reasoning_chain": result.get("reasoning_chain", []),
            "confidence": result.get("confidence", 0.8),
            "sources": result.get("sources", []),
            "intermediate_steps": result.get("intermediate_steps", [])
        }
    
    async def _execute_neural_symbolic(
        self,
        system: NeuralSymbolicRetriever,
        query: str,
        context: Dict[str, Any],
        **kwargs
    ) -> Dict[str, Any]:
        """Execute neural-symbolic reasoning"""
        
        # Use hybrid retrieval with reasoning
        results = await system.retrieve_with_reasoning(
            query=query,
            reasoning_depth=kwargs.get("reasoning_depth", 3),
            top_k=kwargs.get("top_k", 10)
        )
        
        # Extract answer from top results
        if results:
            answer = "\n\n".join([result.content for result in results[:3]])
            reasoning_chain = []
            
            for result in results:
                if hasattr(result, 'reasoning_chain'):
                    reasoning_chain.extend(result.reasoning_chain)
        else:
            answer = "No relevant information found."
            reasoning_chain = []
        
        return {
            "answer": answer,
            "reasoning_chain": reasoning_chain,
            "confidence": results[0].score if results else 0.0,
            "sources": [
                {"content": r.content, "score": r.score}
                for r in results[:5]
            ],
            "knowledge_graph_paths": [
                r.reasoning_chain for r in results if hasattr(r, 'reasoning_chain')
            ]
        }
    
    async def _execute_mcts(
        self,
        system: MCTSFinancialAnalyzer,
        query: str,
        context: Dict[str, Any],
        **kwargs
    ) -> Dict[str, Any]:
        """Execute MCTS optimization"""
        
        # Extract financial context
        portfolio = context.get("portfolio", kwargs.get("portfolio", {}))
        risk_constraints = context.get("risk_constraints", kwargs.get("risk_constraints", {}))
        market_data = context.get("market_data", kwargs.get("market_data", {}))
        
        if not portfolio:
            return {
                "answer": "MCTS requires portfolio data for optimization",
                "error": "Missing portfolio context"
            }
        
        # Run optimization
        optimization_result = await system.optimize_portfolio(
            current_portfolio=portfolio,
            risk_constraints=risk_constraints,
            market_data=market_data
        )
        
        # Format response
        answer = f"""
Portfolio Optimization Analysis:

Optimal Allocation:
{self._format_portfolio(optimization_result.optimal_allocation)}

Expected Return: {optimization_result.expected_return:.2%}
Risk Score: {optimization_result.risk_score:.2f}
Sharpe Ratio: {optimization_result.sharpe_ratio:.2f}

Recommendation: {optimization_result.reasoning_path[-1] if optimization_result.reasoning_path else 'See optimal allocation above'}
"""
        
        return {
            "answer": answer.strip(),
            "reasoning_chain": optimization_result.reasoning_path,
            "confidence": optimization_result.confidence_score,
            "optimization_details": {
                "expected_return": optimization_result.expected_return,
                "risk_score": optimization_result.risk_score,
                "sharpe_ratio": optimization_result.sharpe_ratio,
                "optimal_allocation": optimization_result.optimal_allocation
            }
        }
    
    async def _execute_dspy(
        self,
        system: DSPyFinancialProcessor,
        query: str,
        context: Dict[str, Any],
        **kwargs
    ) -> Dict[str, Any]:
        """Execute DSPy self-improving reasoning"""
        
        # Extract financial data from context
        financial_data = context.get("financial_data", kwargs.get("financial_data", {}))
        user_context = context.get("user_context", kwargs.get("user_context", {}))
        
        # Generate analysis using optimized DSPy modules
        analysis_result = await system.generate_analysis(
            financial_data=financial_data,
            user_context=user_context,
            query=query
        )
        
        return {
            "answer": analysis_result["analysis"],
            "reasoning_chain": [
                {"step": "data_extraction", "content": "Processed financial data"},
                {"step": "analysis_generation", "content": analysis_result["analysis"]},
                {"step": "recommendation_synthesis", "content": str(analysis_result["recommendations"])}
            ],
            "confidence": analysis_result["confidence_score"],
            "recommendations": analysis_result["recommendations"],
            "risk_assessment": analysis_result["risk_assessment"],
            "supporting_evidence": analysis_result["supporting_evidence"]
        }
```

### 3. Result Combination Strategies

```python
    async def _weighted_ensemble_combination(
        self,
        results: Dict[str, Dict[str, Any]]
    ) -> Dict[str, Any]:
        """Combine results using weighted ensemble"""
        
        # Define weights based on system reliability and query type
        system_weights = {
            "tool_calling": 0.25,
            "react": 0.20,
            "reasoning": 0.20,
            "rewoo": 0.15,
            "neural_symbolic": 0.15,
            "mcts": 0.30,  # Higher weight for financial optimization
            "dspy": 0.25,
            "jena": 0.10
        }
        
        valid_results = {k: v for k, v in results.items() if "error" not in v}
        
        if not valid_results:
            return {"error": "All reasoning systems failed"}
        
        # Calculate weighted confidence
        total_weight = 0
        weighted_confidence = 0
        
        for system, result in valid_results.items():
            weight = system_weights.get(system, 0.1)
            confidence = result.get("confidence", 0.5)
            
            weighted_confidence += weight * confidence
            total_weight += weight
        
        if total_weight > 0:
            weighted_confidence /= total_weight
        
        # Combine answers (prioritize highest confidence or most specialized)
        primary_answer = None
        primary_confidence = 0
        
        for system, result in valid_results.items():
            system_confidence = result.get("confidence", 0.5)
            if system_confidence > primary_confidence:
                primary_answer = result.get("answer", "")
                primary_confidence = system_confidence
        
        # Collect all reasoning chains
        combined_reasoning = []
        for system, result in valid_results.items():
            reasoning_chain = result.get("reasoning_chain", [])
            if reasoning_chain:
                combined_reasoning.append({
                    "system": system,
                    "reasoning": reasoning_chain
                })
        
        # Combine sources
        all_sources = []
        for result in valid_results.values():
            sources = result.get("sources", [])
            all_sources.extend(sources)
        
        return {
            "answer": primary_answer,
            "reasoning_chain": combined_reasoning,
            "confidence": weighted_confidence,
            "sources": all_sources[:10],  # Top 10 sources
            "system_results": {
                system: {
                    "answer": result.get("answer", ""),
                    "confidence": result.get("confidence", 0.0)
                }
                for system, result in valid_results.items()
            }
        }
    
    async def _confidence_ranking_combination(
        self,
        results: Dict[str, Dict[str, Any]]
    ) -> Dict[str, Any]:
        """Combine results by ranking confidence scores"""
        
        valid_results = {k: v for k, v in results.items() if "error" not in v}
        
        if not valid_results:
            return {"error": "All reasoning systems failed"}
        
        # Sort by confidence
        ranked_results = sorted(
            valid_results.items(),
            key=lambda x: x[1].get("confidence", 0.0),
            reverse=True
        )
        
        # Use highest confidence result as primary
        primary_system, primary_result = ranked_results[0]
        
        # Add supporting evidence from other systems
        supporting_answers = []
        for system, result in ranked_results[1:]:
            if result.get("confidence", 0.0) > 0.7:  # High confidence threshold
                supporting_answers.append({
                    "system": system,
                    "answer": result.get("answer", ""),
                    "confidence": result.get("confidence", 0.0)
                })
        
        return {
            "answer": primary_result.get("answer", ""),
            "reasoning_chain": primary_result.get("reasoning_chain", []),
            "confidence": primary_result.get("confidence", 0.0),
            "sources": primary_result.get("sources", []),
            "primary_system": primary_system,
            "supporting_evidence": supporting_answers
        }
```

## Practical Integration Examples

### 1. Financial Analysis Pipeline

```python
class FinancialAnalysisPipeline:
    """Complete financial analysis using multiple reasoning systems"""
    
    def __init__(self):
        self.orchestrator = ReasoningOrchestrator({
            "enable_dspy": True,
            "enable_mcts": True,
            "enable_neural_symbolic": True,
            "enable_reasoning_agent": True,
            "dspy_config": {...},
            "mcts_config": {...},
            "neural_symbolic_config": {...},
            "reasoning_config": {...}
        })
    
    async def comprehensive_analysis(
        self,
        documents: List[str],
        user_query: str,
        portfolio: Dict[str, float],
        user_profile: Dict[str, Any]
    ) -> Dict[str, Any]:
        """Complete financial analysis pipeline"""
        
        # Step 1: Extract and process documents using DSPy
        document_analysis = await self.orchestrator.reason(
            query=f"Extract key financial information from documents: {user_query}",
            context={
                "documents": documents,
                "financial_data": True
            },
            strategy="adaptive"  # Use DSPy
        )
        
        # Step 2: Research market context using Neural-Symbolic
        market_research = await self.orchestrator.reason(
            query=f"Research market context and trends relevant to: {user_query}",
            context={
                "domain": "financial_markets",
                "reasoning_depth": 3
            },
            strategy="hybrid"  # Use Neural-Symbolic
        )
        
        # Step 3: Optimize portfolio using MCTS
        portfolio_optimization = await self.orchestrator.reason(
            query=f"Optimize portfolio allocation based on analysis: {user_query}",
            context={
                "portfolio": portfolio,
                "risk_constraints": user_profile.get("risk_constraints", {}),
                "market_data": market_research.get("sources", [])
            },
            strategy="probabilistic"  # Use MCTS
        )
        
        # Step 4: Generate transparent explanation using Reasoning Agent
        explanation = await self.orchestrator.reason(
            query=f"Provide detailed explanation and recommendations for: {user_query}",
            context={
                "document_analysis": document_analysis,
                "market_research": market_research,
                "portfolio_optimization": portfolio_optimization,
                "require_transparency": True
            },
            strategy="transparent"  # Use Reasoning Agent
        )
        
        # Combine all results
        return {
            "comprehensive_analysis": explanation["answer"],
            "document_insights": document_analysis["answer"],
            "market_context": market_research["answer"],
            "portfolio_recommendations": portfolio_optimization["answer"],
            "confidence_scores": {
                "document_analysis": document_analysis["confidence"],
                "market_research": market_research["confidence"],
                "portfolio_optimization": portfolio_optimization["confidence"],
                "overall_explanation": explanation["confidence"]
            },
            "reasoning_transparency": {
                "document_processing": document_analysis["reasoning_chain"],
                "market_research": market_research["reasoning_chain"],
                "optimization_process": portfolio_optimization["reasoning_chain"],
                "final_reasoning": explanation["reasoning_chain"]
            }
        }
```

### 2. Research Assistant Integration

```python
class ResearchAssistantPipeline:
    """Research assistant using multiple reasoning approaches"""
    
    def __init__(self):
        self.orchestrator = ReasoningOrchestrator({
            "enable_neural_symbolic": True,
            "enable_jena": True,
            "enable_react": True,
            "enable_reasoning_agent": True,
            "neural_symbolic_config": {...},
            "jena_config": {...},
            "react_config": {...},
            "reasoning_config": {...}
        })
    
    async def research_query(
        self,
        research_question: str,
        domain: str = "general",
        depth: str = "comprehensive"
    ) -> Dict[str, Any]:
        """Execute research query using appropriate reasoning systems"""
        
        # Determine research strategy based on question type
        if "knowledge graph" in research_question.lower() or "relationships" in research_question.lower():
            primary_strategy = "semantic"  # Use Jena
        elif "explore" in research_question.lower() or "investigate" in research_question.lower():
            primary_strategy = "interactive"  # Use ReAct
        else:
            primary_strategy = "hybrid"  # Use Neural-Symbolic
        
        # Multi-system approach for comprehensive research
        research_systems = ["neural_symbolic", "jena"] if depth == "comprehensive" else [primary_strategy]
        
        research_result = await self.orchestrator.reason(
            query=research_question,
            context={
                "domain": domain,
                "research_depth": depth,
                "reasoning_depth": 3 if depth == "comprehensive" else 2
            },
            combine_systems=research_systems
        )
        
        # Generate summary with transparent reasoning
        summary = await self.orchestrator.reason(
            query=f"Synthesize research findings into comprehensive summary: {research_question}",
            context={
                "research_results": research_result,
                "require_transparency": True,
                "domain": domain
            },
            strategy="transparent"
        )
        
        return {
            "research_summary": summary["answer"],
            "detailed_findings": research_result["answer"],
            "confidence": (research_result["confidence"] + summary["confidence"]) / 2,
            "reasoning_process": summary["reasoning_chain"],
            "source_analysis": research_result.get("sources", []),
            "methodology": {
                "primary_strategy": primary_strategy,
                "systems_used": research_systems,
                "depth": depth
            }
        }
```

### 3. Adaptive System Selection

```python
class AdaptiveReasoningSelector:
    """Intelligent system selection based on query analysis"""
    
    def __init__(self):
        self.query_classifier = self._initialize_classifier()
        self.performance_tracker = {}
        
    async def select_optimal_systems(
        self,
        query: str,
        context: Dict[str, Any] = None
    ) -> Dict[str, Any]:
        """Select optimal reasoning systems based on query analysis"""
        
        # Analyze query characteristics
        query_features = await self._extract_query_features(query, context)
        
        # Rule-based selection
        recommended_systems = []
        
        # Financial queries -> MCTS + DSPy
        if query_features["is_financial"]:
            recommended_systems.extend(["mcts", "dspy"])
        
        # Knowledge queries -> Neural-Symbolic + Jena
        if query_features["requires_knowledge"]:
            recommended_systems.extend(["neural_symbolic", "jena"])
        
        # Exploratory queries -> ReAct
        if query_features["is_exploratory"]:
            recommended_systems.append("react")
        
        # Structured workflows -> ReWOO
        if query_features["is_structured"]:
            recommended_systems.append("rewoo")
        
        # Transparency required -> Reasoning Agent
        if query_features["requires_explanation"]:
            recommended_systems.append("reasoning")
        
        # Default fallback -> Tool Calling
        if not recommended_systems:
            recommended_systems.append("tool_calling")
        
        # Remove duplicates and prioritize
        recommended_systems = list(dict.fromkeys(recommended_systems))
        
        # Consider historical performance
        performance_adjusted = await self._adjust_for_performance(
            recommended_systems, query_features
        )
        
        return {
            "recommended_systems": performance_adjusted,
            "query_analysis": query_features,
            "selection_reasoning": self._explain_selection(
                query_features, performance_adjusted
            )
        }
    
    async def _extract_query_features(
        self,
        query: str,
        context: Dict[str, Any] = None
    ) -> Dict[str, Any]:
        """Extract features from query for system selection"""
        
        query_lower = query.lower()
        context = context or {}
        
        return {
            "is_financial": any(word in query_lower for word in [
                "portfolio", "investment", "stock", "risk", "return",
                "financial", "market", "trading", "asset"
            ]),
            "requires_knowledge": any(word in query_lower for word in [
                "relationship", "connection", "how does", "why does",
                "knowledge", "research", "study", "analysis"
            ]),
            "is_exploratory": any(word in query_lower for word in [
                "explore", "investigate", "discover", "find out",
                "what if", "could", "might", "possible"
            ]),
            "is_structured": any(word in query_lower for word in [
                "step by step", "process", "workflow", "procedure",
                "systematic", "methodical", "plan"
            ]),
            "requires_explanation": any(word in query_lower for word in [
                "explain", "why", "how", "justify", "reasoning",
                "transparent", "show me", "walk through"
            ]),
            "is_optimization": any(word in query_lower for word in [
                "optimize", "best", "improve", "maximize", "minimize",
                "efficient", "performance", "enhance"
            ]),
            "complexity_score": len(query.split()) / 10.0,  # Simple complexity measure
            "has_context": bool(context),
            "domain": context.get("domain", "general")
        }
```

## Best Practices for Integration

### 1. Performance Optimization

```python
# Configuration for optimal performance
optimal_config = {
    "enable_caching": True,
    "cache_config": {
        "ttl_seconds": 3600,
        "max_size_mb": 512
    },
    "parallel_execution": True,
    "max_concurrent_systems": 3,
    "timeout_seconds": 30,
    "fallback_strategy": "fast_systems_first"
}
```

### 2. Error Handling and Resilience

```python
class RobustReasoningOrchestrator(ReasoningOrchestrator):
    """Enhanced orchestrator with robust error handling"""
    
    async def reason_with_resilience(self, *args, **kwargs):
        """Reasoning with comprehensive error handling"""
        
        try:
            return await self.reason(*args, **kwargs)
        
        except Exception as e:
            self.logger.error(f"Primary reasoning failed: {e}")
            
            # Try simplified approach
            try:
                return await self._simplified_reasoning(*args, **kwargs)
            except Exception as e2:
                self.logger.error(f"Simplified reasoning failed: {e2}")
                
                # Final fallback to basic response
                return {
                    "answer": "I encountered an error processing your request. Please try a simpler query.",
                    "error": str(e),
                    "fallback_used": True
                }
```

### 3. Monitoring and Analytics

```python
class ReasoningAnalytics:
    """Analytics and monitoring for reasoning systems"""
    
    def __init__(self):
        self.metrics = []
        self.system_performance = {}
    
    def track_reasoning_session(
        self,
        query: str,
        systems_used: List[str],
        execution_time: float,
        success: bool,
        confidence: float
    ):
        """Track reasoning session metrics"""
        
        self.metrics.append({
            "timestamp": datetime.now().isoformat(),
            "query_hash": hash(query),
            "systems_used": systems_used,
            "execution_time": execution_time,
            "success": success,
            "confidence": confidence
        })
    
    def generate_insights(self) -> Dict[str, Any]:
        """Generate insights from collected metrics"""
        
        if not self.metrics:
            return {"error": "No metrics available"}
        
        # Calculate system performance
        system_stats = {}
        for metric in self.metrics:
            for system in metric["systems_used"]:
                if system not in system_stats:
                    system_stats[system] = {
                        "total_uses": 0,
                        "total_time": 0,
                        "success_count": 0,
                        "total_confidence": 0
                    }
                
                system_stats[system]["total_uses"] += 1
                system_stats[system]["total_time"] += metric["execution_time"]
                if metric["success"]:
                    system_stats[system]["success_count"] += 1
                system_stats[system]["total_confidence"] += metric["confidence"]
        
        # Calculate averages
        for system, stats in system_stats.items():
            stats["avg_time"] = stats["total_time"] / stats["total_uses"]
            stats["success_rate"] = stats["success_count"] / stats["total_uses"]
            stats["avg_confidence"] = stats["total_confidence"] / stats["total_uses"]
        
        return {
            "total_sessions": len(self.metrics),
            "system_performance": system_stats,
            "overall_success_rate": sum(m["success"] for m in self.metrics) / len(self.metrics),
            "avg_execution_time": sum(m["execution_time"] for m in self.metrics) / len(self.metrics),
            "recommendations": self._generate_optimization_recommendations(system_stats)
        }
```

This integration guide provides a comprehensive framework for combining AIQToolkit's reasoning systems effectively. The modular architecture allows for flexible system selection, robust error handling, and continuous optimization based on performance metrics.