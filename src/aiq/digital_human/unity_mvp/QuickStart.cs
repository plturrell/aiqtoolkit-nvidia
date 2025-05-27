using UnityEngine;
using UnityEditor;

namespace AIQToolkit.DigitalHuman.MVP
{
    /// <summary>
    /// Quick start helper for setting up the Digital Human MVP scene
    /// </summary>
    public class QuickStart : EditorWindow
    {
        [MenuItem("AIQToolkit/Digital Human MVP/Quick Setup")]
        public static void ShowWindow()
        {
            GetWindow<QuickStart>("Digital Human MVP Setup");
        }

        private void OnGUI()
        {
            GUILayout.Label("Digital Human MVP Quick Setup", EditorStyles.boldLabel);
            GUILayout.Space(10);

            if (GUILayout.Button("1. Create Scene Structure"))
            {
                CreateSceneStructure();
            }

            if (GUILayout.Button("2. Setup Components"))
            {
                SetupComponents();
            }

            if (GUILayout.Button("3. Create UI"))
            {
                CreateUI();
            }

            if (GUILayout.Button("4. Create Prefabs"))
            {
                CreatePrefabs();
            }

            GUILayout.Space(20);
            GUILayout.Label("One-Click Setup", EditorStyles.boldLabel);
            
            if (GUILayout.Button("Complete Setup", GUILayout.Height(30)))
            {
                CompleteSetup();
            }
        }

        private void CreateSceneStructure()
        {
            // Create managers
            GameObject managers = new GameObject("Managers");
            GameObject mvpManager = new GameObject("MVP Manager");
            GameObject audioManager = new GameObject("Audio Manager");
            mvpManager.transform.SetParent(managers.transform);
            audioManager.transform.SetParent(managers.transform);

            // Create avatar container
            GameObject avatar = new GameObject("Avatar");
            GameObject avatarContainer = new GameObject("Avatar Container");
            avatarContainer.transform.SetParent(avatar.transform);
            avatarContainer.transform.position = Vector3.zero;

            // Create lighting
            GameObject lighting = new GameObject("Lighting");
            lighting.transform.SetParent(avatar.transform);

            GameObject directionalLight = new GameObject("Directional Light");
            Light light = directionalLight.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.2f;
            light.color = new Color(1f, 0.957f, 0.839f);
            directionalLight.transform.SetParent(lighting.transform);
            directionalLight.transform.rotation = Quaternion.Euler(45f, -30f, 0f);

            // Setup camera
            Camera mainCamera = Camera.main;
            if (mainCamera != null)
            {
                mainCamera.transform.position = new Vector3(0, 1.6f, 2.5f);
                mainCamera.fieldOfView = 40f;
                mainCamera.backgroundColor = new Color(0.173f, 0.243f, 0.314f);
            }

            Debug.Log("Scene structure created successfully!");
        }

        private void SetupComponents()
        {
            // Find GameObjects
            GameObject mvpManager = GameObject.Find("MVP Manager");
            GameObject audioManager = GameObject.Find("Audio Manager");
            GameObject avatarContainer = GameObject.Find("Avatar Container");

            if (mvpManager != null)
            {
                mvpManager.AddComponent<MVPManager>();
                mvpManager.AddComponent<BackendConnector>();
            }

            if (audioManager != null)
            {
                audioManager.AddComponent<AudioSource>();
                audioManager.AddComponent<LipSyncController>();
            }

            if (avatarContainer != null)
            {
                avatarContainer.AddComponent<AvatarController>();
            }

            Debug.Log("Components setup completed!");
        }

        private void CreateUI()
        {
            // Create Canvas
            GameObject canvasGO = new GameObject("Canvas");
            Canvas canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGO.AddComponent<UnityEngine.UI.CanvasScaler>();
            canvasGO.AddComponent<UnityEngine.UI.GraphicRaycaster>();

            // Create Event System
            if (GameObject.Find("EventSystem") == null)
            {
                GameObject eventSystem = new GameObject("EventSystem");
                eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
                eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            }

            // Create Chat Panel
            GameObject chatPanel = CreateUIPanel("Chat Panel", canvasGO.transform);
            RectTransform chatRect = chatPanel.GetComponent<RectTransform>();
            chatRect.anchorMin = new Vector2(1, 0);
            chatRect.anchorMax = new Vector2(1, 0);
            chatRect.pivot = new Vector2(1, 0);
            chatRect.anchoredPosition = new Vector2(-50, 50);
            chatRect.sizeDelta = new Vector2(400, 600);

            // Add ChatUIController
            chatPanel.AddComponent<ChatUIController>();

            // Create Status Panel
            GameObject statusPanel = CreateUIPanel("Status Panel", canvasGO.transform);
            RectTransform statusRect = statusPanel.GetComponent<RectTransform>();
            statusRect.anchorMin = new Vector2(0.5f, 1);
            statusRect.anchorMax = new Vector2(0.5f, 1);
            statusRect.pivot = new Vector2(0.5f, 1);
            statusRect.anchoredPosition = new Vector2(0, -50);
            statusRect.sizeDelta = new Vector2(400, 60);

            // Create Loading Panel
            GameObject loadingPanel = CreateUIPanel("Loading Panel", canvasGO.transform);
            RectTransform loadingRect = loadingPanel.GetComponent<RectTransform>();
            loadingRect.anchorMin = Vector2.zero;
            loadingRect.anchorMax = Vector2.one;
            loadingRect.anchoredPosition = Vector2.zero;
            loadingRect.sizeDelta = Vector2.zero;
            loadingPanel.SetActive(false);

            Debug.Log("UI created successfully!");
        }

