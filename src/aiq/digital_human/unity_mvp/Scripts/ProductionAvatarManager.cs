using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using ReadyPlayerMe;
using ReadyPlayerMe.Core;
using ReadyPlayerMe.AvatarLoader;

namespace AIQToolkit.DigitalHuman.Production
{
    /// <summary>
    /// Production-ready avatar manager with Ready Player Me integration
    /// Includes caching, error handling, and performance optimizations
    /// </summary>
    public class ProductionAvatarManager : MonoBehaviour
    {
        #region Events
        public UnityEvent OnAvatarLoaded = new UnityEvent();
        public UnityEvent<GameObject> OnAvatarReady = new UnityEvent<GameObject>();
        public UnityEvent<string> OnError = new UnityEvent<string>();
        public UnityEvent<float> OnLoadProgress = new UnityEvent<float>();
        #endregion

        #region Configuration
        [System.Serializable]
        public class AvatarConfig
        {
            public string defaultAvatarUrl = "https://api.readyplayer.me/v1/avatars/default.glb";
            public float loadTimeout = 30f;
            public bool enableCaching = true;
            public int maxCacheSize = 5;
            public AvatarQuality quality = AvatarQuality.High;
            public bool enableLOD = true;
            public float[] lodDistances = { 5f, 10f, 20f };
        }

        [System.Serializable]
        public enum AvatarQuality
        {
            Low,
            Medium,
            High,
            Ultra
        }

        private AvatarConfig config;
        #endregion

        #region Avatar Management
        private GameObject currentAvatar;
        private GameObject avatarContainer;
        private AvatarLoader avatarLoader;
        private Dictionary<string, GameObject> avatarCache = new Dictionary<string, GameObject>();
        private Queue<string> cacheQueue = new Queue<string>();
        private bool isLoading = false;
        
        // Animation & Expression
        private Animator avatarAnimator;
        private SkinnedMeshRenderer avatarMeshRenderer;
        private Dictionary<string, int> blendShapeIndices = new Dictionary<string, int>();
        private Dictionary<string, float> currentBlendShapeValues = new Dictionary<string, float>();
        
        // Emotions
        private Dictionary<string, EmotionPreset> emotionPresets = new Dictionary<string, EmotionPreset>();
        private string currentEmotion = "neutral";
        private Coroutine emotionTransitionCoroutine;
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            CreateAvatarContainer();
            InitializeEmotionPresets();
        }

        private void OnDestroy()
        {
            Dispose();
        }
        #endregion

        #region Initialization
        public void Initialize(AvatarConfig avatarConfig)
        {
            config = avatarConfig;
            ProductionLogger.Instance.LogInfo("Initializing avatar manager");
            
            // Initialize Ready Player Me
            InitializeAvatarLoader();
            
            // Load default avatar
            LoadAvatar(config.defaultAvatarUrl);
        }

        private void InitializeAvatarLoader()
        {
            avatarLoader = new AvatarLoader();
            
            // Configure avatar loader settings
            var avatarConfig = avatarLoader.AvatarConfig;
            avatarConfig.UseDracoCompression = true;
            avatarConfig.UseTextureSizeLimit = true;
            
            // Set quality settings
            switch (config.quality)
            {
                case AvatarQuality.Low:
                    avatarConfig.TextureSizeLimit = 256;
                    avatarConfig.MeshLod = 2;
                    break;
                case AvatarQuality.Medium:
                    avatarConfig.TextureSizeLimit = 512;
                    avatarConfig.MeshLod = 1;
                    break;
                case AvatarQuality.High:
                    avatarConfig.TextureSizeLimit = 1024;
                    avatarConfig.MeshLod = 0;
                    break;
                case AvatarQuality.Ultra:
                    avatarConfig.TextureSizeLimit = 2048;
                    avatarConfig.MeshLod = 0;
                    break;
            }
            
            // Set up event handlers
            avatarLoader.OnCompleted += OnAvatarLoadCompleted;
            avatarLoader.OnFailed += OnAvatarLoadFailed;
            avatarLoader.OnProgressChanged += OnAvatarLoadProgress;
        }

