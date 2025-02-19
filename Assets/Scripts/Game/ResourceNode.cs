using NaughtyAttributes;
using UnityEngine;

public class ResourceNode : MonoBehaviour
{
    [SerializeField] 
    private bool                unlimitedCharges = true;
    [SerializeField, HideIf(nameof(unlimitedCharges))] 
    private int                 nCharges = 4;
    [SerializeField]
    private float               chargeTime = 10.0f;
    [SerializeField]
    private ResourceDataProbList    allowedTypes;
    [SerializeField]
    private bool                randomizeOnEachSpawn;
    [SerializeField]
    private Resource            resourcePrefab;
    [SerializeField]
    private Transform           spawnPoint;

    private float                       chargeTimer;
    private Resource                    resource;
    private ResourceData                type;
    private SpriteRenderer              spriteRenderer;
    private Tweener.BaseInterpolator    scaleEffect;
    private Vector3                     originalScale;

    void Start()
    {
        chargeTimer = 0;
        originalScale = transform.localScale;

        spriteRenderer = GetComponent<SpriteRenderer>();
        SelectType();
    }

    void SelectType()
    {
        type = allowedTypes.Get();
        spriteRenderer.sprite = type.nodeImage;
    }

    // Update is called once per frame
    void Update()
    {
        if (resource == null)
        {
            if ((unlimitedCharges) || (nCharges > 0))
            {
                chargeTimer -= Time.deltaTime;
            }
            if (chargeTimer <= 0.0f)
            {
                if (randomizeOnEachSpawn) SelectType();
                resource = Instantiate(resourcePrefab, spawnPoint.position, spawnPoint.rotation, transform);
                resource.SetType(type);
                resource.SetNode(this);
                chargeTimer = chargeTime;
                if (!unlimitedCharges) nCharges--;

                transform.ScaleTo(originalScale, 0.2f).EaseFunction(Ease.OutBack);
            }
            else
            {
                if ((transform.localScale.x == 1.0f) && ((scaleEffect == null) || (scaleEffect.isFinished)))
                {
                    scaleEffect = transform.ScaleTo(Vector3.zero, 0.2f);
                }
            }
        }
    }

    internal void Release()
    {
        resource = null;
    }
}
