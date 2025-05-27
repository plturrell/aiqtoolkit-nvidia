using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using WebSocketSharp;

namespace AIQToolkit.DigitalHuman.Production
{
    /// <summary>
    /// Robust WebSocket and REST client for backend communication
    /// Includes retry logic, connection pooling, and error handling
    /// </summary>
    public class ProductionBackendConnector : MonoBehaviour
    {
        #region Events
        public UnityEvent OnConnectionEstablished = new UnityEvent();
        public UnityEvent OnConnectionLost = new UnityEvent();
        public UnityEvent<string> OnMessageReceived = new UnityEvent<string>();
        public UnityEvent<AudioClip> OnAudioReceived = new UnityEvent<AudioClip>();
        public UnityEvent<float> OnStreamingProgressUpdate = new UnityEvent<float>();
        public UnityEvent<string> OnError = new UnityEvent<string>();
        #endregion

        #region Configuration
        [System.Serializable]
        public class BackendConfig
        {
            public string webSocketUrl = "ws://localhost:8080/ws";
            public string restApiUrl = "http://localhost:8080/api";
            public float connectionTimeout = 10f;
            public float reconnectDelay = 5f;
            public int maxReconnectAttempts = 5;
            public bool enableHeartbeat = true;
            public float heartbeatInterval = 30f;
            public Dictionary<string, string> headers = new Dictionary<string, string>();
        }

        private BackendConfig config;
        #endregion

        #region Connection Management
        private WebSocket webSocket;
        private bool isConnected = false;
        private bool isReconnecting = false;
        private int reconnectAttempts = 0;
        private Coroutine heartbeatCoroutine;
        private Queue<string> messageQueue = new Queue<string>();
        private Dictionary<string, Action<string>> pendingRequests = new Dictionary<string, Action<string>>();
        
        public bool IsConnected => isConnected;
        #endregion

        #region Unity Lifecycle
        private void OnDestroy()
        {
            Disconnect();
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                Pause();
            }
            else
            {
                Resume();
            }
        }
        #endregion

        #region Initialization
        public void Initialize(BackendConfig backendConfig)
        {
            config = backendConfig;
            ProductionLogger.Instance.LogInfo("Initializing backend connector");
            Connect();
        }

        private void Connect()
        {
            if (isConnected || isReconnecting)
                return;

            StartCoroutine(ConnectAsync());
        }

        private IEnumerator ConnectAsync()
        {
            ProductionLogger.Instance.LogInfo($"Connecting to WebSocket: {config.webSocketUrl}");
            
            webSocket = new WebSocket(config.webSocketUrl);
            
            // Add headers if any
            foreach (var header in config.headers)
            {
                webSocket.SetCustomHeader(header.Key, header.Value);
            }
            
            // Setup event handlers
            webSocket.OnOpen += OnWebSocketOpen;
            webSocket.OnMessage += OnWebSocketMessage;
            webSocket.OnError += OnWebSocketError;
            webSocket.OnClose += OnWebSocketClose;
            
            // Attempt connection
            webSocket.ConnectAsync();
            
            // Wait for connection with timeout
            float elapsedTime = 0;
            while (!isConnected && elapsedTime < config.connectionTimeout)
            {
                yield return new WaitForSeconds(0.1f);
                elapsedTime += 0.1f;
            }
            
            if (!isConnected)
            {
                ProductionLogger.Instance.LogError("WebSocket connection timeout");
                HandleConnectionFailure();
            }
        }
        #endregion

        #region WebSocket Event Handlers
        private void OnWebSocketOpen(object sender, EventArgs e)
        {
            ProductionLogger.Instance.LogInfo("WebSocket connected");
            isConnected = true;
            reconnectAttempts = 0;
            
            OnConnectionEstablished?.Invoke();
            
            // Start heartbeat
            if (config.enableHeartbeat)
            {
                heartbeatCoroutine = StartCoroutine(HeartbeatCoroutine());
            }
            
            // Send queued messages
            ProcessMessageQueue();
        }

        private void OnWebSocketMessage(object sender, MessageEventArgs e)
        {
            try
            {
                if (e.IsText)
                {
                    ProcessTextMessage(e.Data);
                }
                else if (e.IsBinary)
                {
                    ProcessBinaryMessage(e.RawData);
                }
            }
            catch (Exception ex)
            {
                ProductionLogger.Instance.LogError($"Error processing message: {ex.Message}");
                OnError?.Invoke($"Message processing error: {ex.Message}");
            }
        }

        private void OnWebSocketError(object sender, ErrorEventArgs e)
        {
            ProductionLogger.Instance.LogError($"WebSocket error: {e.Message}");
            OnError?.Invoke(e.Message);
            
            if (e.Message.Contains("connection"))
            {
                HandleConnectionFailure();
            }
        }

