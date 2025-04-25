using NaughtyAttributes;
using UnityEngine;
using UC;

public class SubCustomization : MonoBehaviour
{
    [SerializeField] private ColorPalette   originalPalette;
    [SerializeField] private bool           randomizeAtStart;
    [SerializeField] private Color          hullColor = Color.magenta;
    [SerializeField] private Color          stripeColor = Color.green;
    [SerializeField] private Color          cockpitColor = Color.green;

    ColorPalette modifiedPalette;

    private void Start()
    {
        if (randomizeAtStart)
        {
            hullColor = Color.HSVToRGB(Random.Range(0.0f, 1.0f), Random.Range(0.75f, 1.0f), Random.Range(0.75f, 1.0f));
            stripeColor = Color.HSVToRGB(Random.Range(0.0f, 1.0f), Random.Range(0.25f, 0.5f), Random.Range(0.5f, 0.75f));
            cockpitColor = Color.HSVToRGB(Random.Range(0.0f, 1.0f), Random.Range(0.25f, 0.5f), Random.Range(0.5f, 0.75f));
        }

        UpdateSub();
    }

    [Button("Update Sub")]    
    void UpdateSub()
    {
        modifiedPalette = BuildPalette(originalPalette, hullColor, stripeColor, cockpitColor);

        SpriteEffect effect = GetComponent<SpriteEffect>();
        effect.SetRemap(modifiedPalette);
    }

    public void SetColors(Color hullColor, Color stripeColor, Color cockpitColor)
    {
        this.cockpitColor = cockpitColor;
        this.stripeColor = stripeColor;
        this.hullColor = hullColor;
        randomizeAtStart = false;

        UpdateSub();
    }

    public static ColorPalette BuildPalette(ColorPalette originalPalette, Color hullColor, Color stripecolor, Color cockpitColor)
    {
        var modifiedPalette = originalPalette.Clone();

        Color.RGBToHSV(hullColor, out float h, out float s, out float v);
        modifiedPalette.SetColor("Hull0", Color.HSVToRGB(h, s, v * 0.5f));
        modifiedPalette.SetColor("Hull1", Color.HSVToRGB(h, s, v * 0.75f));
        modifiedPalette.SetColor("Hull2", Color.HSVToRGB(h, s, v));

        Color.RGBToHSV(stripecolor, out h, out s, out v);
        modifiedPalette.SetColor("Stripe0", Color.HSVToRGB(h, s, v * 0.5f));
        modifiedPalette.SetColor("Stripe1", Color.HSVToRGB(h, s, v * 0.75f));
        modifiedPalette.SetColor("Stripe2", Color.HSVToRGB(h, s, v));

        Color.RGBToHSV(cockpitColor, out h, out s, out v);
        modifiedPalette.SetColor("Cockpit0", Color.HSVToRGB(h, s, v * 0.5f));
        modifiedPalette.SetColor("Cockpit1", Color.HSVToRGB(h, s, v * 0.75f));
        modifiedPalette.SetColor("Cockpit2", Color.HSVToRGB(h, s, v));

        modifiedPalette.RefreshCache();

        return modifiedPalette;
    }

    internal Color GetLightColor()
    {
        Color lightColor = hullColor;

        // Remap colors from [0..1] to [0.5..1]
        lightColor = lightColor * 0.5f + 0.5f * Color.white;
        
        // Set max component to be 1
        lightColor = lightColor / Mathf.Max(lightColor.r, lightColor.g, lightColor.b);

        lightColor.a = 1.0f;
        return lightColor;
    }

    internal Color GetTextColor()
    {
        Color lightColor = hullColor;

        // Remap colors from [0..1] to [0.5..1]
        lightColor = lightColor * 0.5f + 0.5f * Color.white;

        if (lightColor.grayscale == 0.0f) lightColor = Color.white * 0.9f;

        lightColor.a = 1.0f;
        return lightColor;
    }
}
