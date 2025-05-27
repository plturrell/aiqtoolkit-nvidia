using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using AIQToolkit.DigitalHuman.UnityIntegration;

/// <summary>
/// Example usage of the BackendConnector for Unity applications
/// This demonstrates how to integrate the digital human backend into a Unity UI
/// </summary>
public class BackendConnectorExample : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private InputField messageInput;
    [SerializeField] private Button sendButton;
    [SerializeField] private Text responseText;
    [SerializeField] private Text connectionStatus;
    [SerializeField] private GameObject loadingIndicator;
    
    [Header("Backend Settings")]
    [SerializeField] private BackendConnector backendConnector;
    
    // State management
    private bool isProcessing = false;
    
    private void Start()
    {
        // Get or create BackendConnector
        if (backendConnector == null)
        {
            GameObject connectorObj = GameObject.Find("BackendConnector");
            if (connectorObj == null)
            {
                connectorObj = new GameObject("BackendConnector");
                backendConnector = connectorObj.AddComponent<BackendConnector>();
            }
            else
            {
                backendConnector = connectorObj.GetComponent<BackendConnector>();
            }
        }
        
        // Subscribe to events
        backendConnector.OnMessageReceived += HandleMessageReceived;
        backendConnector.OnConnectionStateChanged += HandleConnectionStateChanged;
        backendConnector.OnError += HandleError;
        
        // Setup UI
        sendButton.onClick.AddListener(SendMessage);
        messageInput.onEndEdit.AddListener(delegate { SendMessageOnEnter(); });
        
        // Create initial session
        StartCoroutine(InitializeSession());
    }
    
    private void OnDestroy()
    {
        // Unsubscribe from events
        if (backendConnector != null)
        {
            backendConnector.OnMessageReceived -= HandleMessageReceived;
            backendConnector.OnConnectionStateChanged -= HandleConnectionStateChanged;
            backendConnector.OnError -= HandleError;
        }
    }
    
    /// <summary>
    /// Initialize a new session
    /// </summary>
    private IEnumerator InitializeSession()
    {
        UpdateUI(true);
        
        // Create a new session with custom configuration
        var config = new BackendConnector.SessionConfig
        {
            agent_type = "digital_human",
            settings = new Dictionary<string, object>
            {
                { "enable_voice", true },
                { "language", "en" },
                { "persona", "friendly_assistant" }
            }
        };
        
        var sessionTask = backendConnector.CreateSession(config);
        yield return new WaitUntil(() => sessionTask.IsCompleted);
        
        if (!string.IsNullOrEmpty(sessionTask.Result))
        {
            responseText.text = $"Session created: {sessionTask.Result}";
        }
        else
        {
            responseText.text = "Failed to create session";
        }
        
        UpdateUI(false);
    }
    
    /// <summary>
    /// Send a message to the backend
    /// </summary>
    private void SendMessage()
    {
        if (isProcessing || string.IsNullOrWhiteSpace(messageInput.text))
            return;
            
        string message = messageInput.text;
        messageInput.text = "";
        
        // Add message metadata
        var metadata = new Dictionary<string, object>
        {
            { "source", "unity_client" },
            { "timestamp", System.DateTime.UtcNow.ToString("O") },
            { "device", SystemInfo.deviceModel }
        };
        
        StartCoroutine(SendMessageCoroutine(message, metadata));
    }
    
    /// <summary>
    /// Send message on Enter key press
    /// </summary>
    private void SendMessageOnEnter()
    {
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            SendMessage();
        }
    }
    
    /// <summary>
    /// Coroutine to send message
    /// </summary>
    private IEnumerator SendMessageCoroutine(string message, Dictionary<string, object> metadata)
    {
        UpdateUI(true);
        responseText.text = $"You: {message}\n\nProcessing...";
        
        var task = backendConnector.SendChatMessage(message, metadata);
        yield return new WaitUntil(() => task.IsCompleted);
        
        if (task.Result != null)
        {
            responseText.text = $"You: {message}\n\nAssistant: {task.Result.message}";
        }
        
        UpdateUI(false);
    }
    
    /// <summary>
    /// Handle message received from WebSocket
    /// </summary>
    private void HandleMessageReceived(string message)
    {
        try
        {
            var messageObj = JsonUtility.FromJson<Dictionary<string, object>>(message);
            
            // Update UI on main thread
            StartCoroutine(UpdateUIOnMainThread(() =>
            {
                if (messageObj.ContainsKey("type") && messageObj["type"].ToString() == "chat_response")
                {
                    responseText.text = $"Assistant: {messageObj["message"]}";
                }
                else
                {
                    Debug.Log($"Received message: {message}");
                }
            }));
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to parse message: {e.Message}");
        }
    }
    
    /// <summary>
    /// Handle connection state changes
    /// </summary>
    private void HandleConnectionStateChanged(bool isConnected)
    {
        StartCoroutine(UpdateUIOnMainThread(() =>
        {
            connectionStatus.text = isConnected ? "Connected" : "Disconnected";
            connectionStatus.color = isConnected ? Color.green : Color.red;
            sendButton.interactable = isConnected;
        }));
    }
    
    /// <summary>
    /// Handle errors
    /// </summary>
    private void HandleError(string error)
    {
        StartCoroutine(UpdateUIOnMainThread(() =>
        {
            responseText.text = $"Error: {error}";
            Debug.LogError($"Backend error: {error}");
        }));
    }
    
    /// <summary>
    /// Update UI state
    /// </summary>
    private void UpdateUI(bool processing)
    {
        isProcessing = processing;
        loadingIndicator.SetActive(processing);
        sendButton.interactable = !processing && backendConnector.IsConnected;
        messageInput.interactable = !processing;
    }
    
    /// <summary>
    /// Update UI on main thread
    /// </summary>
    private IEnumerator UpdateUIOnMainThread(System.Action action)
    {
        yield return null; // Wait for next frame (main thread)
        action?.Invoke();
    }
    
    /// <summary>
    /// Example: Call MCP tools
    /// </summary>
    public void SearchWeb(string query)
    {
        StartCoroutine(CallMCPTool("web_search", new Dictionary<string, object>
        {
            { "query", query },
            { "max_results", 5 }
        }));
    }
    
    public void BrowseFiles(string path)
    {
        StartCoroutine(CallMCPTool("file_browser", new Dictionary<string, object>
        {
            { "path", path },
            { "include_hidden", false }
        }));
    }
    
    private IEnumerator CallMCPTool(string tool, Dictionary<string, object> parameters)
    {
        UpdateUI(true);
        responseText.text = $"Calling MCP tool: {tool}...";
        
        var task = backendConnector.CallMCPTool(tool, parameters);
        yield return new WaitUntil(() => task.IsCompleted);
        
        if (task.Result != null)
        {
            responseText.text = $"MCP Result: {JsonUtility.ToJson(task.Result)}";
        }
        
        UpdateUI(false);
    }
}