# Core Reasoning Systems

## Overview

AIQToolkit provides four foundational reasoning systems that form the backbone of intelligent decision-making. Each system implements a different approach to reasoning, allowing you to choose the most appropriate method for your specific use case.

## 1. ReAct Reasoning System

### Concept: **Reasoning and Acting**

The ReAct system implements an iterative approach where the AI alternates between **reasoning** (thinking about the problem) and **acting** (taking concrete steps), using observations from actions to refine its understanding.

### Architecture

```
┌─────────────────────────────────────────────────────────────────┐
│                    ReAct Reasoning Loop                          │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│  Question/Task Input                                            │
│          │                                                      │
│          ▼                                                      │
│  ┌─────────────┐    ┌─────────────┐    ┌─────────────┐         │
│  │   THOUGHT   │───▶│   ACTION    │───▶│ OBSERVATION │         │
│  │             │    │             │    │             │         │
│  │ Analyze     │    │ Use Tool    │    │ Get Result  │         │
│  │ Situation   │    │ Call API    │    │ See Output  │         │
│  │ Plan Next   │    │ Execute     │    │ Update      │         │
│  │ Step        │    │ Command     │    │ Knowledge   │         │
│  └─────────────┘    └─────────────┘    └─────────────┘         │
│          ▲                                      │               │
│          │                                      │               │
│          └──────────────────────────────────────┘               │
│                                                                 │
│  Loop continues until final answer is reached                  │
└─────────────────────────────────────────────────────────────────┘
```

### Key Features

- **Iterative Refinement**: Each observation improves understanding
- **Tool Integration**: Seamless use of external tools and APIs
- **Error Recovery**: Can retry and adjust approach based on failures
- **Transparent Process**: Full reasoning chain is visible and auditable
- **Context Maintenance**: Maintains conversation history and context

### Implementation Details

```python
# Core ReAct Agent Configuration
class ReActAgentConfig(FunctionBaseConfig, name="react_agent"):
    llm_name: LLMRef = Field(description="LLM for reasoning")
    tools: list[FunctionRef] = Field(description="Available tools")
    max_iterations: int = Field(default=10, description="Maximum reasoning loops")
    retry_parsing_errors: bool = Field(default=True, description="Retry on parse errors")
    detailed_logs: bool = Field(default=False, description="Enable verbose logging")

# ReAct Process Flow
class ReActGraphState(BaseModel):
    messages: list[BaseMessage] = Field(default_factory=list)
    agent_scratchpad: list[AgentAction] = Field(default_factory=list)
    tool_responses: list[BaseMessage] = Field(default_factory=list)
```

### Usage Examples

```python
# Basic ReAct Agent Setup
from aiq.agent.react_agent import build_react_agent

config = ReActAgentConfig(
    llm_name="openai_gpt4",
    tools=["web_search", "calculator", "code_execution"],
    max_iterations=15,
    detailed_logs=True
)

agent = await build_react_agent(config, builder)

# Execute reasoning task
result = await agent.run("Analyze the latest trends in AI research and calculate market impact")

# Examine reasoning process
for step in result.reasoning_steps:
    print(f"Thought: {step.thought}")
    print(f"Action: {step.action}")
    print(f"Observation: {step.observation}")
```

### Best Use Cases

- **Research Tasks**: Literature review, data analysis, hypothesis testing
- **Interactive Problem Solving**: Customer support, troubleshooting, consultation
- **Multi-step Analysis**: Financial analysis, market research, technical investigation
- **Learning Scenarios**: Educational assistance, skill development, exploration

## 2. ReWOO Reasoning System

### Concept: **Reasoning WithOut Observation**

ReWOO implements a **plan-first** approach where the AI creates a complete execution plan upfront, then executes all steps in sequence without intermediate observations affecting the plan.

### Architecture

