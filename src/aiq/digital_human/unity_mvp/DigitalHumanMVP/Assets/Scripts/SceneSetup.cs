using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SceneSetup : MonoBehaviour
{
    void Start()
    {
        Debug.Log("Digital Human MVP Scene Setup");
        SetupCamera();
        SetupLighting();
        SetupUI();
    }

    void SetupCamera()
    {
        Camera mainCamera = Camera.main;
        if (mainCamera != null)
        {
            mainCamera.transform.position = new Vector3(0, 1.6f, -2f);
            mainCamera.transform.LookAt(new Vector3(0, 1.2f, 0));
            mainCamera.fieldOfView = 60f;
        }
    }

    void SetupLighting()
    {
        // Ambient lighting
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
        RenderSettings.ambientSkyColor = new Color(0.5f, 0.7f, 0.8f);
        RenderSettings.ambientEquatorColor = new Color(0.4f, 0.5f, 0.6f);
        RenderSettings.ambientGroundColor = new Color(0.2f, 0.2f, 0.2f);

        // Main directional light
        GameObject lightGO = new GameObject("Directional Light");
        Light light = lightGO.AddComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = 1f;
        light.transform.rotation = Quaternion.Euler(45f, -30f, 0);
    }

    void SetupUI()
    {
        // Create Canvas
        GameObject canvasGO = new GameObject("UI Canvas");
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasGO.AddComponent<CanvasScaler>();
        canvasGO.AddComponent<GraphicRaycaster>();

        // Add EventSystem
        GameObject eventSystem = new GameObject("EventSystem");
        eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
        eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
    }
}
