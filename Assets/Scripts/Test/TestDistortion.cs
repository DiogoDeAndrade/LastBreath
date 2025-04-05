using UnityEngine;
using UC;

public class TestDistortion : MonoBehaviour
{
    SpriteRenderer spriteRenderer;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        ExecuteScale();
    }

    void ExecuteScale()
    { 
        transform.localScale = Vector3.zero;
        transform.Tween().Interpolate(0.0f, 1.0f, 1.0f, (currentValue) =>
        {
            transform.localScale = Vector3.one * currentValue;
            spriteRenderer.color = Color.white.ChangeAlpha(1.0f - currentValue);
        }).Done(() => ExecuteScale());
    }
}