```
┌─────────────────────────────────────────────────────────────────┐
│                   ReWOO Planning System                         │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│  Question/Task Input                                            │
│          │                                                      │
│          ▼                                                      │
│  ┌─────────────────┐                                           │
│  │    PLANNER      │  Create complete execution plan           │
│  │                 │                                           │
│  │ • Analyze task  │  Plan: #E1 = Tool1(input)               │
│  │ • Identify      │        #E2 = Tool2(#E1)                 │
│  │   required      │        #E3 = Tool3(#E1, #E2)           │
│  │   tools         │        Return: Analysis(#E3)            │
│  │ • Create plan   │                                           │
│  └─────────────────┘                                           │
│          │                                                      │
│          ▼                                                      │
│  ┌─────────────────┐                                           │
│  │    WORKER       │  Execute plan steps in sequence          │
│  │                 │                                           │
│  │ • Execute #E1   │  #E1 = search_results                   │
│  │ • Execute #E2   │  #E2 = processed_data                   │
│  │ • Execute #E3   │  #E3 = combined_analysis                │
│  │ • Store results │                                           │
│  └─────────────────┘                                           │
│          │                                                      │
│          ▼                                                      │
│  ┌─────────────────┐                                           │
│  │     SOLVER      │  Generate final answer                    │
│  │                 │                                           │
│  │ • Combine all   │  Final Answer: Based on #E1, #E2, #E3   │
│  │   results       │  comprehensive analysis shows...         │
│  │ • Generate      │                                           │
│  │   final answer  │                                           │
│  └─────────────────┘                                           │
└─────────────────────────────────────────────────────────────────┘
```

### Key Features

- **Comprehensive Planning**: Complete execution strategy created upfront
- **Parallel Execution**: Independent steps can be executed simultaneously
- **Cost Efficient**: Fewer LLM calls due to planning approach
- **Predictable**: Deterministic execution paths
- **Scalable**: Well-suited for batch processing

### Implementation Details

```python
# ReWOO Agent Configuration
class ReWOOAgentConfig(FunctionBaseConfig, name="rewoo_agent"):
    llm_name: LLMRef = Field(description="LLM for planning and solving")
    planner_prompt: str = Field(description="Planner prompt template")
    solver_prompt: str = Field(description="Solver prompt template")
    tools: list[FunctionRef] = Field(description="Available tools")
    use_tool_schema: bool = Field(default=True, description="Include tool schemas")

# ReWOO State Management
class ReWOOGraphState(BaseModel):
    task: HumanMessage = Field(default_factory=lambda: HumanMessage(content=""))
    plan: AIMessage = Field(default_factory=lambda: AIMessage(content=""))
    steps: AIMessage = Field(default_factory=lambda: AIMessage(content=""))
    intermediate_results: dict[str, ToolMessage] = Field(default_factory=dict)
    result: AIMessage = Field(default_factory=lambda: AIMessage(content=""))
```

### Usage Examples

```python
# ReWOO Agent Setup
from aiq.agent.rewoo_agent import build_rewoo_agent

config = ReWOOAgentConfig(
    llm_name="openai_gpt4",
    tools=["data_analyzer", "report_generator", "chart_creator"],
    planner_prompt="""Create a plan to solve: {task}
Available tools: {tools}
Plan format: #E1 = tool(input), #E2 = tool(#E1), etc.""",
    solver_prompt="""Based on execution results: {intermediate_results}
Provide final answer for: {task}"""
)

agent = await build_rewoo_agent(config, builder)

# Execute with planning
result = await agent.run("Create comprehensive market analysis report with charts")

# Examine plan and execution
print("Generated Plan:")
print(result.plan)
print("\nExecution Results:")
for step, result in result.intermediate_results.items():
    print(f"{step}: {result}")
```

### Best Use Cases

- **Batch Processing**: Large-scale data processing, report generation
- **Structured Workflows**: Document processing, standardized analysis
- **Cost-Sensitive Operations**: Minimal LLM usage, budget constraints
- **Parallel Processing**: Independent task execution, distributed systems

## 3. Reasoning Agent System

### Concept: **Function Augmentation with Transparency**

The Reasoning Agent acts as a **meta-reasoning layer** that augments existing functions with transparent reasoning capabilities, creating detailed execution plans and explanations.

### Architecture

```
┌─────────────────────────────────────────────────────────────────┐
│                Reasoning Agent Augmentation                     │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│  Input Query/Task                                               │
│          │                                                      │
│          ▼                                                      │
│  ┌─────────────────────────────────┐                           │
│  │       REASONING PLANNER         │                           │
│  │                                 │                           │
│  │ • Analyze target function       │                           │
│  │ • Understand available tools    │                           │
│  │ • Create step-by-step plan      │                           │
│  │ • Generate reasoning template   │                           │
│  └─────────────────────────────────┘                           │
│          │                                                      │
│          ▼                                                      │
│  ┌─────────────────────────────────┐                           │
│  │    INSTRUCTION GENERATOR        │                           │
│  │                                 │                           │
│  │ • Combine query with plan       │                           │
│  │ • Create detailed instructions  │                           │
│  │ • Add reasoning guidance        │                           │
│  │ • Format for target function    │                           │
│  └─────────────────────────────────┘                           │
│          │                                                      │
│          ▼                                                      │
│  ┌─────────────────────────────────┐                           │
│  │      AUGMENTED FUNCTION         │                           │
│  │                                 │                           │
│  │ • Execute with enhanced input   │                           │
│  │ • Maintain reasoning context    │                           │
│  │ • Generate transparent output   │                           │
│  │ • Provide step justifications   │                           │
│  └─────────────────────────────────┘                           │
└─────────────────────────────────────────────────────────────────┘
```

