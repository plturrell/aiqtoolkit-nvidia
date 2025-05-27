using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;

namespace AIQToolkit.DigitalHuman.Production
{
    /// <summary>
    /// Professional chat interface with modern UI/UX
    /// Includes accessibility features and responsive design
    /// </summary>
    public class ProductionChatUI : MonoBehaviour
    {
        #region Events
        public UnityEvent<string> OnUserMessage = new UnityEvent<string>();
        public UnityEvent OnOfflineModeEnabled = new UnityEvent();
        public UnityEvent<string> OnError = new UnityEvent<string>();
        #endregion

        #region Configuration
        [System.Serializable]
        public class UIConfig
        {
            public Color userMessageColor = new Color(0.2f, 0.5f, 1f);
            public Color aiMessageColor = new Color(0.3f, 0.8f, 0.3f);
            public Color systemMessageColor = new Color(0.8f, 0.8f, 0.8f);
            public float messageAnimationDuration = 0.3f;
            public int maxVisibleMessages = 50;
            public bool enableMessageAnimation = true;
            public bool enableTypingIndicator = true;
            public float typingIndicatorDelay = 0.5f;
            public bool enableAutoScroll = true;
            public float autoScrollSpeed = 5f;
            public Font defaultFont;
            public int defaultFontSize = 14;
        }

        private UIConfig config;
        #endregion

        #region UI Components
        [Header("UI References")]
        [SerializeField] private Canvas chatCanvas;
        [SerializeField] private RectTransform chatPanel;
        [SerializeField] private ScrollRect scrollView;
        [SerializeField] private RectTransform contentPanel;
        [SerializeField] private TMP_InputField inputField;
        [SerializeField] private Button sendButton;
        [SerializeField] private GameObject messagePrefab;
        [SerializeField] private GameObject typingIndicator;
        [SerializeField] private GameObject offlineIndicator;
        [SerializeField] private GameObject errorPanel;
        [SerializeField] private TextMeshProUGUI errorText;
        [SerializeField] private Button errorCloseButton;
        
        // Object pools
        private Queue<GameObject> messagePool = new Queue<GameObject>();
        private List<MessageEntry> activeMessages = new List<MessageEntry>();
        
        // State
        private bool isOfflineMode = false;
        private bool isTyping = false;
        private Coroutine typingCoroutine;
        private Coroutine autoScrollCoroutine;
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            ValidateComponents();
            SetupEventHandlers();
            InitializeObjectPool();
        }

        private void OnDestroy()
        {
            Dispose();
        }
        #endregion

        #region Initialization
        public void Initialize(UIConfig uiConfig)
        {
            config = uiConfig;
            ProductionLogger.Instance.LogInfo("Initializing chat UI");
            
            SetupUI();
            SetupStyles();
            
            ProductionLogger.Instance.LogInfo("Chat UI initialized successfully");
        }

        private void ValidateComponents()
        {
            if (chatCanvas == null)
            {
                GameObject canvasObj = new GameObject("ChatCanvas");
                chatCanvas = canvasObj.AddComponent<Canvas>();
                chatCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvasObj.AddComponent<CanvasScaler>();
                canvasObj.AddComponent<GraphicRaycaster>();
            }
            
            if (chatPanel == null)
            {
                chatPanel = CreatePanel("ChatPanel", chatCanvas.transform);
            }
            
            if (scrollView == null)
            {
                CreateScrollView();
            }
            
            if (inputField == null)
            {
                CreateInputField();
            }
            
            if (messagePrefab == null)
            {
                CreateMessagePrefab();
            }
            
            if (typingIndicator == null)
            {
                CreateTypingIndicator();
            }
            
            if (offlineIndicator == null)
            {
                CreateOfflineIndicator();
            }
            
            if (errorPanel == null)
            {
                CreateErrorPanel();
            }
        }

