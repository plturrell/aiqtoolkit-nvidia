using System;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;

namespace AIQToolkit.DigitalHuman.MVP
{
    /// <summary>
    /// Controls Ready Player Me avatar loading and animations
    /// </summary>
    public class AvatarController : MonoBehaviour
    {
        public enum AvatarState
        {
            Idle,
            Speaking,
            Thinking,
            Listening
        }
        
        [Header("Avatar Configuration")]
        [SerializeField] private string defaultAvatarURL = "https://models.readyplayer.me/64a6af5b3e8f5f3d0e0f5f3d.glb";
        [SerializeField] private Transform avatarContainer;
        [SerializeField] private float avatarScale = 1f;
        
        [Header("Animation Settings")]
        [SerializeField] private RuntimeAnimatorController animatorController;
        [SerializeField] private float blinkInterval = 4f;
        [SerializeField] private float blinkDuration = 0.15f;
        
        [Header("Emotion Blendshapes")]
        [SerializeField] private Dictionary<string, float> emotionBlendshapes = new Dictionary<string, float>();
        
        // Events
        public event Action<GameObject> OnAvatarLoaded;
        public event Action<string> OnAvatarLoadError;
        
        private GameObject currentAvatar;
        private Animator avatarAnimator;
        private SkinnedMeshRenderer[] avatarMeshes;
        private AvatarState currentState = AvatarState.Idle;
        private Coroutine blinkCoroutine;
        
        public void LoadAvatar(string url = null)
        {
            string avatarURL = string.IsNullOrEmpty(url) ? defaultAvatarURL : url;
            StartCoroutine(LoadAvatarCoroutine(avatarURL));
        }
        
        private IEnumerator LoadAvatarCoroutine(string url)
        {
            using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
            {
                yield return webRequest.SendWebRequest();
                
                if (webRequest.result != UnityWebRequest.Result.Success)
                {
                    string error = $"Failed to load avatar: {webRequest.error}";
                    Debug.LogError(error);
                    OnAvatarLoadError?.Invoke(error);
                    yield break;
                }
                
                // For MVP, we'll use a simple GLB loader
                // In production, use ReadyPlayerMe SDK or GLTFUtility
                yield return LoadGLBModel(webRequest.downloadHandler.data);
            }
        }
        
        private IEnumerator LoadGLBModel(byte[] data)
        {
            // This is a simplified version - in production use proper GLB/GLTF loader
            // For now, we'll instantiate a placeholder or use the ReadyPlayerMe SDK
            
            // Create a placeholder avatar for MVP
            GameObject avatarPrefab = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            currentAvatar = Instantiate(avatarPrefab, avatarContainer);
            currentAvatar.transform.localScale = Vector3.one * avatarScale;
            
            // Add animator
            avatarAnimator = currentAvatar.AddComponent<Animator>();
            if (animatorController != null)
            {
                avatarAnimator.runtimeAnimatorController = animatorController;
            }
            
            // Get mesh renderers for blendshapes
            avatarMeshes = currentAvatar.GetComponentsInChildren<SkinnedMeshRenderer>();
            
            // Start blinking
            if (blinkCoroutine != null)
            {
                StopCoroutine(blinkCoroutine);
            }
            blinkCoroutine = StartCoroutine(BlinkLoop());
            
            OnAvatarLoaded?.Invoke(currentAvatar);
            yield return null;
        }
        
        public void SetAvatarState(AvatarState state)
        {
            currentState = state;
            
            if (avatarAnimator == null) return;
            
            // Trigger appropriate animations
            switch (state)
            {
                case AvatarState.Idle:
                    avatarAnimator.SetTrigger("Idle");
                    break;
                case AvatarState.Speaking:
                    avatarAnimator.SetTrigger("Speaking");
                    break;
                case AvatarState.Thinking:
                    avatarAnimator.SetTrigger("Thinking");
                    break;
                case AvatarState.Listening:
                    avatarAnimator.SetTrigger("Listening");
                    break;
            }
        }
        
        public void SetEmotion(string emotion, float intensity = 1f)
        {
            if (avatarMeshes == null || avatarMeshes.Length == 0) return;
            
            // Apply emotion blendshapes
            foreach (var mesh in avatarMeshes)
            {
                int blendShapeCount = mesh.sharedMesh.blendShapeCount;
                
                for (int i = 0; i < blendShapeCount; i++)
                {
                    string shapeName = mesh.sharedMesh.GetBlendShapeName(i);
                    
                    if (shapeName.ToLower().Contains(emotion.ToLower()))
                    {
                        mesh.SetBlendShapeWeight(i, intensity * 100f);
                    }
                }
            }
        }
        
        private IEnumerator BlinkLoop()
        {
            while (true)
            {
                yield return new WaitForSeconds(blinkInterval);
                
                // Blink animation
                if (avatarMeshes != null)
                {
                    foreach (var mesh in avatarMeshes)
                    {
                        int blinkIndex = GetBlendShapeIndex(mesh, "blink");
                        if (blinkIndex >= 0)
                        {
                            // Close eyes
                            mesh.SetBlendShapeWeight(blinkIndex, 100f);
                            yield return new WaitForSeconds(blinkDuration);
                            // Open eyes
                            mesh.SetBlendShapeWeight(blinkIndex, 0f);
                        }
                    }
                }
            }
        }
        
        private int GetBlendShapeIndex(SkinnedMeshRenderer mesh, string name)
        {
            for (int i = 0; i < mesh.sharedMesh.blendShapeCount; i++)
            {
                if (mesh.sharedMesh.GetBlendShapeName(i).ToLower().Contains(name.ToLower()))
                {
                    return i;
                }
            }
            return -1;
        }
        
        public void SetHeadRotation(Vector3 rotation)
        {
            if (currentAvatar == null) return;
            
            Transform head = FindBone("head");
            if (head != null)
            {
                head.localRotation = Quaternion.Euler(rotation);
            }
        }
        
        private Transform FindBone(string boneName)
        {
            if (currentAvatar == null) return null;
            
            Transform[] allTransforms = currentAvatar.GetComponentsInChildren<Transform>();
            foreach (Transform t in allTransforms)
            {
                if (t.name.ToLower().Contains(boneName.ToLower()))
                {
                    return t;
                }
            }
            return null;
        }
        
        private void OnDestroy()
        {
            if (blinkCoroutine != null)
            {
                StopCoroutine(blinkCoroutine);
            }
            
            if (currentAvatar != null)
            {
                Destroy(currentAvatar);
            }
        }
    }
}