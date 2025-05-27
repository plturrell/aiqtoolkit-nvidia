using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using WebSocketSharp;

namespace AIQToolkit.DigitalHuman.MVP
{
    /// <summary>
    /// Handles WebSocket connection to AIQToolkit LangChain backend
    /// </summary>
    public class BackendConnector : MonoBehaviour
    {
        [Header("Connection Settings")]
        [SerializeField] private string websocketURL = "ws://localhost:8080/ws";
        [SerializeField] private float reconnectDelay = 5f;
        [SerializeField] private int maxReconnectAttempts = 3;
        [SerializeField] private float heartbeatInterval = 30f;
        
        [Header("Debug")]
        [SerializeField] private bool enableDebugLogs = true;
        
        // Events
        public event Action<string> OnMessageReceived;
        public event Action<bool> OnConnectionStatusChanged;
        public event Action<string> OnError;
        
        private WebSocket websocket;
        private Queue<string> messageQueue = new Queue<string>();
        private bool isConnected = false;
        private int reconnectAttempts = 0;
        private Coroutine heartbeatCoroutine;
        private Coroutine reconnectCoroutine;
        
        public void Initialize(string url = null)
        {
            if (!string.IsNullOrEmpty(url))
            {
                websocketURL = url;
            }
            
            DebugLog($"Initializing WebSocket connection to: {websocketURL}");
        }
        
        public void Connect()
        {
            if (websocket != null && websocket.ReadyState == WebSocketState.Open)
            {
                DebugLog("Already connected");
                return;
            }
            
            StartCoroutine(ConnectCoroutine());
        }
        
        private IEnumerator ConnectCoroutine()
        {
            DebugLog("Attempting to connect...");
            
            try
            {
                websocket = new WebSocket(websocketURL);
                
                // Setup event handlers
                websocket.OnOpen += OnWebSocketOpen;
                websocket.OnMessage += OnWebSocketMessage;
                websocket.OnError += OnWebSocketError;
                websocket.OnClose += OnWebSocketClose;
                
                // Connect asynchronously
                websocket.ConnectAsync();
                
                // Wait for connection
                float timeout = 10f;
                float elapsed = 0f;
                
                while (websocket.ReadyState == WebSocketState.Connecting && elapsed < timeout)
                {
                    yield return new WaitForSeconds(0.1f);
                    elapsed += 0.1f;
                }
                
                if (websocket.ReadyState != WebSocketState.Open)
                {
                    throw new Exception("Connection timeout");
                }
                
                DebugLog("Connected successfully");
                isConnected = true;
                reconnectAttempts = 0;
                OnConnectionStatusChanged?.Invoke(true);
                
                // Start heartbeat
                if (heartbeatCoroutine != null)
                {
                    StopCoroutine(heartbeatCoroutine);
                }
                heartbeatCoroutine = StartCoroutine(HeartbeatCoroutine());
            }
            catch (Exception e)
            {
                DebugLog($"Connection failed: {e.Message}");
                OnError?.Invoke($"Connection failed: {e.Message}");
                
                // Try to reconnect
                if (reconnectAttempts < maxReconnectAttempts)
                {
                    reconnectAttempts++;
                    if (reconnectCoroutine != null)
                    {
                        StopCoroutine(reconnectCoroutine);
                    }
                    reconnectCoroutine = StartCoroutine(ReconnectCoroutine());
                }
            }
        }
        
        private IEnumerator ReconnectCoroutine()
        {
            DebugLog($"Reconnecting in {reconnectDelay} seconds... (Attempt {reconnectAttempts}/{maxReconnectAttempts})");
            yield return new WaitForSeconds(reconnectDelay);
            Connect();
        }
        
        private IEnumerator HeartbeatCoroutine()
        {
            while (isConnected && websocket != null && websocket.ReadyState == WebSocketState.Open)
            {
                yield return new WaitForSeconds(heartbeatInterval);
                SendHeartbeat();
            }
        }
        
        private void SendHeartbeat()
        {
            if (websocket != null && websocket.ReadyState == WebSocketState.Open)
            {
                string heartbeat = JsonUtility.ToJson(new { type = "heartbeat", timestamp = Time.time });
                websocket.SendAsync(heartbeat, null);
            }
        }
        
