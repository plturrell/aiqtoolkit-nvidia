using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;
using WebSocketSharp;

namespace AIQToolkit.DigitalHuman.UnityIntegration
{
    /// <summary>
    /// Unity connector for AIQToolkit Digital Human backend services
    /// Handles REST API calls, WebSocket connections, and MCP server interactions
    /// </summary>
    public class BackendConnector : MonoBehaviour
    {
        [Header("Connection Settings")]
        [SerializeField] private string backendURL = "http://localhost:8000";
        [SerializeField] private string websocketURL = "ws://localhost:8000/ws";
        [SerializeField] private float reconnectDelay = 3f;
        [SerializeField] private int maxReconnectAttempts = 5;
        
        [Header("Authentication")]
        [SerializeField] private string apiKey = "";
        [SerializeField] private string authToken = "";
        
        // Events
        public event Action<string> OnMessageReceived;
        public event Action<bool> OnConnectionStateChanged;
        public event Action<string> OnError;
        
        // Connection state
        private WebSocket websocket;
        private bool isConnected = false;
        private int reconnectAttempts = 0;
        private Coroutine reconnectCoroutine;
        
        // HTTP client
        private static readonly HttpClient httpClient = new HttpClient();
        
        // Session management
        private string sessionId;
        private string connectionId;
        
        #region Unity Lifecycle
        
        private void Awake()
        {
            // Ensure persistent across scenes
            DontDestroyOnLoad(this.gameObject);
        }
        
        private void Start()
        {
            InitializeConnection();
        }
        
