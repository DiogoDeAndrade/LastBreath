using UnityEngine;

public class Resource : MonoBehaviour
{
    [SerializeField] private ResourceData data;

    SpriteRenderer spriteRenderer;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void SetType(ResourceData data)
    {
        this.data = data;
        spriteRenderer.sprite = data.resourceImage;
    }
}
