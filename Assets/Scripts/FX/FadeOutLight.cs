using UnityEngine;
using UnityEngine.Rendering.Universal;
using UC;

public class FadeOutLight : MonoBehaviour
{
    Light2D mainLight;

    void Start()
    {
        mainLight = GetComponent<Light2D>();

        float rangeMin = mainLight.pointLightInnerRadius;
        float rangeMax = mainLight.pointLightOuterRadius;
        mainLight.Tween().Interpolate(0.0f, 1.0f, 0.1f, (value) =>
        {
            mainLight.pointLightInnerRadius = rangeMin * value;
            mainLight.pointLightOuterRadius = rangeMax * value;
        }).EaseFunction(Ease.Sqrt);

        mainLight.FadeOut(0.25f).Done(() => mainLight.enabled = false).DelayStart(0.1f);
    }
}