        private void OnDestroy()
        {
            DisconnectWebSocket();
        }
        
        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                DisconnectWebSocket();
            }
            else
            {
                InitializeConnection();
            }
        }
        
        private void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus && Application.platform == RuntimePlatform.IPhonePlayer)
            {
                DisconnectWebSocket();
            }
            else if (hasFocus)
            {
                InitializeConnection();
            }
        }
        
        #endregion
        
        #region Connection Management
        
        /// <summary>
        /// Initialize backend connection
        /// </summary>
        public void InitializeConnection()
        {
            StartCoroutine(InitializeAsync());
        }
        
        private IEnumerator InitializeAsync()
        {
            // First, test REST API connection
            yield return TestRESTConnection();
            
            // Then establish WebSocket connection
            ConnectWebSocket();
        }
        
        /// <summary>
        /// Test REST API connection
        /// </summary>
        private IEnumerator TestRESTConnection()
        {
            var request = UnityWebRequest.Get($"{backendURL}/health");
            if (!string.IsNullOrEmpty(authToken))
            {
                request.SetRequestHeader("Authorization", $"Bearer {authToken}");
            }
            
            yield return request.SendWebRequest();
            
            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("REST API connection successful");
            }
            else
            {
                Debug.LogError($"REST API connection failed: {request.error}");
                OnError?.Invoke($"REST API connection failed: {request.error}");
            }
        }
        
        #endregion
        
        #region WebSocket Management
        
        /// <summary>
        /// Connect to WebSocket server
        /// </summary>
        private void ConnectWebSocket()
        {
            try
            {
                websocket = new WebSocket(websocketURL);
                
                // Configure WebSocket
                if (!string.IsNullOrEmpty(authToken))
                {
                    websocket.SetCredentials("Bearer", authToken, true);
                }
                
                // Set up event handlers
                websocket.OnOpen += OnWebSocketOpen;
                websocket.OnMessage += OnWebSocketMessage;
                websocket.OnError += OnWebSocketError;
                websocket.OnClose += OnWebSocketClose;
                
                // Connect
                websocket.Connect();
            }
            catch (Exception e)
            {
                Debug.LogError($"WebSocket connection error: {e.Message}");
                OnError?.Invoke($"WebSocket connection error: {e.Message}");
                StartReconnection();
            }
        }
        
        /// <summary>
        /// Disconnect WebSocket
        /// </summary>
        private void DisconnectWebSocket()
        {
            if (websocket != null)
            {
                isConnected = false;
                websocket.Close();
                websocket = null;
            }
            
            if (reconnectCoroutine != null)
            {
                StopCoroutine(reconnectCoroutine);
                reconnectCoroutine = null;
            }
        }
        
        /// <summary>
        /// Handle WebSocket opened
        /// </summary>
        private void OnWebSocketOpen(object sender, EventArgs e)
        {
            Debug.Log("WebSocket connected");
            isConnected = true;
            reconnectAttempts = 0;
            OnConnectionStateChanged?.Invoke(true);
            
            // Send initialization message
            SendWebSocketMessage(new
            {
                type = "init",
                session_id = sessionId,
                client_type = "unity",
                platform = Application.platform.ToString()
            });
        }
        
        /// <summary>
        /// Handle WebSocket message received
        /// </summary>
        private void OnWebSocketMessage(object sender, MessageEventArgs e)
        {
            if (e.IsBinary)
            {
                Debug.LogWarning("Binary WebSocket messages not supported");
                return;
            }
            
            try
            {
                var message = JsonConvert.DeserializeObject<Dictionary<string, object>>(e.Data);
                ProcessWebSocketMessage(message);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to parse WebSocket message: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Process incoming WebSocket message
        /// </summary>
        private void ProcessWebSocketMessage(Dictionary<string, object> message)
        {
            if (!message.ContainsKey("type"))
            {
                Debug.LogWarning("WebSocket message missing type");
                return;
            }
            
            string messageType = message["type"].ToString();
            
            switch (messageType)
            {
                case "init_response":
                    HandleInitResponse(message);
                    break;
                case "chat_response":
                    HandleChatResponse(message);
                    break;
                case "consensus_update":
                    HandleConsensusUpdate(message);
                    break;
                case "error":
                    HandleErrorMessage(message);
                    break;
                case "heartbeat":
                    HandleHeartbeat();
                    break;
                default:
                    OnMessageReceived?.Invoke(JsonConvert.SerializeObject(message));
                    break;
            }
        }
        
        /// <summary>
        /// Handle WebSocket error
        /// </summary>
        private void OnWebSocketError(object sender, ErrorEventArgs e)
        {
            Debug.LogError($"WebSocket error: {e.Message}");
            OnError?.Invoke($"WebSocket error: {e.Message}");
        }
        
        /// <summary>
        /// Handle WebSocket closed
        /// </summary>
        private void OnWebSocketClose(object sender, CloseEventArgs e)
        {
            Debug.Log($"WebSocket closed: {e.Reason}");
            isConnected = false;
            OnConnectionStateChanged?.Invoke(false);
            
            if (!e.WasClean)
            {
                StartReconnection();
            }
        }
        
        /// <summary>
        /// Start reconnection process
        /// </summary>
        private void StartReconnection()
        {
            if (reconnectCoroutine == null && reconnectAttempts < maxReconnectAttempts)
            {
                reconnectCoroutine = StartCoroutine(ReconnectCoroutine());
            }
        }
        
        /// <summary>
        /// Reconnection coroutine
        /// </summary>
        private IEnumerator ReconnectCoroutine()
        {
            reconnectAttempts++;
            Debug.Log($"Reconnection attempt {reconnectAttempts}/{maxReconnectAttempts}");
            
            yield return new WaitForSeconds(reconnectDelay * reconnectAttempts);
            
            ConnectWebSocket();
            reconnectCoroutine = null;
        }
        
        #endregion
        
        #region Message Handlers
        
        private void HandleInitResponse(Dictionary<string, object> message)
        {
            if (message.ContainsKey("session_id"))
            {
                sessionId = message["session_id"].ToString();
            }
            if (message.ContainsKey("connection_id"))
            {
                connectionId = message["connection_id"].ToString();
            }
            
            Debug.Log($"Initialized with session ID: {sessionId}");
        }
        
        private void HandleChatResponse(Dictionary<string, object> message)
        {
            OnMessageReceived?.Invoke(JsonConvert.SerializeObject(message));
        }
        
        private void HandleConsensusUpdate(Dictionary<string, object> message)
        {
            // Handle consensus updates from multiple agents
            OnMessageReceived?.Invoke(JsonConvert.SerializeObject(message));
        }
        
        private void HandleErrorMessage(Dictionary<string, object> message)
        {
            string errorMsg = message.ContainsKey("message") ? message["message"].ToString() : "Unknown error";
            OnError?.Invoke(errorMsg);
        }
        
        private void HandleHeartbeat()
        {
            // Respond to heartbeat
            SendWebSocketMessage(new { type = "heartbeat_response" });
        }
        
        #endregion
        
        #region Public API
        
        /// <summary>
        /// Send chat message
        /// </summary>
        public async Task<ChatResponse> SendChatMessage(string message, Dictionary<string, object> metadata = null)
        {
            var payload = new
            {
                message = message,
                session_id = sessionId,
                metadata = metadata ?? new Dictionary<string, object>(),
                timestamp = DateTime.UtcNow.ToString("O")
            };
            
            if (isConnected && websocket != null)
            {
                // Send via WebSocket for real-time response
                SendWebSocketMessage(new
                {
                    type = "chat_message",
                    payload = payload
                });
                return null; // Response will come through WebSocket
            }
            else
            {
                // Fallback to REST API
                return await SendChatMessageREST(payload);
            }
        }
        
        /// <summary>
        /// Send chat message via REST API
        /// </summary>
        private async Task<ChatResponse> SendChatMessageREST(object payload)
        {
            var json = JsonConvert.SerializeObject(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            var request = new HttpRequestMessage(HttpMethod.Post, $"{backendURL}/api/v1/chat");
            request.Content = content;
            
            if (!string.IsNullOrEmpty(authToken))
            {
                request.Headers.Add("Authorization", $"Bearer {authToken}");
            }
            
            try
            {
                var response = await httpClient.SendAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    var responseJson = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<ChatResponse>(responseJson);
                }
                else
                {
                    throw new Exception($"HTTP error: {response.StatusCode}");
                }
            }
            catch (Exception e)
            {
                OnError?.Invoke($"REST API error: {e.Message}");
                return null;
            }
        }
        
        /// <summary>
        /// Create new session
        /// </summary>
        public async Task<string> CreateSession(SessionConfig config = null)
        {
            var payload = config ?? new SessionConfig();
            var json = JsonConvert.SerializeObject(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            var request = new HttpRequestMessage(HttpMethod.Post, $"{backendURL}/api/v1/sessions");
            request.Content = content;
            
            if (!string.IsNullOrEmpty(authToken))
            {
                request.Headers.Add("Authorization", $"Bearer {authToken}");
            }
            
            try
            {
                var response = await httpClient.SendAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    var responseJson = await response.Content.ReadAsStringAsync();
                    var sessionResponse = JsonConvert.DeserializeObject<SessionResponse>(responseJson);
                    sessionId = sessionResponse.session_id;
                    return sessionId;
                }
                else
                {
                    throw new Exception($"HTTP error: {response.StatusCode}");
                }
            }
            catch (Exception e)
            {
                OnError?.Invoke($"Session creation error: {e.Message}");
                return null;
            }
        }
        
        /// <summary>
        /// Call MCP server tool
        /// </summary>
        public async Task<object> CallMCPTool(string tool, Dictionary<string, object> parameters)
        {
            var payload = new
            {
                tool = tool,
                parameters = parameters,
                session_id = sessionId
            };
            
            if (isConnected && websocket != null)
            {
                // Send via WebSocket
                SendWebSocketMessage(new
                {
                    type = "mcp_tool_call",
                    payload = payload
                });
                return null; // Response will come through WebSocket
            }
            else
            {
                // Fallback to REST API
                return await CallMCPToolREST(payload);
            }
        }
        
        /// <summary>
        /// Call MCP tool via REST API
        /// </summary>
        private async Task<object> CallMCPToolREST(object payload)
        {
            var json = JsonConvert.SerializeObject(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            var request = new HttpRequestMessage(HttpMethod.Post, $"{backendURL}/api/v1/mcp/tools/call");
            request.Content = content;
            
            if (!string.IsNullOrEmpty(authToken))
            {
                request.Headers.Add("Authorization", $"Bearer {authToken}");
            }
            
            try
            {
                var response = await httpClient.SendAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    var responseJson = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<object>(responseJson);
                }
                else
                {
                    throw new Exception($"HTTP error: {response.StatusCode}");
                }
            }
            catch (Exception e)
            {
                OnError?.Invoke($"MCP tool call error: {e.Message}");
                return null;
            }
        }
        
        /// <summary>
        /// Send WebSocket message
        /// </summary>
        private void SendWebSocketMessage(object message)
        {
            if (websocket != null && isConnected)
            {
                var json = JsonConvert.SerializeObject(message);
                websocket.Send(json);
            }
        }
        
        /// <summary>
        /// Get connection status
        /// </summary>
        public bool IsConnected => isConnected;
        
        /// <summary>
        /// Get current session ID
        /// </summary>
        public string SessionId => sessionId;
        
        #endregion
        
        #region Data Models
        
        [Serializable]
        public class ChatResponse
        {
            public string message { get; set; }
            public string session_id { get; set; }
            public Dictionary<string, object> metadata { get; set; }
            public string timestamp { get; set; }
        }
        
        [Serializable]
        public class SessionConfig
        {
            public string agent_type { get; set; } = "digital_human";
            public Dictionary<string, object> settings { get; set; } = new Dictionary<string, object>();
        }
        
        [Serializable]
        public class SessionResponse
        {
            public string session_id { get; set; }
            public string status { get; set; }
            public Dictionary<string, object> config { get; set; }
        }
        
        #endregion
    }
}