        private void CreateAvatarContainer()
        {
            avatarContainer = new GameObject("AvatarContainer");
            avatarContainer.transform.SetParent(transform);
            avatarContainer.transform.localPosition = Vector3.zero;
            avatarContainer.transform.localRotation = Quaternion.identity;
            avatarContainer.transform.localScale = Vector3.one;
        }

        private void InitializeEmotionPresets()
        {
            // Define emotion presets with blend shape configurations
            emotionPresets["neutral"] = new EmotionPreset
            {
                blendShapes = new Dictionary<string, float>
                {
                    ["eyesClosed"] = 0f,
                    ["mouthSmile"] = 0f,
                    ["browsUp"] = 0f
                }
            };
            
            emotionPresets["happy"] = new EmotionPreset
            {
                blendShapes = new Dictionary<string, float>
                {
                    ["eyesClosed"] = 0.1f,
                    ["mouthSmile"] = 0.8f,
                    ["browsUp"] = 0.2f
                }
            };
            
            emotionPresets["sad"] = new EmotionPreset
            {
                blendShapes = new Dictionary<string, float>
                {
                    ["eyesClosed"] = 0.2f,
                    ["mouthSmile"] = -0.5f,
                    ["browsUp"] = -0.3f
                }
            };
            
            emotionPresets["angry"] = new EmotionPreset
            {
                blendShapes = new Dictionary<string, float>
                {
                    ["eyesClosed"] = 0.3f,
                    ["mouthSmile"] = -0.7f,
                    ["browsUp"] = -0.5f
                }
            };
            
            emotionPresets["surprised"] = new EmotionPreset
            {
                blendShapes = new Dictionary<string, float>
                {
                    ["eyesClosed"] = -0.5f,
                    ["mouthSmile"] = 0.2f,
                    ["browsUp"] = 0.8f
                }
            };
        }
        #endregion

        #region Avatar Loading
        public void LoadAvatar(string avatarUrl)
        {
            if (isLoading)
            {
                ProductionLogger.Instance.LogWarning("Avatar load already in progress");
                return;
            }
            
            // Check cache first
            if (config.enableCaching && avatarCache.ContainsKey(avatarUrl))
            {
                ProductionLogger.Instance.LogInfo("Loading avatar from cache");
                SetActiveAvatar(avatarCache[avatarUrl]);
                return;
            }
            
            StartCoroutine(LoadAvatarCoroutine(avatarUrl));
        }

        private IEnumerator LoadAvatarCoroutine(string avatarUrl)
        {
            isLoading = true;
            ProductionLogger.Instance.LogInfo($"Loading avatar: {avatarUrl}");
            ProductionMetrics.Instance.StartAvatarLoad();
            
            // Start loading with timeout
            float startTime = Time.realtimeSinceStartup;
            bool loadCompleted = false;
            bool loadFailed = false;
            string errorMessage = "";
            
            // Set up completion handlers
            System.Action<object, CompletionEventArgs> onCompleted = (sender, args) =>
            {
                loadCompleted = true;
            };
            
            System.Action<object, FailureEventArgs> onFailed = (sender, args) =>
            {
                loadFailed = true;
                errorMessage = args.Message;
            };
            
            avatarLoader.OnCompleted += onCompleted;
            avatarLoader.OnFailed += onFailed;
            
            // Start loading
            avatarLoader.LoadAvatar(avatarUrl);
            
            // Wait for completion or timeout
            while (!loadCompleted && !loadFailed && (Time.realtimeSinceStartup - startTime) < config.loadTimeout)
            {
                yield return new WaitForSeconds(0.1f);
            }
            
            // Clean up handlers
            avatarLoader.OnCompleted -= onCompleted;
            avatarLoader.OnFailed -= onFailed;
            
            if (loadCompleted)
            {
                ProductionMetrics.Instance.EndAvatarLoad(true);
                CacheAvatar(avatarUrl, avatarLoader.Avatar);
                SetActiveAvatar(avatarLoader.Avatar);
            }
            else if (loadFailed)
            {
                ProductionMetrics.Instance.EndAvatarLoad(false);
                ProductionLogger.Instance.LogError($"Avatar load failed: {errorMessage}");
                OnError?.Invoke(errorMessage);
                
                // Try fallback avatar
                if (avatarUrl != config.defaultAvatarUrl)
                {
                    yield return LoadAvatarCoroutine(config.defaultAvatarUrl);
                }
            }
            else
            {
                ProductionMetrics.Instance.EndAvatarLoad(false);
                ProductionLogger.Instance.LogError("Avatar load timeout");
                OnError?.Invoke("Avatar load timeout");
            }
            
            isLoading = false;
        }