        private void OnWebSocketClose(object sender, CloseEventArgs e)
        {
            ProductionLogger.Instance.LogWarning($"WebSocket closed: {e.Reason}");
            isConnected = false;
            
            if (heartbeatCoroutine != null)
            {
                StopCoroutine(heartbeatCoroutine);
                heartbeatCoroutine = null;
            }
            
            OnConnectionLost?.Invoke();
            
            // Attempt reconnection if not intentional close
            if (!e.WasClean && reconnectAttempts < config.maxReconnectAttempts)
            {
                StartCoroutine(ReconnectCoroutine());
            }
        }
        #endregion

        #region Message Processing
        private void ProcessTextMessage(string data)
        {
            try
            {
                var message = JsonUtility.FromJson<BackendMessage>(data);
                
                switch (message.type)
                {
                    case "response":
                        OnMessageReceived?.Invoke(message.content);
                        break;
                        
                    case "audio":
                        StartCoroutine(ProcessAudioData(message.content));
                        break;
                        
                    case "status":
                        ProcessStatusUpdate(message);
                        break;
                        
                    case "error":
                        OnError?.Invoke(message.content);
                        break;
                        
                    case "heartbeat":
                        // Heartbeat response received
                        break;
                        
                    default:
                        ProductionLogger.Instance.LogWarning($"Unknown message type: {message.type}");
                        break;
                }
                
                // Handle request callbacks
                if (!string.IsNullOrEmpty(message.requestId) && pendingRequests.ContainsKey(message.requestId))
                {
                    pendingRequests[message.requestId]?.Invoke(message.content);
                    pendingRequests.Remove(message.requestId);
                }
            }
            catch (Exception e)
            {
                ProductionLogger.Instance.LogError($"Error parsing message: {e.Message}");
            }
        }

        private void ProcessBinaryMessage(byte[] data)
        {
            // Process binary data (e.g., audio streams)
            ProductionLogger.Instance.LogInfo($"Received binary data: {data.Length} bytes");
            
            // Convert to AudioClip if it's audio data
            StartCoroutine(ConvertToAudioClip(data));
        }

        private IEnumerator ProcessAudioData(string audioDataBase64)
        {
            try
            {
                byte[] audioBytes = Convert.FromBase64String(audioDataBase64);
                yield return ConvertToAudioClip(audioBytes);
            }
            catch (Exception e)
            {
                ProductionLogger.Instance.LogError($"Error processing audio data: {e.Message}");
                OnError?.Invoke($"Audio processing error: {e.Message}");
            }
        }

        private IEnumerator ConvertToAudioClip(byte[] audioData)
        {
            // This is a simplified version - in production, use proper audio decoding
            // You might want to use NAudio or similar library for proper audio handling
            
            ProductionLogger.Instance.LogInfo("Converting audio data to AudioClip");
            
            // Create a temporary file
            string tempPath = System.IO.Path.Combine(Application.temporaryCachePath, "temp_audio.wav");
            System.IO.File.WriteAllBytes(tempPath, audioData);
            
            // Load as AudioClip
            using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip("file://" + tempPath, AudioType.WAV))
            {
                yield return www.SendWebRequest();
                
                if (www.result == UnityWebRequest.Result.Success)
                {
                    AudioClip audioClip = DownloadHandlerAudioClip.GetContent(www);
                    OnAudioReceived?.Invoke(audioClip);
                    ProductionMetrics.Instance.LogAudioReceived(audioClip.length);
                }
                else
                {
                    ProductionLogger.Instance.LogError($"Error loading audio: {www.error}");
                    OnError?.Invoke($"Audio loading error: {www.error}");
                }
            }
            
