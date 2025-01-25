using UnityEngine;
public class Resource : MonoBehaviour
{
    [SerializeField] private ResourceData _data;

    SpriteRenderer  spriteRenderer;
    Collider2D      mainCollider;
    Rigidbody2D     rb;
    ResourceNode    resourceNode;
    public ResourceData data => _data;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        mainCollider = GetComponent<Collider2D>();
        rb = GetComponent<Rigidbody2D>();
    }

    public void SetType(ResourceData data)
    {
        this._data = data;
        spriteRenderer.sprite = data.resourceImage;
    }

    public void SetNode(ResourceNode node)
    {
        resourceNode = node;
        transform.localScale = Vector3.zero;
    }

    public void Grab()
    {
        mainCollider.enabled = false;
        rb.bodyType = RigidbodyType2D.Kinematic;

        if (resourceNode)
        {
            resourceNode.Release();
            resourceNode = null;
            transform.SetParent(null);
            transform.localScale = Vector3.one;
        }
    }

    public void RandomThrow()
    {
        mainCollider.enabled = true;
        mainCollider.isTrigger = false;
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.linearVelocity = Random.insideUnitCircle * Random.Range(100.0f, 200.0f);        
    }
}