        private void OnAvatarLoadCompleted(object sender, CompletionEventArgs args)
        {
            ProductionLogger.Instance.LogInfo("Avatar load completed");
        }

        private void OnAvatarLoadFailed(object sender, FailureEventArgs args)
        {
            ProductionLogger.Instance.LogError($"Avatar load failed: {args.Message}");
            ProductionMetrics.Instance.LogError("Avatar", args.Message);
        }

        private void OnAvatarLoadProgress(object sender, ProgressChangeEventArgs args)
        {
            OnLoadProgress?.Invoke(args.Progress);
            ProductionLogger.Instance.LogDebug($"Avatar load progress: {args.Progress * 100}%");
        }

        private void CacheAvatar(string url, GameObject avatar)
        {
            if (!config.enableCaching)
                return;
            
            // Remove oldest cache entry if at capacity
            if (avatarCache.Count >= config.maxCacheSize)
            {
                string oldestUrl = cacheQueue.Dequeue();
                if (avatarCache.ContainsKey(oldestUrl))
                {
                    Destroy(avatarCache[oldestUrl]);
                    avatarCache.Remove(oldestUrl);
                }
            }
            
            // Add to cache
            avatarCache[url] = avatar;
            cacheQueue.Enqueue(url);
            
            // Make sure cached avatar is inactive
            avatar.SetActive(false);
        }

        private void SetActiveAvatar(GameObject avatar)
        {
            // Disable current avatar
            if (currentAvatar != null)
            {
                currentAvatar.SetActive(false);
            }
            
            // Set new avatar
            currentAvatar = avatar;
            currentAvatar.transform.SetParent(avatarContainer.transform);
            currentAvatar.transform.localPosition = Vector3.zero;
            currentAvatar.transform.localRotation = Quaternion.identity;
            currentAvatar.transform.localScale = Vector3.one;
            currentAvatar.SetActive(true);
            
            // Initialize components
            InitializeAvatarComponents();
            
            // Apply LOD if enabled
            if (config.enableLOD)
            {
                SetupLOD();
            }
            
            OnAvatarLoaded?.Invoke();
            OnAvatarReady?.Invoke(currentAvatar);
            
            ProductionLogger.Instance.LogInfo("Avatar activated successfully");
        }

        private void InitializeAvatarComponents()
        {
            // Get animator
            avatarAnimator = currentAvatar.GetComponent<Animator>();
            if (avatarAnimator == null)
            {
                avatarAnimator = currentAvatar.AddComponent<Animator>();
                ProductionLogger.Instance.LogWarning("No animator found, added default");
            }
            
            // Get mesh renderer for blend shapes
            avatarMeshRenderer = currentAvatar.GetComponentInChildren<SkinnedMeshRenderer>();
            if (avatarMeshRenderer != null)
            {
                IndexBlendShapes();
            }
            else
            {
                ProductionLogger.Instance.LogWarning("No SkinnedMeshRenderer found for blend shapes");
            }
        }

        private void IndexBlendShapes()
        {
            blendShapeIndices.Clear();
            
            if (avatarMeshRenderer == null || avatarMeshRenderer.sharedMesh == null)
                return;
            
            Mesh mesh = avatarMeshRenderer.sharedMesh;
            for (int i = 0; i < mesh.blendShapeCount; i++)
            {
                string shapeName = mesh.GetBlendShapeName(i);
                blendShapeIndices[shapeName] = i;
                currentBlendShapeValues[shapeName] = 0f;
            }
            
            ProductionLogger.Instance.LogInfo($"Indexed {blendShapeIndices.Count} blend shapes");
        }

