using UnityEngine;

public class DistortionCamera : MonoBehaviour
{
    [SerializeField] 
    private LayerMask   distortionLayers;
    [SerializeField, Range(1, 5)] 
    private int         renderTargetDiv = 1;

    Camera          distortCamera;
    Camera          mainCamera;
    RenderTexture   distortionTexture;

    private int     lastScreenWidth;
    private int     lastScreenHeight;
    private int     lastRenderTargetDiv;

    private float   renderTargetScale => 1.0f / Mathf.Pow(2, renderTargetDiv - 1);

    void Start()
    {
        mainCamera = GetComponentInParent<Camera>();
        distortCamera = GetComponent<Camera>();
        if (distortCamera == null)
        {
            distortCamera = gameObject.AddComponent<Camera>();
        }
        distortCamera.CopyFrom(mainCamera);
        distortCamera.cullingMask = distortionLayers;
        distortCamera.clearFlags = CameraClearFlags.SolidColor;
        distortCamera.backgroundColor = new Color(0.5f, 0.5f, 1.0f, 1.0f);
        distortCamera.transform.localPosition = Vector3.zero;
        distortCamera.transform.localRotation = Quaternion.identity;

        UpdateRenderTexture();
    }

    private void UpdateRenderTexture()
    {
        var prevDistortTexture = distortionTexture;

        int width = Mathf.Max(1, Mathf.RoundToInt(Screen.width * renderTargetScale));
        int height = Mathf.Max(1, Mathf.RoundToInt(Screen.height * renderTargetScale));

        distortionTexture = new RenderTexture(width, height, 0, RenderTextureFormat.ARGB32);
        distortionTexture.name = "_DistortionTexture";
        distortionTexture.Create();

        if (distortCamera != null)
        {
            distortCamera.targetTexture = distortionTexture;
        }

        lastScreenWidth = Screen.width;
        lastScreenHeight = Screen.height;
        lastRenderTargetDiv = renderTargetDiv;

        if (prevDistortTexture != null)
        {
            prevDistortTexture.Release();
            DestroyImmediate(prevDistortTexture);
        }
    }

    void LateUpdate()
    {
        if ((Screen.width != lastScreenWidth) || (Screen.height != lastScreenHeight) || (renderTargetDiv != lastRenderTargetDiv))
        {
            UpdateRenderTexture();
        }

        Shader.SetGlobalTexture("_DistortionMap", distortionTexture);
    }
}
