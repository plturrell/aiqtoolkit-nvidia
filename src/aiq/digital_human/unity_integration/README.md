# Unity Integration for AIQToolkit Digital Human

This Unity integration provides a robust connector for interfacing Unity applications with the AIQToolkit Digital Human backend services.

## Features

- **REST API Integration**: Full support for all backend REST endpoints
- **WebSocket Support**: Real-time bidirectional communication
- **MCP Server Interface**: Access to Model Context Protocol servers (web search, file browser)
- **Automatic Reconnection**: Handles network interruptions gracefully
- **Error Handling**: Comprehensive error reporting and recovery
- **Session Management**: Maintains persistent sessions across connections
- **Platform Support**: Works on all Unity-supported platforms (Windows, Mac, Linux, Mobile, WebGL)

## Installation

1. Copy the `BackendConnector.cs` script to your Unity project's Scripts folder
2. Ensure you have the following Unity packages installed:
   - Newtonsoft Json.NET
   - WebSocketSharp (or Unity's native WebSocket support)

## Quick Start

### Basic Setup

1. Create an empty GameObject in your scene
2. Add the `BackendConnector` component to it
3. Configure the connection settings in the Inspector:
   - Backend URL: `http://localhost:8000`
   - WebSocket URL: `ws://localhost:8000/ws`
   - (Optional) Add authentication tokens if required

### Example Usage

```csharp
using AIQToolkit.DigitalHuman.UnityIntegration;

public class MyDigitalHumanController : MonoBehaviour
{
    private BackendConnector connector;
    
    void Start()
    {
        // Get the connector instance
        connector = GetComponent<BackendConnector>();
        
        // Subscribe to events
        connector.OnMessageReceived += HandleMessage;
        connector.OnConnectionStateChanged += HandleConnectionChange;
        connector.OnError += HandleError;
        
        // Create a new session
        StartCoroutine(CreateSession());
    }
    
    IEnumerator CreateSession()
    {
        var config = new BackendConnector.SessionConfig
        {
            agent_type = "digital_human",
            settings = new Dictionary<string, object>
            {
                { "enable_voice", true },
                { "language", "en" }
            }
        };
        
        var sessionTask = connector.CreateSession(config);
        yield return new WaitUntil(() => sessionTask.IsCompleted);
        
        if (!string.IsNullOrEmpty(sessionTask.Result))
        {
            Debug.Log($"Session created: {sessionTask.Result}");
        }
    }
    
    async void SendMessage(string message)
    {
        var response = await connector.SendChatMessage(message);
        if (response != null)
        {
            Debug.Log($"Response: {response.message}");
        }
    }
}
```

## API Reference

### Properties

- `IsConnected`: Returns true if WebSocket is connected
- `SessionId`: Current session identifier

### Methods

#### `CreateSession(SessionConfig config)`
Creates a new session with the backend.

```csharp
var config = new BackendConnector.SessionConfig
{
    agent_type = "digital_human",
    settings = new Dictionary<string, object>()
};
string sessionId = await connector.CreateSession(config);
```

#### `SendChatMessage(string message, Dictionary<string, object> metadata)`
Sends a chat message to the backend.

```csharp
var response = await connector.SendChatMessage("Hello, AI!", metadata);
```

#### `CallMCPTool(string tool, Dictionary<string, object> parameters)`
Calls an MCP server tool.

```csharp
// Web search example
var results = await connector.CallMCPTool("web_search", new Dictionary<string, object>
{
    { "query", "Unity AI integration" },
    { "max_results", 5 }
});

// File browser example
var files = await connector.CallMCPTool("file_browser", new Dictionary<string, object>
{
    { "path", "/home/user/documents" }
});
```

### Events

- `OnMessageReceived`: Fired when a message is received via WebSocket
- `OnConnectionStateChanged`: Fired when connection state changes
- `OnError`: Fired when an error occurs

## WebSocket Message Types

The connector handles the following WebSocket message types:

- `init`: Initialize connection
- `chat_message`: Send/receive chat messages
- `mcp_tool_call`: Execute MCP tool calls
- `consensus_update`: Multi-agent consensus updates
- `heartbeat`: Keep-alive messages

## Error Handling

The connector includes comprehensive error handling:

- Automatic reconnection with exponential backoff
- Fallback from WebSocket to REST API
- Detailed error messages via the `OnError` event

## Platform-Specific Considerations

### Mobile (iOS/Android)
- Handles app pause/resume events
- Manages connection state during backgrounding

### WebGL
- Uses Unity's WebSocket implementation
- Handles browser connection limits

### Desktop
- Full WebSocket support
- Persistent connections

## Configuration

### Backend URL
Set the base URL for REST API calls:
```csharp
connector.backendURL = "https://api.mydigitalhuman.com";
```

### WebSocket URL
Set the WebSocket endpoint:
```csharp
connector.websocketURL = "wss://api.mydigitalhuman.com/ws";
```

### Authentication
Add authentication tokens:
```csharp
connector.authToken = "your-jwt-token";
connector.apiKey = "your-api-key";
```

## Troubleshooting

### Connection Issues
1. Ensure the backend server is running
2. Check firewall settings
3. Verify URL configuration
4. Check authentication credentials

### WebSocket Errors
- Enable debug logging in the BackendConnector
- Check browser console for WebGL builds
- Verify WebSocket protocol support

### Performance
- Adjust reconnection delay and max attempts
- Use connection pooling for multiple instances
- Implement message batching for high-frequency updates

## Example Projects

See the `BackendConnectorExample.cs` for a complete implementation example with UI integration.

## Support

For issues or questions, please refer to the main AIQToolkit documentation or create an issue in the repository.