        private RectTransform CreatePanel(string name, Transform parent)
        {
            GameObject panelObj = new GameObject(name);
            panelObj.transform.SetParent(parent);
            
            RectTransform rect = panelObj.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0, 0);
            rect.anchorMax = new Vector2(1, 1);
            rect.sizeDelta = Vector2.zero;
            rect.anchoredPosition = Vector2.zero;
            
            Image image = panelObj.AddComponent<Image>();
            image.color = new Color(0.1f, 0.1f, 0.1f, 0.95f);
            
            return rect;
        }

        private void CreateScrollView()
        {
            GameObject scrollObj = new GameObject("ScrollView");
            scrollObj.transform.SetParent(chatPanel);
            
            RectTransform scrollRect = scrollObj.AddComponent<RectTransform>();
            scrollRect.anchorMin = new Vector2(0, 0.1f);
            scrollRect.anchorMax = new Vector2(1, 1);
            scrollRect.sizeDelta = Vector2.zero;
            scrollRect.anchoredPosition = Vector2.zero;
            
            scrollView = scrollObj.AddComponent<ScrollRect>();
            scrollView.horizontal = false;
            scrollView.vertical = true;
            scrollView.scrollSensitivity = 20f;
            
            // Create viewport
            GameObject viewportObj = new GameObject("Viewport");
            viewportObj.transform.SetParent(scrollObj.transform);
            
            RectTransform viewport = viewportObj.AddComponent<RectTransform>();
            viewport.anchorMin = Vector2.zero;
            viewport.anchorMax = Vector2.one;
            viewport.sizeDelta = Vector2.zero;
            viewport.anchoredPosition = Vector2.zero;
            
            Image viewportImage = viewportObj.AddComponent<Image>();
            viewportImage.color = Color.clear;
            viewportObj.AddComponent<Mask>().showMaskGraphic = false;
            
            // Create content
            GameObject contentObj = new GameObject("Content");
            contentObj.transform.SetParent(viewport);
            
            contentPanel = contentObj.AddComponent<RectTransform>();
            contentPanel.anchorMin = new Vector2(0, 1);
            contentPanel.anchorMax = new Vector2(1, 1);
            contentPanel.pivot = new Vector2(0.5f, 1);
            contentPanel.sizeDelta = new Vector2(0, 0);
            contentPanel.anchoredPosition = Vector2.zero;
            
            VerticalLayoutGroup layoutGroup = contentObj.AddComponent<VerticalLayoutGroup>();
            layoutGroup.spacing = 10f;
            layoutGroup.padding = new RectOffset(10, 10, 10, 10);
            layoutGroup.childAlignment = TextAnchor.UpperCenter;
            layoutGroup.childControlHeight = true;
            layoutGroup.childControlWidth = true;
            layoutGroup.childForceExpandHeight = false;
            layoutGroup.childForceExpandWidth = false;
            
            ContentSizeFitter sizeFitter = contentObj.AddComponent<ContentSizeFitter>();
            sizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            
            scrollView.viewport = viewport;
            scrollView.content = contentPanel;
        }

        private void CreateInputField()
        {
            GameObject inputPanel = new GameObject("InputPanel");
            inputPanel.transform.SetParent(chatPanel);
            
            RectTransform inputPanelRect = inputPanel.AddComponent<RectTransform>();
            inputPanelRect.anchorMin = new Vector2(0, 0);
            inputPanelRect.anchorMax = new Vector2(1, 0.1f);
            inputPanelRect.sizeDelta = Vector2.zero;
            inputPanelRect.anchoredPosition = Vector2.zero;
            
            HorizontalLayoutGroup layoutGroup = inputPanel.AddComponent<HorizontalLayoutGroup>();
            layoutGroup.spacing = 10f;
            layoutGroup.padding = new RectOffset(10, 10, 10, 10);
            layoutGroup.childAlignment = TextAnchor.MiddleCenter;
            layoutGroup.childControlHeight = true;
            layoutGroup.childControlWidth = true;
            layoutGroup.childForceExpandHeight = true;
            layoutGroup.childForceExpandWidth = true;
            
            // Create input field
            GameObject inputObj = new GameObject("InputField");
            inputObj.transform.SetParent(inputPanel.transform);
            
            inputField = inputObj.AddComponent<TMP_InputField>();
            inputField.textComponent = CreateTextComponent(inputObj, "Enter message...");
            
            Image inputBg = inputObj.AddComponent<Image>();
            inputBg.color = new Color(0.2f, 0.2f, 0.2f, 1f);
            
            LayoutElement inputLayout = inputObj.AddComponent<LayoutElement>();
            inputLayout.flexibleWidth = 1f;
            
            // Create send button
            GameObject buttonObj = new GameObject("SendButton");
            buttonObj.transform.SetParent(inputPanel.transform);
            
            sendButton = buttonObj.AddComponent<Button>();
            Image buttonImage = buttonObj.AddComponent<Image>();
            buttonImage.color = new Color(0.2f, 0.5f, 1f, 1f);
            
            TextMeshProUGUI buttonText = CreateTextComponent(buttonObj, "Send");
            buttonText.fontSize = 16;
            buttonText.alignment = TextAlignmentOptions.Center;
            
            LayoutElement buttonLayout = buttonObj.AddComponent<LayoutElement>();
            buttonLayout.preferredWidth = 80f;
        }

        private void CreateMessagePrefab()
        {
            messagePrefab = new GameObject("MessagePrefab");
            messagePrefab.SetActive(false);
            
            RectTransform messageRect = messagePrefab.AddComponent<RectTransform>();
            
            HorizontalLayoutGroup layoutGroup = messagePrefab.AddComponent<HorizontalLayoutGroup>();
            layoutGroup.spacing = 10f;
            layoutGroup.padding = new RectOffset(10, 10, 5, 5);
            layoutGroup.childAlignment = TextAnchor.MiddleLeft;
            layoutGroup.childControlHeight = true;
            layoutGroup.childControlWidth = false;
            layoutGroup.childForceExpandHeight = false;
            layoutGroup.childForceExpandWidth = false;
            
            // Create avatar icon
            GameObject avatarObj = new GameObject("Avatar");
            avatarObj.transform.SetParent(messagePrefab.transform);
            
            Image avatarImage = avatarObj.AddComponent<Image>();
            avatarImage.color = Color.white;
            
            LayoutElement avatarLayout = avatarObj.AddComponent<LayoutElement>();
            avatarLayout.preferredWidth = 40f;
            avatarLayout.preferredHeight = 40f;
            
            // Create message content
            GameObject contentObj = new GameObject("Content");
            contentObj.transform.SetParent(messagePrefab.transform);
            
            VerticalLayoutGroup contentLayout = contentObj.AddComponent<VerticalLayoutGroup>();
            contentLayout.spacing = 2f;
            contentLayout.childAlignment = TextAnchor.UpperLeft;
            contentLayout.childControlHeight = true;
            contentLayout.childControlWidth = true;
            contentLayout.childForceExpandHeight = false;
            contentLayout.childForceExpandWidth = false;
            
            // Create sender name
            GameObject nameObj = new GameObject("SenderName");
            nameObj.transform.SetParent(contentObj.transform);
            
            TextMeshProUGUI nameText = nameObj.AddComponent<TextMeshProUGUI>();
            nameText.fontSize = 12;
            nameText.fontWeight = FontWeight.Bold;
            
            // Create message text
            GameObject textObj = new GameObject("MessageText");
            textObj.transform.SetParent(contentObj.transform);
            
            TextMeshProUGUI messageText = textObj.AddComponent<TextMeshProUGUI>();
            messageText.fontSize = 14;
            messageText.enableWordWrapping = true;
            
            LayoutElement contentLayoutElement = contentObj.AddComponent<LayoutElement>();
            contentLayoutElement.flexibleWidth = 1f;
            
            // Create timestamp
            GameObject timestampObj = new GameObject("Timestamp");
            timestampObj.transform.SetParent(contentObj.transform);
            
            TextMeshProUGUI timestampText = timestampObj.AddComponent<TextMeshProUGUI>();
            timestampText.fontSize = 10;
            timestampText.color = new Color(0.6f, 0.6f, 0.6f, 1f);
        }

        private void CreateTypingIndicator()
        {
            typingIndicator = Instantiate(messagePrefab, contentPanel);
            typingIndicator.name = "TypingIndicator";
            typingIndicator.SetActive(false);
            
            // Customize for typing indicator
            TextMeshProUGUI messageText = typingIndicator.transform.Find("Content/MessageText").GetComponent<TextMeshProUGUI>();
            messageText.text = "AI is typing...";
            messageText.fontStyle = FontStyles.Italic;
            
            // Add animation
            StartCoroutine(AnimateTypingIndicator(messageText));
        }

        private void CreateOfflineIndicator()
        {
            offlineIndicator = new GameObject("OfflineIndicator");
            offlineIndicator.transform.SetParent(chatPanel);
            offlineIndicator.SetActive(false);
            
            RectTransform rect = offlineIndicator.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 1f);
            rect.anchorMax = new Vector2(0.5f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.sizeDelta = new Vector2(200f, 30f);
            rect.anchoredPosition = new Vector2(0, -10f);
            
            Image bg = offlineIndicator.AddComponent<Image>();
            bg.color = new Color(1f, 0.5f, 0f, 0.9f);
            
            TextMeshProUGUI text = CreateTextComponent(offlineIndicator, "OFFLINE MODE");
            text.alignment = TextAlignmentOptions.Center;
            text.fontSize = 14;
            text.fontWeight = FontWeight.Bold;
        }

        private void CreateErrorPanel()
        {
            errorPanel = new GameObject("ErrorPanel");
            errorPanel.transform.SetParent(chatCanvas.transform);
            errorPanel.SetActive(false);
            
            RectTransform rect = errorPanel.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.sizeDelta = Vector2.zero;
            rect.anchoredPosition = Vector2.zero;
            
            Image bg = errorPanel.AddComponent<Image>();
            bg.color = new Color(0, 0, 0, 0.8f);
            
            // Create error content
            GameObject contentObj = new GameObject("ErrorContent");
            contentObj.transform.SetParent(errorPanel.transform);
            
            RectTransform contentRect = contentObj.AddComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0.5f, 0.5f);
            contentRect.anchorMax = new Vector2(0.5f, 0.5f);
            contentRect.pivot = new Vector2(0.5f, 0.5f);
            contentRect.sizeDelta = new Vector2(400f, 200f);
            contentRect.anchoredPosition = Vector2.zero;
            
            Image contentBg = contentObj.AddComponent<Image>();
            contentBg.color = new Color(0.8f, 0.2f, 0.2f, 0.95f);
            
            // Error text
            errorText = CreateTextComponent(contentObj, "Error");
            errorText.alignment = TextAlignmentOptions.Center;
            errorText.fontSize = 16;
            
            // Close button
            GameObject closeButtonObj = new GameObject("CloseButton");
            closeButtonObj.transform.SetParent(contentObj.transform);
            
            RectTransform closeRect = closeButtonObj.AddComponent<RectTransform>();
            closeRect.anchorMin = new Vector2(0.5f, 0);
            closeRect.anchorMax = new Vector2(0.5f, 0);
            closeRect.pivot = new Vector2(0.5f, 0);
            closeRect.sizeDelta = new Vector2(100f, 40f);
            closeRect.anchoredPosition = new Vector2(0, 20f);
            
            errorCloseButton = closeButtonObj.AddComponent<Button>();
            Image closeImage = closeButtonObj.AddComponent<Image>();
            closeImage.color = new Color(0.2f, 0.2f, 0.2f, 1f);
            
            TextMeshProUGUI closeText = CreateTextComponent(closeButtonObj, "Close");
            closeText.alignment = TextAlignmentOptions.Center;
        }

        private TextMeshProUGUI CreateTextComponent(GameObject parent, string defaultText)
        {
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(parent.transform);
            
            RectTransform rect = textObj.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.sizeDelta = Vector2.zero;
            rect.anchoredPosition = Vector2.zero;
            
            TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
            text.text = defaultText;
            text.color = Color.white;
            text.fontSize = 14;
            
            return text;
        }

        private void InitializeObjectPool()
        {
            for (int i = 0; i < 20; i++)
            {
                GameObject messageObj = Instantiate(messagePrefab, contentPanel);
                messageObj.SetActive(false);
                messagePool.Enqueue(messageObj);
            }
        }
        #endregion

        #region UI Setup
        private void SetupUI()
        {
            // Set proper anchoring for responsive design
            chatPanel.anchorMin = new Vector2(0, 0);
            chatPanel.anchorMax = new Vector2(1, 1);
            chatPanel.offsetMin = new Vector2(20, 20);
            chatPanel.offsetMax = new Vector2(-20, -20);
            
            // Configure input field
            inputField.characterLimit = 500;
            inputField.lineType = TMP_InputField.LineType.MultiLineNewline;
            inputField.placeholder = CreateTextComponent(inputField.gameObject, "Type your message here...");
            
            // Set up auto-scroll
            if (config.enableAutoScroll)
            {
                autoScrollCoroutine = StartCoroutine(AutoScrollCoroutine());
            }
        }

        private void SetupStyles()
        {
            // Apply configuration styles
            if (config.defaultFont != null)
            {
                UpdateFontRecursive(chatCanvas.transform, config.defaultFont);
            }
            
            UpdateFontSizeRecursive(chatCanvas.transform, config.defaultFontSize);
        }

        private void UpdateFontRecursive(Transform root, Font font)
        {
            Text[] texts = root.GetComponentsInChildren<Text>(true);
            foreach (var text in texts)
            {
                text.font = font;
            }
            
            TextMeshProUGUI[] tmpTexts = root.GetComponentsInChildren<TextMeshProUGUI>(true);
            foreach (var tmpText in tmpTexts)
            {
                // Convert Unity font to TMP font asset
                // In production, you'd have TMP font assets pre-created
            }
        }

        private void UpdateFontSizeRecursive(Transform root, int fontSize)
        {
            Text[] texts = root.GetComponentsInChildren<Text>(true);
            foreach (var text in texts)
            {
                text.fontSize = fontSize;
            }
            
            TextMeshProUGUI[] tmpTexts = root.GetComponentsInChildren<TextMeshProUGUI>(true);
            foreach (var tmpText in tmpTexts)
            {
                if (tmpText.gameObject.name != "Timestamp") // Preserve timestamp size
                {
                    tmpText.fontSize = fontSize;
                }
            }
        }

        private void SetupEventHandlers()
        {
            // Send button
            sendButton.onClick.AddListener(OnSendButtonClicked);
            
            // Input field
            inputField.onSubmit.AddListener(OnInputFieldSubmit);
            inputField.onValueChanged.AddListener(OnInputFieldChanged);
            
            // Error panel
            errorCloseButton.onClick.AddListener(() => errorPanel.SetActive(false));
            
            // Scroll view
            scrollView.onValueChanged.AddListener(OnScrollValueChanged);
        }
        #endregion

        #region Message Management
        public void AddMessage(string content, bool isUser)
        {
            StartCoroutine(AddMessageCoroutine(content, isUser));
        }

        private IEnumerator AddMessageCoroutine(string content, bool isUser)
        {
            // Get or create message object
            GameObject messageObj = GetMessageObject();
            
            // Configure message
            MessageEntry entry = new MessageEntry
            {
                gameObject = messageObj,
                content = content,
                isUser = isUser,
                timestamp = DateTime.Now
            };
            
            ConfigureMessage(entry);
            
            // Add to active messages
            activeMessages.Add(entry);
            
            // Show message with animation
            messageObj.SetActive(true);
            
            if (config.enableMessageAnimation)
            {
                yield return AnimateMessageAppear(messageObj);
            }
            
            // Manage message count
            while (activeMessages.Count > config.maxVisibleMessages)
            {
                RemoveOldestMessage();
            }
            
            // Scroll to bottom
            if (config.enableAutoScroll)
            {
                ScrollToBottom();
            }
            
            ProductionMetrics.Instance.LogMessageAdded(isUser);
        }

        private GameObject GetMessageObject()
        {
            if (messagePool.Count > 0)
            {
                return messagePool.Dequeue();
            }
            else
            {
                return Instantiate(messagePrefab, contentPanel);
            }
        }

        private void ConfigureMessage(MessageEntry entry)
        {
            // Get components
            Image avatar = entry.gameObject.transform.Find("Avatar").GetComponent<Image>();
            TextMeshProUGUI senderName = entry.gameObject.transform.Find("Content/SenderName").GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI messageText = entry.gameObject.transform.Find("Content/MessageText").GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI timestamp = entry.gameObject.transform.Find("Content/Timestamp").GetComponent<TextMeshProUGUI>();
            
            // Set content
            messageText.text = entry.content;
            timestamp.text = entry.timestamp.ToString("HH:mm:ss");
            
            // Configure based on sender
            if (entry.isUser)
            {
                senderName.text = "You";
                senderName.color = config.userMessageColor;
                avatar.color = config.userMessageColor;
                
                // Align to right
                HorizontalLayoutGroup layout = entry.gameObject.GetComponent<HorizontalLayoutGroup>();
                layout.childAlignment = TextAnchor.MiddleRight;
                layout.reverseArrangement = true;
            }
            else
            {
                senderName.text = "AI Assistant";
                senderName.color = config.aiMessageColor;
                avatar.color = config.aiMessageColor;
                
                // Align to left
                HorizontalLayoutGroup layout = entry.gameObject.GetComponent<HorizontalLayoutGroup>();
                layout.childAlignment = TextAnchor.MiddleLeft;
                layout.reverseArrangement = false;
            }
        }

        private IEnumerator AnimateMessageAppear(GameObject messageObj)
        {
            CanvasGroup canvasGroup = messageObj.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = messageObj.AddComponent<CanvasGroup>();
            }
            
            // Fade in
            canvasGroup.alpha = 0f;
            
            float elapsedTime = 0f;
            while (elapsedTime < config.messageAnimationDuration)
            {
                canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsedTime / config.messageAnimationDuration);
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            
            canvasGroup.alpha = 1f;
        }

        private void RemoveOldestMessage()
        {
            if (activeMessages.Count == 0)
                return;
            
            MessageEntry oldestMessage = activeMessages[0];
            activeMessages.RemoveAt(0);
            
            oldestMessage.gameObject.SetActive(false);
            messagePool.Enqueue(oldestMessage.gameObject);
        }

        public void ShowTypingIndicator()
        {
            if (!config.enableTypingIndicator || isTyping)
                return;
            
            isTyping = true;
            typingIndicator.SetActive(true);
            
            if (typingCoroutine != null)
                StopCoroutine(typingCoroutine);
            
            typingCoroutine = StartCoroutine(HideTypingIndicatorAfterDelay());
        }

        public void HideTypingIndicator()
        {
            isTyping = false;
            typingIndicator.SetActive(false);
            
            if (typingCoroutine != null)
            {
                StopCoroutine(typingCoroutine);
                typingCoroutine = null;
            }
        }

        private IEnumerator HideTypingIndicatorAfterDelay()
        {
            yield return new WaitForSeconds(config.typingIndicatorDelay);
            HideTypingIndicator();
        }

        private IEnumerator AnimateTypingIndicator(TextMeshProUGUI text)
        {
            string baseText = "AI is typing";
            string[] dots = { "", ".", "..", "..." };
            int index = 0;
            
            while (true)
            {
                text.text = baseText + dots[index];
                index = (index + 1) % dots.Length;
                yield return new WaitForSeconds(0.5f);
            }
        }
        #endregion

        #region Event Handlers
        private void OnSendButtonClicked()
        {
            SendMessage();
        }

        private void OnInputFieldSubmit(string text)
        {
            if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
            {
                // Shift+Enter for new line
                return;
            }
            
            SendMessage();
        }

        private void OnInputFieldChanged(string text)
        {
            // Enable/disable send button based on input
            sendButton.interactable = !string.IsNullOrWhiteSpace(text);
        }

        private void OnScrollValueChanged(Vector2 scrollPosition)
        {
            // Disable auto-scroll if user manually scrolls up
            if (scrollPosition.y > 0.01f)
            {
                config.enableAutoScroll = false;
            }
            else
            {
                config.enableAutoScroll = true;
            }
        }

        private void SendMessage()
        {
            string message = inputField.text.Trim();
            if (string.IsNullOrEmpty(message))
                return;
            
            // Add user message to UI
            AddMessage(message, true);
            
            // Clear input field
            inputField.text = "";
            inputField.ActivateInputField();
            
            // Show typing indicator
            ShowTypingIndicator();
            
            // Send to backend
            OnUserMessage?.Invoke(message);
            
            ProductionLogger.Instance.LogInfo($"User message sent: {message}");
        }
        #endregion

        #region Scrolling
        private void ScrollToBottom()
        {
            Canvas.ForceUpdateCanvases();
            scrollView.verticalNormalizedPosition = 0f;
        }

        private IEnumerator AutoScrollCoroutine()
        {
            while (true)
            {
                if (config.enableAutoScroll)
                {
                    float currentScroll = scrollView.verticalNormalizedPosition;
                    if (currentScroll > 0.01f)
                    {
                        float targetScroll = Mathf.MoveTowards(currentScroll, 0f, config.autoScrollSpeed * Time.deltaTime);
                        scrollView.verticalNormalizedPosition = targetScroll;
                    }
                }
                
                yield return null;
            }
        }
        #endregion

        #region Offline Mode
        public void EnableOfflineMode()
        {
            isOfflineMode = true;
            offlineIndicator.SetActive(true);
            
            AddMessage("System is now in offline mode. Limited functionality available.", false);
            
            OnOfflineModeEnabled?.Invoke();
            ProductionLogger.Instance.LogWarning("Offline mode enabled");
        }

        public void DisableOfflineMode()
        {
            isOfflineMode = false;
            offlineIndicator.SetActive(false);
            
            AddMessage("System is back online.", false);
            ProductionLogger.Instance.LogInfo("Offline mode disabled");
        }
        #endregion

        #region Error Handling
        public void ShowError(string message)
        {
            errorText.text = message;
            errorPanel.SetActive(true);
            
            OnError?.Invoke(message);
            ProductionLogger.Instance.LogError($"UI Error: {message}");
        }

        public void ShowCriticalError(string message)
        {
            ShowError(message);
            
            // Disable input in critical error state
            inputField.interactable = false;
            sendButton.interactable = false;
        }
        #endregion

        #region Cleanup
        public void Dispose()
        {
            // Stop all coroutines
            StopAllCoroutines();
            
            // Clear messages
            foreach (var message in activeMessages)
            {
                if (message.gameObject != null)
                    Destroy(message.gameObject);
            }
            activeMessages.Clear();
            
            // Clear pool
            while (messagePool.Count > 0)
            {
                GameObject obj = messagePool.Dequeue();
                if (obj != null)
                    Destroy(obj);
            }
            
            // Clean up UI
            if (chatCanvas != null)
                Destroy(chatCanvas.gameObject);
            
            ProductionLogger.Instance.LogInfo("Chat UI disposed");
        }
        #endregion

        #region Data Models
        [System.Serializable]
        private class MessageEntry
        {
            public GameObject gameObject;
            public string content;
            public bool isUser;
            public DateTime timestamp;
        }
        #endregion
    }
}