        public void SendMessage(string message)
        {
            if (websocket == null || websocket.ReadyState != WebSocketState.Open)
            {
                DebugLog("Cannot send message - not connected");
                OnError?.Invoke("Not connected to backend");
                return;
            }
            
            try
            {
                // Create message object
                var messageObject = new
                {
                    type = "chat",
                    content = message,
                    timestamp = DateTime.UtcNow.ToString("o"),
                    sessionId = SystemInfo.deviceUniqueIdentifier
                };
                
                string json = JsonUtility.ToJson(messageObject);
                DebugLog($"Sending message: {json}");
                
                websocket.SendAsync(json, (bool success) =>
                {
                    if (!success)
                    {
                        DebugLog("Failed to send message");
                        OnError?.Invoke("Failed to send message");
                    }
                });
            }
            catch (Exception e)
            {
                DebugLog($"Error sending message: {e.Message}");
                OnError?.Invoke($"Error sending message: {e.Message}");
            }
        }
        
        public void Disconnect()
        {
            DebugLog("Disconnecting...");
            
            isConnected = false;
            
            if (heartbeatCoroutine != null)
            {
                StopCoroutine(heartbeatCoroutine);
                heartbeatCoroutine = null;
            }
            
            if (reconnectCoroutine != null)
            {
                StopCoroutine(reconnectCoroutine);
                reconnectCoroutine = null;
            }
            
            if (websocket != null)
            {
                websocket.CloseAsync();
                websocket = null;
            }
            
            OnConnectionStatusChanged?.Invoke(false);
        }
        
        private void OnWebSocketOpen(object sender, EventArgs e)
        {
            DebugLog("WebSocket opened");
            isConnected = true;
            
            // Send initial handshake
            var handshake = new
            {
                type = "handshake",
                clientType = "unity",
                version = "1.0.0",
                capabilities = new[] { "audio", "text", "emotions" }
            };
            
            websocket.Send(JsonUtility.ToJson(handshake));
        }
        
        private void OnWebSocketMessage(object sender, MessageEventArgs e)
        {
            DebugLog($"Received message: {e.Data}");
            
            // Queue message for main thread processing
            lock (messageQueue)
            {
                messageQueue.Enqueue(e.Data);
            }
        }
        
        private void OnWebSocketError(object sender, ErrorEventArgs e)
        {
            DebugLog($"WebSocket error: {e.Message}");
            OnError?.Invoke(e.Message);
        }
        
        private void OnWebSocketClose(object sender, CloseEventArgs e)
        {
            DebugLog($"WebSocket closed: {e.Reason}");
            isConnected = false;
            OnConnectionStatusChanged?.Invoke(false);
            
            // Attempt reconnection
            if (reconnectAttempts < maxReconnectAttempts)
            {
                reconnectAttempts++;
                if (reconnectCoroutine != null)
                {
                    StopCoroutine(reconnectCoroutine);
                }
                reconnectCoroutine = StartCoroutine(ReconnectCoroutine());
            }
        }
        
        private void Update()
        {
            // Process queued messages on main thread
            lock (messageQueue)
            {
                while (messageQueue.Count > 0)
                {
                    string message = messageQueue.Dequeue();
                    ProcessMessage(message);
                }
            }
        }
        
        private void ProcessMessage(string message)
        {
            try
            {
                // Parse message type
                var baseMessage = JsonUtility.FromJson<BaseMessage>(message);
                
                switch (baseMessage.type)
                {
                    case "chat_response":
                        OnMessageReceived?.Invoke(message);
                        break;
                        
                    case "error":
                        OnError?.Invoke(baseMessage.content);
                        break;
                        
                    case "status":
                        DebugLog($"Status: {baseMessage.content}");
                        break;
                        
                    case "heartbeat_ack":
                        // Heartbeat acknowledged
                        break;
                        
                    default:
                        DebugLog($"Unknown message type: {baseMessage.type}");
                        break;
                }
            }
            catch (Exception e)
            {
                DebugLog($"Error processing message: {e.Message}");
                OnError?.Invoke($"Error processing message: {e.Message}");
            }
        }
        
        private void DebugLog(string message)
        {
            if (enableDebugLogs)
            {
                Debug.Log($"[BackendConnector] {message}");
            }
        }
        
        private void OnDestroy()
        {
            Disconnect();
        }
        
        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                Disconnect();
            }
            else
            {
                Connect();
            }
        }
        
        private void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus && Application.isMobilePlatform)
            {
                Disconnect();
            }
        }
        
        [Serializable]
        private class BaseMessage
        {
            public string type;
            public string content;
        }
    }
}