        private void SetupLOD()
        {
            LODGroup lodGroup = currentAvatar.GetComponent<LODGroup>();
            if (lodGroup == null)
            {
                lodGroup = currentAvatar.AddComponent<LODGroup>();
            }
            
            // Set up LOD levels based on config
            LOD[] lods = new LOD[config.lodDistances.Length];
            Renderer[] renderers = currentAvatar.GetComponentsInChildren<Renderer>();
            
            for (int i = 0; i < lods.Length; i++)
            {
                float screenRelativeHeight = 1f / (config.lodDistances[i] + 1f);
                lods[i] = new LOD(screenRelativeHeight, renderers);
            }
            
            lodGroup.SetLODs(lods);
            lodGroup.RecalculateBounds();
        }
        #endregion

        #region Avatar Control
        public void SetEmotion(string emotion)
        {
            if (!emotionPresets.ContainsKey(emotion))
            {
                ProductionLogger.Instance.LogWarning($"Unknown emotion: {emotion}");
                return;
            }
            
            currentEmotion = emotion;
            
            if (emotionTransitionCoroutine != null)
            {
                StopCoroutine(emotionTransitionCoroutine);
            }
            
            emotionTransitionCoroutine = StartCoroutine(TransitionToEmotion(emotion));
        }

        private IEnumerator TransitionToEmotion(string emotion)
        {
            EmotionPreset preset = emotionPresets[emotion];
            float transitionTime = 0.5f;
            float elapsedTime = 0f;
            
            // Store starting values
            Dictionary<string, float> startValues = new Dictionary<string, float>(currentBlendShapeValues);
            
            while (elapsedTime < transitionTime)
            {
                float t = elapsedTime / transitionTime;
                t = Mathf.SmoothStep(0, 1, t); // Smooth transition
                
                foreach (var blendShape in preset.blendShapes)
                {
                    if (blendShapeIndices.ContainsKey(blendShape.Key))
                    {
                        float startValue = startValues.ContainsKey(blendShape.Key) ? startValues[blendShape.Key] : 0f;
                        float targetValue = blendShape.Value * 100f; // Convert to percentage
                        float currentValue = Mathf.Lerp(startValue, targetValue, t);
                        
                        avatarMeshRenderer.SetBlendShapeWeight(blendShapeIndices[blendShape.Key], currentValue);
                        currentBlendShapeValues[blendShape.Key] = currentValue;
                    }
                }
                
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            
            // Set final values
            foreach (var blendShape in preset.blendShapes)
            {
                if (blendShapeIndices.ContainsKey(blendShape.Key))
                {
                    float targetValue = blendShape.Value * 100f;
                    avatarMeshRenderer.SetBlendShapeWeight(blendShapeIndices[blendShape.Key], targetValue);
                    currentBlendShapeValues[blendShape.Key] = targetValue;
                }
            }
            
            ProductionLogger.Instance.LogInfo($"Emotion set to: {emotion}");
            ProductionMetrics.Instance.LogEmotionChange(emotion);
        }

        public void SetExpression(string blendShapeName, float value)
        {
            if (!blendShapeIndices.ContainsKey(blendShapeName))
            {
                ProductionLogger.Instance.LogWarning($"Blend shape not found: {blendShapeName}");
                return;
            }
            
            value = Mathf.Clamp(value, 0f, 1f) * 100f; // Convert to percentage
            avatarMeshRenderer.SetBlendShapeWeight(blendShapeIndices[blendShapeName], value);
            currentBlendShapeValues[blendShapeName] = value;
        }

        public void PlayAnimation(string animationName)
        {
            if (avatarAnimator == null)
            {
                ProductionLogger.Instance.LogWarning("No animator available");
                return;
            }
            
            avatarAnimator.Play(animationName);
            ProductionLogger.Instance.LogInfo($"Playing animation: {animationName}");
        }

        public void SetAnimationParameter(string parameterName, float value)
        {
            if (avatarAnimator == null)
                return;
            
            avatarAnimator.SetFloat(parameterName, value);
        }

        public void SetAnimationParameter(string parameterName, bool value)
        {
            if (avatarAnimator == null)
                return;
            
            avatarAnimator.SetBool(parameterName, value);
        }

        public void SetAnimationParameter(string parameterName, int value)
        {
            if (avatarAnimator == null)
                return;
            
            avatarAnimator.SetInteger(parameterName, value);
        }

        public void ResetToIdle()
        {
            SetEmotion("neutral");
            
            if (avatarAnimator != null)
            {
                avatarAnimator.SetTrigger("Idle");
            }
        }
        #endregion

        #region Blend Shape Access
        public Dictionary<string, int> GetAvatarMouthBlendShapes()
        {
            Dictionary<string, int> mouthShapes = new Dictionary<string, int>();
            
            // Common mouth-related blend shape names
            string[] mouthShapeNames = {
                "viseme_PP", "viseme_FF", "viseme_TH", "viseme_DD",
                "viseme_kk", "viseme_CH", "viseme_SS", "viseme_nn",
                "viseme_RR", "viseme_aa", "viseme_E", "viseme_I",
                "viseme_O", "viseme_U", "mouthOpen", "mouthSmile",
                "mouthFrown", "mouthPucker", "mouthLeft", "mouthRight"
            };
            
            foreach (string shapeName in mouthShapeNames)
            {
                if (blendShapeIndices.ContainsKey(shapeName))
                {
                    mouthShapes[shapeName] = blendShapeIndices[shapeName];
                }
            }
            
            ProductionLogger.Instance.LogInfo($"Found {mouthShapes.Count} mouth blend shapes");
            return mouthShapes;
        }
        #endregion

        #region Fallback & Error Handling
        public bool LoadDefaultAvatar()
        {
            try
            {
                // Create a simple cube as fallback avatar
                GameObject fallbackAvatar = GameObject.CreatePrimitive(PrimitiveType.Cube);
                fallbackAvatar.name = "FallbackAvatar";
                
                SetActiveAvatar(fallbackAvatar);
                ProductionLogger.Instance.LogWarning("Loaded fallback avatar");
                return true;
            }
            catch (Exception e)
            {
                ProductionLogger.Instance.LogError($"Failed to load fallback avatar: {e.Message}");
                return false;
            }
        }

        public void ReloadAvatar()
        {
            if (currentAvatar != null)
            {
                string currentUrl = GetCurrentAvatarUrl();
                if (!string.IsNullOrEmpty(currentUrl))
                {
                    // Clear from cache to force reload
                    if (avatarCache.ContainsKey(currentUrl))
                    {
                        Destroy(avatarCache[currentUrl]);
                        avatarCache.Remove(currentUrl);
                    }
                    
                    LoadAvatar(currentUrl);
                }
            }
        }

        private string GetCurrentAvatarUrl()
        {
            // Try to find the URL in cache
            foreach (var kvp in avatarCache)
            {
                if (kvp.Value == currentAvatar)
                    return kvp.Key;
            }
            
            return config.defaultAvatarUrl;
        }
        #endregion

        #region Cleanup
        public void Dispose()
        {
            // Stop all coroutines
            StopAllCoroutines();
            
            // Clear cache
            foreach (var avatar in avatarCache.Values)
            {
                if (avatar != null)
                    Destroy(avatar);
            }
            avatarCache.Clear();
            cacheQueue.Clear();
            
            // Destroy current avatar
            if (currentAvatar != null)
                Destroy(currentAvatar);
            
            // Destroy container
            if (avatarContainer != null)
                Destroy(avatarContainer);
            
            ProductionLogger.Instance.LogInfo("Avatar manager disposed");
        }
        #endregion

        #region Data Models
        [System.Serializable]
        private class EmotionPreset
        {
            public Dictionary<string, float> blendShapes = new Dictionary<string, float>();
        }
        #endregion
    }
}