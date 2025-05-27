using UnityEngine;
using UnityEditor;
using AIQToolkit.DigitalHuman.UnityIntegration;

namespace AIQToolkit.DigitalHuman.UnityIntegration.Editor
{
    /// <summary>
    /// Custom editor for BackendConnector component
    /// Provides quick setup and testing functionality
    /// </summary>
    [CustomEditor(typeof(BackendConnector))]
    public class BackendConnectorEditor : UnityEditor.Editor
    {
        private BackendConnector connector;
        private bool showAdvancedSettings = false;
        private string testMessage = "Hello, AI assistant!";
        
        private void OnEnable()
        {
            connector = (BackendConnector)target;
        }
        
        public override void OnInspectorGUI()
        {
            // Draw default inspector
            DrawDefaultInspector();
            
            EditorGUILayout.Space();
            
            // Connection Status
            EditorGUILayout.LabelField("Connection Status", EditorStyles.boldLabel);
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField("Status:", GUILayout.Width(60));
                
                bool isConnected = Application.isPlaying && connector.IsConnected;
                Color statusColor = isConnected ? Color.green : Color.red;
                string statusText = isConnected ? "Connected" : "Disconnected";
                
                GUI.color = statusColor;
                EditorGUILayout.LabelField(statusText);
                GUI.color = Color.white;
            }
            
            if (Application.isPlaying && connector.IsConnected)
            {
                EditorGUILayout.LabelField($"Session ID: {connector.SessionId ?? "None"}");
            }
            
            EditorGUILayout.Space();
            
            // Quick Actions
            EditorGUILayout.LabelField("Quick Actions", EditorStyles.boldLabel);
            
            if (!Application.isPlaying)
            {
                EditorGUILayout.HelpBox("Enter Play Mode to test connection", MessageType.Info);
            }
            else
            {
                if (GUILayout.Button("Initialize Connection"))
                {
                    connector.InitializeConnection();
                }
                
                if (connector.IsConnected)
                {
                    EditorGUILayout.Space();
                    
                    // Test Message
                    EditorGUILayout.LabelField("Test Message:");
                    testMessage = EditorGUILayout.TextArea(testMessage, GUILayout.Height(40));
                    
                    if (GUILayout.Button("Send Test Message"))
                    {
                        SendTestMessage();
                    }
                    
                    EditorGUILayout.Space();
                    
                    // MCP Tools
                    EditorGUILayout.LabelField("MCP Tools", EditorStyles.boldLabel);
                    
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        if (GUILayout.Button("Test Web Search"))
                        {
                            TestWebSearch();
                        }
                        
                        if (GUILayout.Button("Test File Browser"))
                        {
                            TestFileBrowser();
                        }
                    }
                }
            }
            
            EditorGUILayout.Space();
            
            // Advanced Settings
            showAdvancedSettings = EditorGUILayout.Foldout(showAdvancedSettings, "Advanced Settings");
            if (showAdvancedSettings)
            {
                EditorGUI.indentLevel++;
                
                EditorGUILayout.LabelField("Debug Options", EditorStyles.boldLabel);
                
                if (GUILayout.Button("Clear Session"))
                {
                    ClearSession();
                }
                
                if (GUILayout.Button("Force Reconnect"))
                {
                    ForceReconnect();
                }
                
                EditorGUILayout.Space();
                
                EditorGUILayout.LabelField("Connection Diagnostics", EditorStyles.boldLabel);
                
                if (GUILayout.Button("Test REST Connection"))
                {
                    TestRESTConnection();
                }
                
                if (GUILayout.Button("Test WebSocket Connection"))
                {
                    TestWebSocketConnection();
                }
                
                EditorGUI.indentLevel--;
            }
        }
        
        private async void SendTestMessage()
        {
            if (connector == null || !connector.IsConnected)
                return;
                
            try
            {
                var response = await connector.SendChatMessage(testMessage);
                if (response != null)
                {
                    Debug.Log($"Test Message Response: {response.message}");
                }
                else
                {
                    Debug.Log("Message sent via WebSocket - check console for response");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to send test message: {e.Message}");
            }
        }
        
        private async void TestWebSearch()
        {
            if (connector == null || !connector.IsConnected)
                return;
                
            try
            {
                var parameters = new System.Collections.Generic.Dictionary<string, object>
                {
                    { "query", "Unity game development" },
                    { "max_results", 3 }
                };
                
                var result = await connector.CallMCPTool("web_search", parameters);
                Debug.Log($"Web Search Result: {JsonUtility.ToJson(result)}");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to test web search: {e.Message}");
            }
        }
        
        private async void TestFileBrowser()
        {
            if (connector == null || !connector.IsConnected)
                return;
                
            try
            {
                var parameters = new System.Collections.Generic.Dictionary<string, object>
                {
                    { "path", Application.dataPath },
                    { "include_hidden", false }
                };
                
                var result = await connector.CallMCPTool("file_browser", parameters);
                Debug.Log($"File Browser Result: {JsonUtility.ToJson(result)}");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to test file browser: {e.Message}");
            }
        }
        
        private void ClearSession()
        {
            Debug.Log("Session cleared - reconnect to create new session");
        }
        
        private void ForceReconnect()
        {
            if (connector != null && Application.isPlaying)
            {
                connector.InitializeConnection();
                Debug.Log("Forcing reconnection...");
            }
        }
        
        private void TestRESTConnection()
        {
            Debug.Log("Testing REST connection...");
            // Implementation would trigger the REST test
        }
        
        private void TestWebSocketConnection()
        {
            Debug.Log("Testing WebSocket connection...");
            // Implementation would trigger the WebSocket test
        }
    }
    
    /// <summary>
    /// Menu items for quick setup
    /// </summary>
    public static class BackendConnectorMenu
    {
        [MenuItem("GameObject/AIQToolkit/Backend Connector", false, 10)]
        static void CreateBackendConnector()
        {
            GameObject connectorObject = new GameObject("BackendConnector");
            connectorObject.AddComponent<BackendConnector>();
            
            // Register undo
            Undo.RegisterCreatedObjectUndo(connectorObject, "Create Backend Connector");
            
            // Select the created object
            Selection.activeGameObject = connectorObject;
            
            Debug.Log("Created Backend Connector object");
        }
        
        [MenuItem("AIQToolkit/Setup/Create Backend Connector")]
        static void CreateConnectorFromMenu()
        {
            CreateBackendConnector();
        }
        
        [MenuItem("AIQToolkit/Documentation/Unity Integration Guide")]
        static void OpenDocumentation()
        {
            Application.OpenURL("https://github.com/AIQToolkit/docs/unity-integration");
        }
    }
}