### Key Features

- **Function Enhancement**: Adds reasoning to any existing function
- **Transparent Process**: Detailed reasoning plans and explanations
- **Tool Awareness**: Understands available tools and dependencies
- **Flexible Integration**: Works with streaming and non-streaming functions
- **Reasoning Extraction**: Can remove model-specific reasoning tags (e.g., <think>)

### Implementation Details

```python
# Reasoning Function Configuration
class ReasoningFunctionConfig(FunctionBaseConfig, name="reasoning_agent"):
    llm_name: LLMRef = Field(description="LLM for reasoning")
    augmented_fn: FunctionRef = Field(description="Function to enhance")
    verbose: bool = Field(default=False, description="Detailed logging")
    reasoning_prompt_template: str = Field(
        default="""Create detailed execution plan for: {augmented_function_desc}
Input: {input_text}
Tools: {tools}
Plan:""",
        description="Reasoning prompt template"
    )
    instruction_prompt_template: str = Field(
        default="""Answer: {input_text}
Execution plan: {reasoning_output}
Follow the plan to provide comprehensive answer.""",
        description="Instruction prompt template"
    )

# Reasoning Process Flow
async def reasoning_process(input_message, reasoning_template, function):
    # Generate reasoning plan
    reasoning_output = await llm.astream(reasoning_template)
    
    # Create enhanced instructions
    enhanced_input = await instruction_template.ainvoke({
        "input_text": input_text,
        "reasoning_output": reasoning_output
    })
    
    # Execute augmented function
    return await function.acall(enhanced_input)
```

### Usage Examples

```python
# Reasoning Agent Setup
from aiq.agent.reasoning_agent import build_reasoning_function

config = ReasoningFunctionConfig(
    llm_name="openai_gpt4",
    augmented_fn="document_analyzer",
    verbose=True,
    reasoning_prompt_template="""
You are expert reasoning model. Create detailed plan for: {augmented_function_desc}

Input: {input_text}
Available tools: {tools}

Provide step-by-step plan:
1. Identify key components
2. Select appropriate tools
3. Define execution sequence
4. Expected outcomes

Plan:""",
    instruction_prompt_template="""
Answer this question: {input_text}

Use this execution plan as guidance: {reasoning_output}

Provide comprehensive, reasoned response."""
)

reasoning_agent = await build_reasoning_function(config, builder)

# Execute with reasoning
result = await reasoning_agent.run("Analyze this contract for potential risks")

# Access reasoning process
print("Reasoning Plan:")
print(result.reasoning_plan)
print("\nFinal Analysis:")
print(result.analysis)
```

### Best Use Cases

- **AI Transparency**: Making black-box functions explainable
- **Audit Requirements**: Systems requiring reasoning trails
- **Complex Analysis**: Financial analysis, legal review, research evaluation
- **Function Enhancement**: Adding intelligence to existing workflows

## 4. Tool Calling Reasoning System

### Concept: **Native Function Integration**

The Tool Calling system leverages modern LLMs' built-in function calling capabilities for direct, efficient tool invocation with integrated reasoning.

### Architecture

```
┌─────────────────────────────────────────────────────────────────┐
│                 Tool Calling Reasoning                          │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│  User Query                                                     │
│       │                                                         │
│       ▼                                                         │
│  ┌─────────────────────────────────┐                           │
│  │         LLM AGENT               │                           │
│  │                                 │                           │
│  │ • Analyze query requirements    │                           │
│  │ • Select appropriate tools      │                           │
│  │ • Generate function calls       │                           │
│  │ • Handle tool responses         │                           │
│  └─────────────────────────────────┘                           │
│       │                                                         │
│       ▼                                                         │
│  ┌─────────────────────────────────┐                           │
│  │      TOOL SELECTION             │                           │
│  │                                 │                           │
│  │ • Native LLM function calling   │                           │
│  │ • Parallel tool execution       │                           │
│  │ • Parameter validation          │                           │
│  │ • Error handling & retry        │                           │
│  └─────────────────────────────────┘                           │
│       │                                                         │
│       ▼                                                         │
│  ┌─────────────────────────────────┐                           │
│  │      RESPONSE SYNTHESIS         │                           │
│  │                                 │                           │
│  │ • Process tool outputs          │                           │
│  │ • Combine results logically     │                           │
│  │ • Generate coherent response    │                           │
│  │ • Include reasoning rationale   │                           │
│  └─────────────────────────────────┘                           │
└─────────────────────────────────────────────────────────────────┘
```