            // Clean up temporary file
            if (System.IO.File.Exists(tempPath))
            {
                System.IO.File.Delete(tempPath);
            }
        }

        private void ProcessStatusUpdate(BackendMessage message)
        {
            try
            {
                var status = JsonUtility.FromJson<StatusUpdate>(message.content);
                
                if (status.isStreaming)
                {
                    OnStreamingProgressUpdate?.Invoke(status.progress);
                }
                
                ProductionMetrics.Instance.LogBackendStatus(status);
            }
            catch (Exception e)
            {
                ProductionLogger.Instance.LogError($"Error processing status update: {e.Message}");
            }
        }
        #endregion

        #region Sending Messages
        public void SendMessage(string message)
        {
            var requestMessage = new BackendMessage
            {
                type = "user_message",
                content = message,
                requestId = Guid.NewGuid().ToString(),
                timestamp = DateTime.UtcNow.ToString("O")
            };
            
            SendJson(requestMessage);
            ProductionMetrics.Instance.LogMessageSent();
        }

        public void SendCommand(string command, Dictionary<string, object> parameters = null)
        {
            var commandMessage = new BackendMessage
            {
                type = "command",
                content = command,
                requestId = Guid.NewGuid().ToString(),
                timestamp = DateTime.UtcNow.ToString("O"),
                metadata = parameters
            };
            
            SendJson(commandMessage);
        }

        private void SendJson(object obj)
        {
            string json = JsonUtility.ToJson(obj);
            
            if (isConnected && webSocket != null && webSocket.ReadyState == WebSocketState.Open)
            {
                webSocket.Send(json);
                ProductionLogger.Instance.LogDebug($"Sent message: {json}");
            }
            else
            {
                ProductionLogger.Instance.LogWarning("WebSocket not connected, queuing message");
                messageQueue.Enqueue(json);
            }
        }

        private void ProcessMessageQueue()
        {
            while (messageQueue.Count > 0 && isConnected)
            {
                string message = messageQueue.Dequeue();
                webSocket.Send(message);
                ProductionLogger.Instance.LogDebug($"Sent queued message: {message}");
            }
        }
        #endregion

        #region REST API Methods
        public IEnumerator Get(string endpoint, Action<string> onSuccess, Action<string> onError)
        {
            string url = $"{config.restApiUrl}/{endpoint}";
            
            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                // Add headers
                foreach (var header in config.headers)
                {
                    request.SetRequestHeader(header.Key, header.Value);
                }
                
                yield return request.SendWebRequest();
                
                if (request.result == UnityWebRequest.Result.Success)
                {
                    onSuccess?.Invoke(request.downloadHandler.text);
                    ProductionMetrics.Instance.LogApiRequest("GET", endpoint, true);
                }
                else
                {
                    string error = $"GET {endpoint} failed: {request.error}";
                    ProductionLogger.Instance.LogError(error);
                    onError?.Invoke(error);
                    ProductionMetrics.Instance.LogApiRequest("GET", endpoint, false);
                }
            }
        }

        public IEnumerator Post(string endpoint, string jsonData, Action<string> onSuccess, Action<string> onError)
        {
            string url = $"{config.restApiUrl}/{endpoint}";
            
            using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
            {
                byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");
                
                // Add headers
                foreach (var header in config.headers)
                {
                    request.SetRequestHeader(header.Key, header.Value);
                }
                
                yield return request.SendWebRequest();
                
                if (request.result == UnityWebRequest.Result.Success)
                {
                    onSuccess?.Invoke(request.downloadHandler.text);
                    ProductionMetrics.Instance.LogApiRequest("POST", endpoint, true);
                }
                else
                {
                    string error = $"POST {endpoint} failed: {request.error}";
                    ProductionLogger.Instance.LogError(error);
                    onError?.Invoke(error);
                    ProductionMetrics.Instance.LogApiRequest("POST", endpoint, false);
                }
            }
        }
        #endregion

        #region Connection Management
        public void Reconnect()
        {
            if (isReconnecting)
                return;
                
            StartCoroutine(ReconnectCoroutine());
        }

        private IEnumerator ReconnectCoroutine()
        {
            isReconnecting = true;
            reconnectAttempts++;
            
            ProductionLogger.Instance.LogInfo($"Attempting reconnection ({reconnectAttempts}/{config.maxReconnectAttempts})");
            
            yield return new WaitForSeconds(config.reconnectDelay);
            
            Connect();
            
            isReconnecting = false;
        }

        private void HandleConnectionFailure()
        {
            if (reconnectAttempts < config.maxReconnectAttempts)
            {
                StartCoroutine(ReconnectCoroutine());
            }
            else
            {
                ProductionLogger.Instance.LogError("Max reconnection attempts reached");
                OnError?.Invoke("Failed to establish connection after maximum attempts");
            }
        }

        private IEnumerator HeartbeatCoroutine()
        {
            while (isConnected)
            {
                yield return new WaitForSeconds(config.heartbeatInterval);
                
                if (isConnected && webSocket.ReadyState == WebSocketState.Open)
                {
                    var heartbeat = new BackendMessage
                    {
                        type = "heartbeat",
                        timestamp = DateTime.UtcNow.ToString("O")
                    };
                    
                    SendJson(heartbeat);
                    ProductionLogger.Instance.LogDebug("Heartbeat sent");
                }
            }
        }

        public void Pause()
        {
            ProductionLogger.Instance.LogInfo("Pausing backend connection");
            
            if (webSocket != null && webSocket.ReadyState == WebSocketState.Open)
            {
                SendCommand("pause");
            }
        }

        public void Resume()
        {
            ProductionLogger.Instance.LogInfo("Resuming backend connection");
            
            if (webSocket != null && webSocket.ReadyState == WebSocketState.Open)
            {
                SendCommand("resume");
            }
            else
            {
                Connect();
            }
        }

        public void Disconnect()
        {
            ProductionLogger.Instance.LogInfo("Disconnecting from backend");
            
            isConnected = false;
            
            if (heartbeatCoroutine != null)
            {
                StopCoroutine(heartbeatCoroutine);
                heartbeatCoroutine = null;
            }
            
            if (webSocket != null)
            {
                webSocket.Close();
                webSocket = null;
            }
        }
        #endregion

        #region Data Models
        [System.Serializable]
        private class BackendMessage
        {
            public string type;
            public string content;
            public string requestId;
            public string timestamp;
            public Dictionary<string, object> metadata;
        }

        [System.Serializable]
        private class StatusUpdate
        {
            public bool isStreaming;
            public float progress;
            public string status;
            public Dictionary<string, float> metrics;
        }
        #endregion
    }
}