        private GameObject CreateUIPanel(string name, Transform parent)
        {
            GameObject panel = new GameObject(name);
            panel.transform.SetParent(parent);
            
            UnityEngine.UI.Image image = panel.AddComponent<UnityEngine.UI.Image>();
            image.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
            
            RectTransform rect = panel.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = Vector2.zero;
            
            return panel;
        }

        private void CreatePrefabs()
        {
            // Create prefabs folder
            string prefabPath = "Assets/Prefabs";
            if (!AssetDatabase.IsValidFolder(prefabPath))
            {
                AssetDatabase.CreateFolder("Assets", "Prefabs");
            }

            // Create message prefabs
            CreateMessagePrefab("UserMessagePrefab", true);
            CreateMessagePrefab("AIMessagePrefab", false);

            Debug.Log("Prefabs created successfully!");
        }

        private void CreateMessagePrefab(string name, bool isUserMessage)
        {
            GameObject message = new GameObject(name);
            
            // Add background
            GameObject background = new GameObject("Background");
            background.transform.SetParent(message.transform);
            UnityEngine.UI.Image bgImage = background.AddComponent<UnityEngine.UI.Image>();
            bgImage.color = isUserMessage 
                ? new Color(0.2f, 0.6f, 1f, 1f) 
                : new Color(0.9f, 0.9f, 0.9f, 1f);

            // Add text
            GameObject textGO = new GameObject("Text");
            textGO.transform.SetParent(message.transform);
            UnityEngine.UI.Text text = textGO.AddComponent<UnityEngine.UI.Text>();
            text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            text.fontSize = 14;
            text.color = isUserMessage ? Color.white : Color.black;
            text.alignment = isUserMessage ? TextAnchor.MiddleRight : TextAnchor.MiddleLeft;

            // Setup layout
            UnityEngine.UI.HorizontalLayoutGroup layout = message.AddComponent<UnityEngine.UI.HorizontalLayoutGroup>();
            layout.padding = new RectOffset(10, 10, 5, 5);
            layout.childAlignment = isUserMessage ? TextAnchor.MiddleRight : TextAnchor.MiddleLeft;

            // Save as prefab
            string path = $"Assets/Prefabs/{name}.prefab";
            PrefabUtility.SaveAsPrefabAsset(message, path);
            DestroyImmediate(message);
        }

        private void CompleteSetup()
        {
            CreateSceneStructure();
            SetupComponents();
            CreateUI();
            CreatePrefabs();
            
            // Link components
            LinkComponents();
            
            EditorUtility.DisplayDialog("Setup Complete", 
                "Digital Human MVP scene has been set up successfully!\n\n" +
                "Next steps:\n" +
                "1. Configure Ready Player Me avatar URL\n" +
                "2. Set up uLipSync viseme mappings\n" +
                "3. Start your backend server\n" +
                "4. Press Play to test", 
                "OK");
        }

        private void LinkComponents()
        {
            MVPManager mvpManager = GameObject.Find("MVP Manager")?.GetComponent<MVPManager>();
            if (mvpManager != null)
            {
                // Auto-link components
                mvpManager.avatarController = GameObject.Find("Avatar Container")?.GetComponent<AvatarController>();
                mvpManager.lipSyncController = GameObject.Find("Audio Manager")?.GetComponent<LipSyncController>();
                mvpManager.chatUIController = GameObject.Find("Chat Panel")?.GetComponent<ChatUIController>();
                mvpManager.backendConnector = mvpManager.GetComponent<BackendConnector>();
                
                // Link UI elements
                mvpManager.loadingPanel = GameObject.Find("Loading Panel");
                
                GameObject statusPanel = GameObject.Find("Status Panel");
                if (statusPanel != null)
                {
                    UnityEngine.UI.Text statusText = statusPanel.GetComponentInChildren<UnityEngine.UI.Text>();
                    if (statusText == null)
                    {
                        GameObject textGO = new GameObject("Status Text");
                        textGO.transform.SetParent(statusPanel.transform);
                        statusText = textGO.AddComponent<UnityEngine.UI.Text>();
                        statusText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
                        statusText.color = Color.white;
                        statusText.alignment = TextAnchor.MiddleCenter;
                    }
                    mvpManager.statusText = statusText;
                }
            }
            
            Debug.Log("Components linked successfully!");
        }
    }
}