### Key Features

- **Native Integration**: Uses LLM's built-in function calling
- **Parallel Execution**: Multiple tools can run simultaneously
- **Error Handling**: Automatic retry and error recovery
- **Parameter Optimization**: LLM optimizes tool parameters
- **Efficient Processing**: Minimal overhead, maximum performance

### Implementation Details

```python
# Tool Calling Agent Configuration
class ToolCallAgentConfig(FunctionBaseConfig, name="tool_calling_agent"):
    llm_name: LLMRef = Field(description="LLM with tool calling support")
    tools: list[FunctionRef] = Field(description="Available tools")
    handle_tool_errors: bool = Field(default=True, description="Handle tool errors")
    parallel_execution: bool = Field(default=True, description="Allow parallel tools")
    detailed_logs: bool = Field(default=False, description="Enable verbose logging")

# Tool Calling State
class ToolCallAgentGraphState(BaseModel):
    messages: list[BaseMessage] = Field(default_factory=list)

# Execution Flow
async def tool_execution_flow(state):
    # Agent decides on tool calls
    response = await llm.ainvoke(state.messages)
    
    if response.tool_calls:
        # Execute tools (potentially in parallel)
        tool_responses = await tool_executor.ainvoke(response.tool_calls)
        state.messages.extend([response] + tool_responses)
        return "continue"
    else:
        # Final answer ready
        state.messages.append(response)
        return "end"
```

### Usage Examples

```python
# Tool Calling Agent Setup
from aiq.agent.tool_calling_agent import build_tool_calling_agent

config = ToolCallAgentConfig(
    llm_name="openai_gpt4",  # Must support function calling
    tools=["financial_data_api", "chart_generator", "report_writer"],
    handle_tool_errors=True,
    parallel_execution=True,
    detailed_logs=True
)

agent = await build_tool_calling_agent(config, builder)

# Execute with native tool calling
result = await agent.run("Generate quarterly financial report with performance charts")

# Examine tool usage
for message in result.message_history:
    if hasattr(message, 'tool_calls'):
        print(f"Tool Calls: {message.tool_calls}")
    if hasattr(message, 'tool_call_id'):
        print(f"Tool Response: {message.content}")
```

### Best Use Cases

- **Modern LLM Integration**: GPT-4, Claude 3.5, Gemini with function calling
- **API Orchestration**: Complex API workflows, data integration
- **Real-time Systems**: Low-latency applications, live data processing
- **Function-Heavy Workflows**: Applications with many specialized tools

## Comparison and Selection Guide

### When to Use Each System

| System | Best For | Strengths | Considerations |
|--------|----------|-----------|----------------|
| **ReAct** | Interactive, exploratory tasks | Adaptive, transparent, error recovery | Higher API usage |
| **ReWOO** | Batch processing, structured workflows | Cost efficient, predictable, parallel | Less adaptive |
| **Reasoning Agent** | Transparency, augmentation | Explainable, enhances existing functions | Added complexity |
| **Tool Calling** | Modern LLMs, API-heavy tasks | Native integration, efficient, parallel | LLM capability dependent |

### Performance Characteristics

```python
# Performance comparison example
performance_metrics = {
    "react": {
        "api_calls": "High (iterative)",
        "transparency": "Excellent",
        "adaptability": "High",
        "cost": "Higher"
    },
    "rewoo": {
        "api_calls": "Low (planning)",
        "transparency": "Good",
        "adaptability": "Medium",
        "cost": "Lower"
    },
    "reasoning_agent": {
        "api_calls": "Medium (augmentation)",
        "transparency": "Excellent",
        "adaptability": "High",
        "cost": "Medium"
    },
    "tool_calling": {
        "api_calls": "Low (native)",
        "transparency": "Good",
        "adaptability": "High",
        "cost": "Lower"
    }
}
```

### Configuration Best Practices

1. **Choose Based on Use Case**: Match system to problem characteristics
2. **Configure Appropriately**: Set iteration limits, timeouts, error handling
3. **Monitor Performance**: Track API usage, latency, success rates
4. **Enable Transparency**: Use detailed logging for debugging and audit
5. **Test Thoroughly**: Validate with representative data and edge cases

The core reasoning systems provide a solid foundation for intelligent decision-making. For more advanced scenarios, consider combining these with the hybrid neural-symbolic systems, MCTS optimization, or self-improving DSPy components.